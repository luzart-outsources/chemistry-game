using System;
using System.Collections.Generic;
using System.Linq;
using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChemistryGame.EditorTools.ContentEditor
{
    /// <summary>
    /// All-in-one content authoring window for designers.
    /// 3 tabs (Substances/Reactions/Levels) × 3 columns (List / Form / Validation+Actions).
    /// </summary>
    public class ContentEditorWindow : EditorWindow
    {
        // ---- Persisted state (survives domain reload) ----
        [SerializeField] private int    _tab;
        [SerializeField] private string _selectedGuid;
        [SerializeField] private string _searchText = "";
        [SerializeField] private bool   _overlayOpen;

        // ---- Runtime state (rebuilt on enable) ----
        private AssetIndex _index;
        private SerializedObject _targetSO;
        private Object _selectedAsset;
        private List<ValidationIssue> _issues = new List<ValidationIssue>();
        private Vector2 _listScroll, _detailScroll, _sideScroll;

        private SubstanceData _draftSubstance;
        private Action<SubstanceData> _overlayOnCreated;

        private static readonly string[] TabNames = { "Substances", "Reactions", "Levels" };

        [MenuItem("ChemistryGame/Open Content Editor")]
        public static void Open()
        {
            var w = GetWindow<ContentEditorWindow>("Content Editor");
            w.minSize = new Vector2(1100, 620);
            w.Show();
        }

        private void OnEnable()
        {
            _index = new AssetIndex();
            AssetIndexPostprocessor.Active = _index;
            _index.OnIndexChanged += OnIndexChanged;
            RestoreSelection();
        }

        private void OnDisable()
        {
            if (_index != null) _index.OnIndexChanged -= OnIndexChanged;
            if (AssetIndexPostprocessor.Active == _index) AssetIndexPostprocessor.Active = null;
        }

        private void OnDestroy()
        {
            if (_targetSO != null && _targetSO.hasModifiedProperties)
            {
                var choice = EditorUtility.DisplayDialogComplex(
                    "Content Editor — chưa lưu",
                    "Bạn có thay đổi chưa lưu. Lưu trước khi đóng?",
                    "Lưu", "Bỏ", "Huỷ đóng");
                if (choice == 0)
                {
                    _targetSO.ApplyModifiedProperties();
                    AssetWriter.SaveAll();
                }
                else if (choice == 2)
                {
                    EditorApplication.delayCall += Open;
                }
            }
        }

        private void OnIndexChanged()
        {
            // The selected asset might have been deleted externally — re-resolve via GUID.
            if (!string.IsNullOrEmpty(_selectedGuid))
            {
                var path = AssetDatabase.GUIDToAssetPath(_selectedGuid);
                if (string.IsNullOrEmpty(path))
                {
                    _selectedAsset = null;
                    _targetSO = null;
                }
            }
            Repaint();
        }

        private void OnGUI()
        {
            DrawTabBar();
            EditorGUILayout.Space(2);

            var totalW = position.width;
            var leftW = totalW * 0.25f;
            var centerW = totalW * 0.50f;
            var rightW = totalW * 0.25f;

            using (new EditorGUI.DisabledScope(_overlayOpen))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    DrawListColumn(leftW);
                    DrawDetailColumn(centerW);
                    DrawSideColumn(rightW);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (_overlayOpen)
            {
                InlineCreateOverlayDrawer.Draw(new Rect(0, 0, position.width, position.height), _draftSubstance, out var save, out var cancel);
                if (save)
                {
                    var newSub = AssetWriter.CreateSubstance(_draftSubstance.Id, s =>
                    {
                        s.Formula     = _draftSubstance.Formula;
                        s.DisplayName = _draftSubstance.DisplayName;
                        s.Category    = _draftSubstance.Category;
                        s.Phase       = _draftSubstance.Phase;
                        s.PH          = _draftSubstance.PH;
                        s.VisualColor = _draftSubstance.VisualColor;
                    });
                    _index.Invalidate();
                    _overlayOnCreated?.Invoke(newSub);
                    CloseOverlay();
                }
                else if (cancel)
                {
                    CloseOverlay();
                }
            }
        }

        private void DrawTabBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            for (int i = 0; i < TabNames.Length; i++)
            {
                int count = i switch
                {
                    0 => _index.GetAllSubstances().Count,
                    1 => _index.GetAllReactions().Count,
                    _ => _index.GetAllLevels().Count,
                };
                if (GUILayout.Toggle(_tab == i, $"{TabNames[i]} ({count})", EditorStyles.toolbarButton) && _tab != i)
                {
                    _tab = i;
                    SetSelection(null);
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(60)))
                CreateNew();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawListColumn(float w)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(w));
            EditorGUILayout.LabelField(TabNames[_tab], EditorStyles.boldLabel);
            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            DrawList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailColumn(float w)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(w));
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            if (_selectedAsset == null)
                EditorGUILayout.HelpBox("Chọn 1 item ở cột trái hoặc bấm + New để tạo.", MessageType.Info);
            else
                DrawDetailForm();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSideColumn(float w)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(w));
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            _sideScroll = EditorGUILayout.BeginScrollView(_sideScroll);
            if (_selectedAsset != null)
            {
                RebuildIssues();
                ValidationPanelDrawer.Draw(_issues);
                QuickActionsPanelDrawer.Draw(_selectedAsset, _index, SetSelection, () => _index.Invalidate());
            }
            else
            {
                EditorGUILayout.LabelField("(chọn 1 asset)", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawList()
        {
            switch (_tab)
            {
                case 0:
                    if (ListPanelDrawer.Draw(_index.GetAllSubstances(), _selectedAsset as SubstanceData, _searchText,
                            s => string.IsNullOrEmpty(s.Id) ? s.name : $"{s.Id}  <color=#888>({s.Phase})</color>",
                            out var sub)) SetSelection(sub);
                    break;
                case 1:
                    if (ListPanelDrawer.Draw(_index.GetAllReactions(), _selectedAsset as ReactionRule, _searchText,
                            r => string.IsNullOrEmpty(r.Id) ? r.name : r.Id, out var rx)) SetSelection(rx);
                    break;
                case 2:
                    if (ListPanelDrawer.Draw(_index.GetAllLevels(), _selectedAsset as LevelConfig, _searchText,
                            l => $"L{l.LevelIndex:D2} — {l.DisplayName}", out var lv)) SetSelection(lv);
                    break;
            }
        }

        private void DrawDetailForm()
        {
            switch (_tab)
            {
                case 0:
                    SubstanceFormDrawer.Draw(_targetSO, _selectedAsset as SubstanceData, _index, NavigateTo);
                    break;
                case 1:
                    ReactionFormDrawer.Draw(_targetSO, _selectedAsset as ReactionRule, _index, OpenInlineCreate, NavigateTo);
                    break;
                case 2:
                    LevelFormDrawer.Draw(_targetSO, _selectedAsset as LevelConfig, _index, OpenInlineCreate, NavigateTo);
                    break;
            }
        }

        private void RebuildIssues()
        {
            _issues = _selectedAsset switch
            {
                SubstanceData s => Validator.ValidateSubstance(s, _index),
                ReactionRule r  => Validator.ValidateReaction(r, _index),
                LevelConfig l   => Validator.ValidateLevel(l, _index),
                _               => new List<ValidationIssue>(),
            };
        }

        private void CreateNew()
        {
            Object created = null;
            switch (_tab)
            {
                case 0:
                {
                    var id = $"NewSub_{UnixTs()}";
                    created = AssetWriter.CreateSubstance(id);
                    break;
                }
                case 1:
                {
                    var id = $"NewRx_{UnixTs()}";
                    created = AssetWriter.CreateReaction(id);
                    break;
                }
                case 2:
                {
                    int idx = _index.GetAllLevels().Select(x => x.LevelIndex).DefaultIfEmpty(0).Max() + 1;
                    created = AssetWriter.CreateLevel(idx, $"New Level {idx}");
                    break;
                }
            }
            _index.Invalidate();
            if (created != null) SetSelection(created);
        }

        private void SetSelection(Object asset)
        {
            _selectedAsset = asset;
            _selectedGuid  = asset == null ? null : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            _targetSO      = asset == null ? null : new SerializedObject(asset);
            _issues.Clear();
            Repaint();
        }

        private void NavigateTo(Object asset)
        {
            if (asset == null) return;
            if      (asset is SubstanceData) _tab = 0;
            else if (asset is ReactionRule)  _tab = 1;
            else if (asset is LevelConfig)   _tab = 2;
            SetSelection(asset);
        }

        private void RestoreSelection()
        {
            if (string.IsNullOrEmpty(_selectedGuid)) return;
            var path = AssetDatabase.GUIDToAssetPath(_selectedGuid);
            if (string.IsNullOrEmpty(path)) return;
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null) SetSelection(asset);
        }

        // ---- Inline create overlay ----

        public void OpenInlineCreate(string prefilledId, Action<SubstanceData> onCreated)
        {
            _overlayOpen = true;
            _draftSubstance = ScriptableObject.CreateInstance<SubstanceData>();
            _draftSubstance.Id = prefilledId ?? "";
            _draftSubstance.Phase = SubstancePhase.Aqueous;
            _draftSubstance.PH = 7f;
            _draftSubstance.VisualColor = new Color(0.6f, 0.85f, 1f, 0.8f);
            _overlayOnCreated = onCreated;
            Repaint();
        }

        private void CloseOverlay()
        {
            _overlayOpen = false;
            if (_draftSubstance != null) DestroyImmediate(_draftSubstance);
            _draftSubstance = null;
            _overlayOnCreated = null;
            Repaint();
        }

        private static string UnixTs()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }
    }
}
