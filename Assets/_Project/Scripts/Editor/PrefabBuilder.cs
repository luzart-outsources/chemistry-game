#if UNITY_EDITOR
using ChemistryGame.Chemistry;
using ChemistryGame.Core;
using ChemistryGame.Gameplay;
using ChemistryGame.UI;
using Luzart;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.EditorTools
{
    /// <summary>
    /// Builds tất cả prefabs cho game: UI screens, popups, gameplay items.
    /// Idempotent: overwrite nếu đã tồn tại.
    /// </summary>
    public static class PrefabBuilder
    {
        private const string UI_DIR = "Assets/_Project/Prefabs/UI";
        private const string GP_DIR = "Assets/_Project/Prefabs/Gameplay";

        // ===== HELPERS =====
        private static GameObject MakeUI(string name, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private static RectTransform Anchor(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
            return rt;
        }

        private static Image AddImage(GameObject go, Color color, Sprite sprite = null)
        {
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.color = color;
            if (sprite != null) img.sprite = sprite;
            img.raycastTarget = true;
            return img;
        }

        private static TMP_Text AddText(GameObject parent, string text, int size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center,
            Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin ?? Vector2.zero;
            rt.anchorMax = anchorMax ?? Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.color = color;
            t.alignment = align;
            t.enableWordWrapping = true;
            return t;
        }

        private static Button MakeButton(string name, Transform parent, string label, Color bgColor,
            Vector2 pos, Vector2 size)
        {
            var go = MakeUI(name, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            AddImage(go, bgColor);
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.15f;
            colors.pressedColor = bgColor * 0.85f;
            btn.colors = colors;
            AddText(go, label, 32, Color.white);
            return btn;
        }

        private static GameObject SavePrefab(GameObject go, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        [MenuItem("ChemistryGame/Build/All Prefabs")]
        public static void BuildAll()
        {
            var bottlePrefab = BuildBottlePrefab();
            var toolPrefab   = BuildToolButtonPrefab();
            var tubePrefab   = BuildTestTubePrefab();
            var cardPrefab   = BuildLevelCardPrefab();

            var menu       = BuildMainMenuPrefab();
            var levelSel   = BuildLevelSelectPrefab(cardPrefab);
            var gameplay   = BuildGameplayPrefab(bottlePrefab, toolPrefab, tubePrefab);
            var pause      = BuildPausePopupPrefab();
            var result     = BuildResultPopupPrefab();
            var wrong      = BuildWrongPopupPrefab();
            var hint       = BuildHintPopupPrefab();
            var settings   = BuildSettingsPopupPrefab();
            var toast      = BuildToastPrefab();

            // Register in UIRegistry
            RegisterAll(menu, levelSel, gameplay, pause, result, wrong, hint, settings, toast);

            // Link GameManager.levels with all LevelConfig assets
            LinkGameManagerLevels();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PrefabBuilder] ALL prefabs built and registered.");
        }

        // ===== Bottle =====
        public static GameObject BuildBottlePrefab()
        {
            var root = new GameObject("DraggableBottle", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 180);
            var bodyImg = root.GetComponent<Image>();
            bodyImg.color = new Color(0.95f, 0.95f, 0.95f, 0.4f);

            // Liquid inside
            var liquidGo = MakeUI("Liquid", root.transform);
            var lrt = Anchor(liquidGo, new Vector2(0.15f, 0.05f), new Vector2(0.85f, 0.85f), Vector2.zero, Vector2.zero);
            var liqImg = AddImage(liquidGo, new Color(0.4f, 0.8f, 1f, 0.7f));
            liqImg.type = Image.Type.Filled;
            liqImg.fillMethod = Image.FillMethod.Vertical;
            liqImg.fillOrigin = (int)Image.OriginVertical.Bottom;
            liqImg.fillAmount = 1f;

            // Label
            var labelGo = MakeUI("Label", root.transform);
            Anchor(labelGo, new Vector2(0f, 0.85f), new Vector2(1f, 1.05f), Vector2.zero, Vector2.zero);
            var labelT = AddText(labelGo, "?", 22, Color.black);

            // Amount
            var amtGo = MakeUI("Amount", root.transform);
            Anchor(amtGo, new Vector2(0f, -0.15f), new Vector2(1f, 0f), Vector2.zero, Vector2.zero);
            var amtT = AddText(amtGo, "50", 18, Color.white);

            var comp = root.AddComponent<DraggableBottle>();
            var soComp = new SerializedObject(comp);
            soComp.FindProperty("canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            soComp.FindProperty("rect").objectReferenceValue = rt;
            soComp.FindProperty("bodyImage").objectReferenceValue = bodyImg;
            soComp.FindProperty("liquidImage").objectReferenceValue = liqImg;
            soComp.FindProperty("labelText").objectReferenceValue = labelT;
            soComp.FindProperty("amountText").objectReferenceValue = amtT;
            soComp.ApplyModifiedProperties();

            return SavePrefab(root, $"{GP_DIR}/DraggableBottle.prefab");
        }

        // ===== Tool button =====
        public static GameObject BuildToolButtonPrefab()
        {
            var root = new GameObject("ToolButton", typeof(RectTransform), typeof(Image), typeof(Button));
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 80);
            AddImage(root, new Color(0.32f, 0.42f, 0.55f, 0.95f));
            var btn = root.GetComponent<Button>();
            var labelT = AddText(root, "Tool", 22, Color.white);
            // Icon child
            var iconGo = MakeUI("Icon", root.transform);
            Anchor(iconGo, new Vector2(0.05f, 0.15f), new Vector2(0.35f, 0.85f), Vector2.zero, Vector2.zero);
            var iconImg = AddImage(iconGo, Color.white);

            var comp = root.AddComponent<ToolButton>();
            var so = new SerializedObject(comp);
            so.FindProperty("icon").objectReferenceValue = iconImg;
            so.FindProperty("label").objectReferenceValue = labelT;
            so.FindProperty("button").objectReferenceValue = btn;
            so.ApplyModifiedProperties();
            return SavePrefab(root, $"{GP_DIR}/ToolButton.prefab");
        }

        // ===== Test tube =====
        public static GameObject BuildTestTubePrefab()
        {
            var root = new GameObject("TestTube", typeof(RectTransform), typeof(Image), typeof(DroppableTube));
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220, 520);
            AddImage(root, new Color(1f, 1f, 1f, 0.15f));

            // Glass outline (BG)
            var outline = MakeUI("Glass", root.transform);
            Anchor(outline, Vector2.zero, Vector2.one, new Vector2(8, 8), new Vector2(-8, -8));
            var oimg = AddImage(outline, new Color(0.7f, 0.85f, 0.95f, 0.2f));

            // Liquid fill
            var liquidGo = MakeUI("LiquidFill", root.transform);
            Anchor(liquidGo, new Vector2(0.12f, 0.02f), new Vector2(0.88f, 0.92f), Vector2.zero, Vector2.zero);
            var liqImg = AddImage(liquidGo, new Color(0.4f, 0.8f, 1f, 0.7f));
            liqImg.type = Image.Type.Filled;
            liqImg.fillMethod = Image.FillMethod.Vertical;
            liqImg.fillOrigin = (int)Image.OriginVertical.Bottom;
            liqImg.fillAmount = 0f;
            liqImg.raycastTarget = false;

            // Surface
            var surfaceGo = MakeUI("Surface", root.transform);
            Anchor(surfaceGo, new Vector2(0.12f, 0.5f), new Vector2(0.88f, 0.55f), Vector2.zero, Vector2.zero);
            var surfImg = AddImage(surfaceGo, new Color(1f, 1f, 1f, 0.4f));
            surfImg.raycastTarget = false;

            // Add LiquidView
            var lv = root.AddComponent<LiquidView>();
            var so = new SerializedObject(lv);
            so.FindProperty("liquidFill").objectReferenceValue = liqImg;
            so.FindProperty("surfaceImage").objectReferenceValue = surfImg;
            so.ApplyModifiedProperties();

            // DroppableTube link
            var dt = root.GetComponent<DroppableTube>();
            var dtSo = new SerializedObject(dt);
            dtSo.FindProperty("liquidView").objectReferenceValue = lv;
            dtSo.ApplyModifiedProperties();

            // Add FXPlayer anchor
            var fxAnchor = MakeUI("FXAnchor", root.transform);
            Anchor(fxAnchor, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-2, -10), new Vector2(2, 10));

            var fxp = root.AddComponent<FXPlayer>();
            var fxSo = new SerializedObject(fxp);
            fxSo.FindProperty("spawnAnchor").objectReferenceValue = fxAnchor.transform;
            fxSo.ApplyModifiedProperties();

            return SavePrefab(root, $"{GP_DIR}/TestTube.prefab");
        }

        // ===== Level Card =====
        public static GameObject BuildLevelCardPrefab()
        {
            var root = new GameObject("LevelCard", typeof(RectTransform), typeof(Image), typeof(Button));
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 320);
            AddImage(root, new Color(0.22f, 0.3f, 0.45f, 1f));
            var btn = root.GetComponent<Button>();
            var levelGo = MakeUI("LevelLabel", root.transform);
            Anchor(levelGo, new Vector2(0, 0.8f), new Vector2(1, 1f), Vector2.zero, Vector2.zero);
            var levelLabel = AddText(levelGo, "Màn ?", 38, Color.white);

            var objGo = MakeUI("Objective", root.transform);
            Anchor(objGo, new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.75f), Vector2.zero, Vector2.zero);
            var objLabel = AddText(objGo, "Mục tiêu", 22, new Color(0.9f, 0.9f, 0.95f), TextAlignmentOptions.Center);

            // Stars (3 images)
            var stars = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var sGo = MakeUI($"Star{i}", root.transform);
                var srt = Anchor(sGo, new Vector2(0.2f + i * 0.2f, 0.1f), new Vector2(0.32f + i * 0.2f, 0.28f), Vector2.zero, Vector2.zero);
                stars[i] = AddImage(sGo, new Color(0.4f, 0.4f, 0.45f));
            }

            // Lock overlay
            var lockGo = MakeUI("Lock", root.transform);
            Anchor(lockGo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(lockGo, new Color(0, 0, 0, 0.6f));
            AddText(lockGo, "🔒 KHOÁ", 36, Color.white);
            lockGo.SetActive(false);

            var comp = root.AddComponent<LevelCard>();
            var so = new SerializedObject(comp);
            so.FindProperty("levelLabel").objectReferenceValue = levelLabel;
            so.FindProperty("objectiveText").objectReferenceValue = objLabel;
            so.FindProperty("button").objectReferenceValue = btn;
            so.FindProperty("lockedOverlay").objectReferenceValue = lockGo;
            var starsProp = so.FindProperty("stars");
            starsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                starsProp.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
            so.ApplyModifiedProperties();

            return SavePrefab(root, $"{UI_DIR}/LevelCard.prefab");
        }

        // ===== Main Menu =====
        public static GameObject BuildMainMenuPrefab()
        {
            var root = new GameObject("MainMenu", typeof(RectTransform), typeof(CanvasGroup));
            Anchor(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgGo = MakeUI("BG", root.transform);
            Anchor(bgGo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(bgGo, new Color(0.08f, 0.12f, 0.2f, 0.95f));

            // Title
            var titleGo = MakeUI("Title", root.transform);
            Anchor(titleGo, new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);
            var titleT = AddText(titleGo, "ỐNG NGHIỆM", 110, new Color(1f, 0.95f, 0.7f));
            titleT.fontStyle = FontStyles.Bold;

            var tagGo = MakeUI("Tagline", root.transform);
            Anchor(tagGo, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.65f), Vector2.zero, Vector2.zero);
            AddText(tagGo, "Hoá học để chơi, không phải để học thuộc.", 28, new Color(0.85f, 0.9f, 1f));

            // Buttons
            var playBtn = MakeButton("PlayButton", root.transform, "▶  Chơi", new Color(0.32f, 0.7f, 0.4f), new Vector2(0, 80), new Vector2(360, 90));
            var setBtn  = MakeButton("SettingsButton", root.transform, "⚙  Cài đặt", new Color(0.4f, 0.5f, 0.7f), new Vector2(0, -30), new Vector2(360, 80));
            var quitBtn = MakeButton("QuitButton", root.transform, "Thoát", new Color(0.55f, 0.3f, 0.3f), new Vector2(0, -130), new Vector2(360, 80));

            // Stars total
            var starsGo = MakeUI("TotalStars", root.transform);
            Anchor(starsGo, new Vector2(0.7f, 0.92f), new Vector2(0.98f, 0.99f), Vector2.zero, Vector2.zero);
            var starsT = AddText(starsGo, "0 / 15 ⭐", 32, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Right);

            // Version
            var verGo = MakeUI("Version", root.transform);
            Anchor(verGo, new Vector2(0.01f, 0.01f), new Vector2(0.2f, 0.06f), Vector2.zero, Vector2.zero);
            var verT = AddText(verGo, "v0.1", 22, new Color(0.7f, 0.7f, 0.8f), TextAlignmentOptions.Left);

            // Diploma badge
            var dipGo = MakeUI("DiplomaBadge", root.transform);
            Anchor(dipGo, new Vector2(0.05f, 0.85f), new Vector2(0.25f, 0.95f), Vector2.zero, Vector2.zero);
            AddImage(dipGo, new Color(0.9f, 0.7f, 0.2f, 0.9f));
            AddText(dipGo, "🎓 Lab Diploma", 22, Color.black);
            dipGo.SetActive(false);

            // Add component
            var ui = root.AddComponent<MainMenuUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("settingsButton").objectReferenceValue = setBtn;
            so.FindProperty("quitButton").objectReferenceValue = quitBtn;
            so.FindProperty("totalStarsText").objectReferenceValue = starsT;
            so.FindProperty("titleText").objectReferenceValue = titleT;
            so.FindProperty("versionText").objectReferenceValue = verT;
            so.FindProperty("diplomaBadge").objectReferenceValue = dipGo;
            so.FindProperty("canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            so.ApplyModifiedProperties();

            return SavePrefab(root, $"{UI_DIR}/MainMenu.prefab");
        }

        // ===== Level Select =====
        public static GameObject BuildLevelSelectPrefab(GameObject cardPrefab)
        {
            var root = new GameObject("LevelSelect", typeof(RectTransform), typeof(CanvasGroup));
            Anchor(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bg = MakeUI("BG", root.transform);
            Anchor(bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(bg, new Color(0.08f, 0.12f, 0.2f, 0.95f));

            // Header
            var headerGo = MakeUI("Header", root.transform);
            Anchor(headerGo, new Vector2(0, 0.88f), new Vector2(1, 1f), Vector2.zero, Vector2.zero);
            AddText(headerGo, "CHỌN MÀN CHƠI", 60, new Color(1f, 0.95f, 0.7f));

            // Total stars
            var starsGo = MakeUI("TotalStars", root.transform);
            Anchor(starsGo, new Vector2(0.78f, 0.9f), new Vector2(0.98f, 0.97f), Vector2.zero, Vector2.zero);
            var starsT = AddText(starsGo, "0 / 15 ⭐", 30, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Right);

            // Cards root: horizontal layout
            var cardsGo = MakeUI("CardsRoot", root.transform);
            var cardsRt = Anchor(cardsGo, new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.85f), Vector2.zero, Vector2.zero);
            var hlg = cardsGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

            // Back button
            var backBtn = MakeButton("BackButton", root.transform, "← Quay lại", new Color(0.4f, 0.5f, 0.7f), new Vector2(-700, -460), new Vector2(220, 70));

            var ui = root.AddComponent<LevelSelectUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("cardsRoot").objectReferenceValue = cardsRt;
            so.FindProperty("cardPrefab").objectReferenceValue = cardPrefab.GetComponent<LevelCard>();
            so.FindProperty("backButton").objectReferenceValue = backBtn;
            so.FindProperty("totalStarsText").objectReferenceValue = starsT;
            so.FindProperty("canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            so.ApplyModifiedProperties();

            return SavePrefab(root, $"{UI_DIR}/LevelSelect.prefab");
        }

        // ===== Gameplay =====
        public static GameObject BuildGameplayPrefab(GameObject bottlePrefab, GameObject toolPrefab, GameObject tubePrefab)
        {
            var root = new GameObject("Gameplay", typeof(RectTransform), typeof(CanvasGroup));
            Anchor(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // BG
            var bg = MakeUI("BG", root.transform);
            Anchor(bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(bg, new Color(0.12f, 0.16f, 0.22f, 1f));

            // Top bar (9%)
            var topGo = MakeUI("TopBar", root.transform);
            Anchor(topGo, new Vector2(0, 0.91f), new Vector2(1, 1f), Vector2.zero, Vector2.zero);
            AddImage(topGo, new Color(0.15f, 0.2f, 0.3f, 0.95f));

            var pauseBtn = MakeButton("PauseButton", topGo.transform, "⏸", new Color(0.5f, 0.55f, 0.65f), new Vector2(-880, 0), new Vector2(80, 70));
            var lvlGo = MakeUI("LevelLabel", topGo.transform);
            Anchor(lvlGo, new Vector2(0.05f, 0.1f), new Vector2(0.2f, 0.9f), Vector2.zero, Vector2.zero);
            var lvlT = AddText(lvlGo, "Màn 1", 32, Color.white, TextAlignmentOptions.Left);

            var objGo = MakeUI("ObjectiveText", topGo.transform);
            Anchor(objGo, new Vector2(0.2f, 0.1f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            var objT = AddText(objGo, "Mục tiêu...", 26, new Color(1f, 0.95f, 0.75f));

            // Left (Inventory) 18%
            var leftGo = MakeUI("InventoryPanel", root.transform);
            Anchor(leftGo, new Vector2(0, 0.13f), new Vector2(0.18f, 0.91f), Vector2.zero, Vector2.zero);
            AddImage(leftGo, new Color(0.18f, 0.24f, 0.32f, 0.85f));

            var bottlesGo = MakeUI("BottlesRoot", leftGo.transform);
            var bottlesRt = Anchor(bottlesGo, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
            var vlg = bottlesGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12; vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = false; vlg.childForceExpandHeight = false;

            // Right (State Panel) 18%
            var rightGo = MakeUI("StatePanel", root.transform);
            Anchor(rightGo, new Vector2(0.82f, 0.13f), new Vector2(1f, 0.91f), Vector2.zero, Vector2.zero);
            AddImage(rightGo, new Color(0.18f, 0.24f, 0.32f, 0.85f));

            var phGo = MakeUI("PhText", rightGo.transform);
            Anchor(phGo, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero);
            var phT = AddText(phGo, "pH: 7", 28, Color.white, TextAlignmentOptions.Left);

            var tempGo = MakeUI("TempText", rightGo.transform);
            Anchor(tempGo, new Vector2(0.05f, 0.8f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);
            var tempT = AddText(tempGo, "T°: 25°C", 28, Color.white, TextAlignmentOptions.Left);

            var contentsGo = MakeUI("ContentsText", rightGo.transform);
            Anchor(contentsGo, new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.8f), Vector2.zero, Vector2.zero);
            var conT = AddText(contentsGo, "(rỗng)", 22, new Color(0.85f, 0.95f, 1f), TextAlignmentOptions.TopLeft);

            var logGo = MakeUI("LogText", rightGo.transform);
            Anchor(logGo, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.45f), Vector2.zero, Vector2.zero);
            var logT = AddText(logGo, "", 18, new Color(0.7f, 0.78f, 0.88f), TextAlignmentOptions.TopLeft);

            // Tools row (top of inventory area or bottom)
            var toolsGo = MakeUI("ToolsRoot", root.transform);
            var toolsRt = Anchor(toolsGo, new Vector2(0.18f, 0.13f), new Vector2(0.82f, 0.23f), Vector2.zero, Vector2.zero);
            AddImage(toolsGo, new Color(0.15f, 0.2f, 0.3f, 0.7f));
            var hlg = toolsGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

            // Center workspace (Tube)
            var wsGo = MakeUI("Workspace", root.transform);
            Anchor(wsGo, new Vector2(0.2f, 0.23f), new Vector2(0.8f, 0.9f), Vector2.zero, Vector2.zero);

            // Place tube prefab as instance
            var tubeGo = (GameObject)PrefabUtility.InstantiatePrefab(tubePrefab, wsGo.transform);
            var tubeRt = tubeGo.GetComponent<RectTransform>();
            tubeRt.anchorMin = new Vector2(0.5f, 0.5f);
            tubeRt.anchorMax = new Vector2(0.5f, 0.5f);
            tubeRt.anchoredPosition = new Vector2(0, -20);

            // Reaction feed text
            var feedGo = MakeUI("ReactionFeed", wsGo.transform);
            Anchor(feedGo, new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero);
            var feedT = AddText(feedGo, "", 26, new Color(0.9f, 1f, 0.85f));

            // Bottom bar (13%)
            var bottomGo = MakeUI("BottomBar", root.transform);
            Anchor(bottomGo, new Vector2(0, 0), new Vector2(1, 0.13f), Vector2.zero, Vector2.zero);
            AddImage(bottomGo, new Color(0.15f, 0.2f, 0.3f, 0.95f));

            var hintBtn   = MakeButton("HintButton",   bottomGo.transform, "💡 Gợi ý", new Color(0.5f, 0.4f, 0.75f), new Vector2(-700, 0), new Vector2(220, 90));
            var undoBtn   = MakeButton("UndoButton",   bottomGo.transform, "↶ Undo",   new Color(0.5f, 0.55f, 0.65f), new Vector2(-450, 0), new Vector2(200, 90));
            var restartBtn= MakeButton("RestartBtn",   bottomGo.transform, "↻ Restart",new Color(0.6f, 0.4f, 0.4f), new Vector2(-220, 0), new Vector2(220, 90));
            var submitBtn = MakeButton("SubmitButton", bottomGo.transform, "✓ Nộp bài", new Color(0.32f, 0.72f, 0.4f), new Vector2(700, 0), new Vector2(260, 100));

            // GameplayController + StatePanel components on root
            var spComp = wsGo.AddComponent<StatePanel>();
            var spSo = new SerializedObject(spComp);
            spSo.FindProperty("phText").objectReferenceValue = phT;
            spSo.FindProperty("tempText").objectReferenceValue = tempT;
            spSo.FindProperty("contentsText").objectReferenceValue = conT;
            spSo.FindProperty("logText").objectReferenceValue = logT;
            spSo.ApplyModifiedProperties();

            var gp = root.AddComponent<GameplayController>();
            var liquidView = tubeGo.GetComponent<LiquidView>();
            var droppable = tubeGo.GetComponent<DroppableTube>();
            var fxPlayer = tubeGo.GetComponent<FXPlayer>();
            var gpSo = new SerializedObject(gp);
            gpSo.FindProperty("mainTube").objectReferenceValue = liquidView;
            gpSo.FindProperty("tube").objectReferenceValue = droppable;
            gpSo.FindProperty("bottlesRoot").objectReferenceValue = bottlesRt;
            gpSo.FindProperty("toolsRoot").objectReferenceValue = toolsRt;
            gpSo.FindProperty("statePanel").objectReferenceValue = spComp;
            gpSo.FindProperty("fxPlayer").objectReferenceValue = fxPlayer;
            gpSo.FindProperty("bottlePrefab").objectReferenceValue = bottlePrefab.GetComponent<DraggableBottle>();
            gpSo.FindProperty("toolButtonPrefab").objectReferenceValue = toolPrefab.GetComponent<ToolButton>();
            gpSo.FindProperty("objectiveText").objectReferenceValue = objT;
            gpSo.FindProperty("reactionFeedText").objectReferenceValue = feedT;
            gpSo.ApplyModifiedProperties();

            // GameplayHUD
            var hud = root.AddComponent<GameplayHUD>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("levelLabel").objectReferenceValue = lvlT;
            hudSo.FindProperty("objectiveText").objectReferenceValue = objT;
            hudSo.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
            hudSo.FindProperty("submitButton").objectReferenceValue = submitBtn;
            hudSo.FindProperty("undoButton").objectReferenceValue = undoBtn;
            hudSo.FindProperty("restartButton").objectReferenceValue = restartBtn;
            hudSo.FindProperty("hintButton").objectReferenceValue = hintBtn;
            hudSo.FindProperty("canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            hudSo.FindProperty("gameplayController").objectReferenceValue = gp;
            hudSo.ApplyModifiedProperties();

            return SavePrefab(root, $"{UI_DIR}/Gameplay.prefab");
        }

        // ===== Popups =====
        private static (GameObject root, GameObject card, CanvasGroup cg) MakePopupShell(string name, Vector2 cardSize)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            Anchor(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(root, new Color(0, 0, 0, 0.55f));
            var card = MakeUI("Card", root.transform);
            var crt = Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            crt.sizeDelta = cardSize;
            AddImage(card, new Color(0.16f, 0.22f, 0.32f, 0.98f));
            return (root, card, root.GetComponent<CanvasGroup>());
        }

        public static GameObject BuildPausePopupPrefab()
        {
            var (root, card, cg) = MakePopupShell("PausePopup", new Vector2(540, 460));
            var crt = card.GetComponent<RectTransform>();

            var titleGo = MakeUI("Title", card.transform);
            Anchor(titleGo, new Vector2(0, 0.78f), new Vector2(1, 0.95f), Vector2.zero, Vector2.zero);
            AddText(titleGo, "TẠM DỪNG", 48, new Color(1f, 0.95f, 0.7f));

            var resumeBtn = MakeButton("ResumeButton",      card.transform, "▶  Tiếp tục",    new Color(0.32f, 0.7f, 0.4f), new Vector2(0, 80), new Vector2(420, 80));
            var restartBtn= MakeButton("RestartButton",     card.transform, "↻  Chơi lại",    new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -20), new Vector2(420, 80));
            var lsBtn     = MakeButton("LevelSelectButton", card.transform, "Quay về danh sách",new Color(0.55f, 0.3f, 0.3f), new Vector2(0, -120), new Vector2(420, 80));

            var ui = root.AddComponent<PausePopup>();
            var so = new SerializedObject(ui);
            so.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
            so.FindProperty("restartButton").objectReferenceValue = restartBtn;
            so.FindProperty("levelSelectButton").objectReferenceValue = lsBtn;
            so.FindProperty("card").objectReferenceValue = crt;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.ApplyModifiedProperties();
            return SavePrefab(root, $"{UI_DIR}/PausePopup.prefab");
        }

        public static GameObject BuildResultPopupPrefab()
        {
            var (root, card, cg) = MakePopupShell("ResultPopup", new Vector2(720, 540));
            var crt = card.GetComponent<RectTransform>();

            var titleGo = MakeUI("Title", card.transform);
            Anchor(titleGo, new Vector2(0, 0.78f), new Vector2(1, 0.95f), Vector2.zero, Vector2.zero);
            var titleT = AddText(titleGo, "Hoàn thành!", 50, new Color(1f, 0.95f, 0.7f));

            var stars = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var sGo = MakeUI($"Star{i}", card.transform);
                var srt = Anchor(sGo, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, Vector2.zero);
                srt.anchoredPosition = new Vector2(-120 + i * 120, 0);
                srt.sizeDelta = new Vector2(96, 96);
                stars[i] = AddImage(sGo, new Color(0.4f, 0.4f, 0.45f));
            }

            var reasonGo = MakeUI("Reason", card.transform);
            Anchor(reasonGo, new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.45f), Vector2.zero, Vector2.zero);
            var reasonT = AddText(reasonGo, "", 26, new Color(0.85f, 0.95f, 1f));

            var replayBtn= MakeButton("ReplayButton",       card.transform, "↻ Chơi lại",      new Color(0.5f, 0.5f, 0.6f), new Vector2(-180, -180), new Vector2(220, 80));
            var nextBtn  = MakeButton("NextButton",         card.transform, "Màn kế →",        new Color(0.32f, 0.7f, 0.4f), new Vector2(60, -180), new Vector2(220, 80));
            var lsBtn    = MakeButton("LevelSelectButton",  card.transform, "Danh sách",       new Color(0.55f, 0.45f, 0.3f), new Vector2(290, -180), new Vector2(200, 80));

            var ui = root.AddComponent<ResultPopupUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("titleText").objectReferenceValue = titleT;
            so.FindProperty("reasonText").objectReferenceValue = reasonT;
            so.FindProperty("replayButton").objectReferenceValue = replayBtn;
            so.FindProperty("nextButton").objectReferenceValue = nextBtn;
            so.FindProperty("levelSelectButton").objectReferenceValue = lsBtn;
            so.FindProperty("card").objectReferenceValue = crt;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            var arr = so.FindProperty("starImages");
            arr.arraySize = 3;
            for (int i = 0; i < 3; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
            so.ApplyModifiedProperties();
            return SavePrefab(root, $"{UI_DIR}/ResultPopup.prefab");
        }

        public static GameObject BuildWrongPopupPrefab()
        {
            var (root, card, cg) = MakePopupShell("WrongProductPopup", new Vector2(640, 480));
            var crt = card.GetComponent<RectTransform>();

            var titleGo = MakeUI("Title", card.transform);
            Anchor(titleGo, new Vector2(0, 0.78f), new Vector2(1, 0.95f), Vector2.zero, Vector2.zero);
            var titleT = AddText(titleGo, "Sản phẩm chưa đạt", 42, new Color(1f, 0.7f, 0.7f));

            var reasonGo = MakeUI("Reason", card.transform);
            Anchor(reasonGo, new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.75f), Vector2.zero, Vector2.zero);
            var reasonT = AddText(reasonGo, "Sai", 26, new Color(0.9f, 0.95f, 1f));

            var wrongGo = MakeUI("Wrong", card.transform);
            Anchor(wrongGo, new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.45f), Vector2.zero, Vector2.zero);
            var wrongT = AddText(wrongGo, "", 24, new Color(1f, 0.85f, 0.6f));

            var retryBtn = MakeButton("RetryButton",         card.transform, "↻ Thử lại",  new Color(0.5f, 0.55f, 0.7f), new Vector2(-120, -160), new Vector2(220, 80));
            var lsBtn    = MakeButton("LevelSelectButton",   card.transform, "Danh sách",  new Color(0.55f, 0.3f, 0.3f), new Vector2(120, -160), new Vector2(220, 80));

            var ui = root.AddComponent<WrongProductPopupUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("titleText").objectReferenceValue = titleT;
            so.FindProperty("reasonText").objectReferenceValue = reasonT;
            so.FindProperty("wrongProductText").objectReferenceValue = wrongT;
            so.FindProperty("retryButton").objectReferenceValue = retryBtn;
            so.FindProperty("levelSelectButton").objectReferenceValue = lsBtn;
            so.FindProperty("card").objectReferenceValue = crt;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.ApplyModifiedProperties();
            return SavePrefab(root, $"{UI_DIR}/WrongProductPopup.prefab");
        }

        public static GameObject BuildHintPopupPrefab()
        {
            var (root, card, cg) = MakePopupShell("HintPopup", new Vector2(680, 520));
            var crt = card.GetComponent<RectTransform>();

            var titleGo = MakeUI("Title", card.transform);
            Anchor(titleGo, new Vector2(0, 0.82f), new Vector2(1, 0.96f), Vector2.zero, Vector2.zero);
            AddText(titleGo, "GỢI Ý", 44, new Color(0.85f, 0.7f, 1f));

            var btns = new Button[3];
            var labels = new TMP_Text[3];
            string[] tierLabels = { "Mức 1: Thuốc thử (giữ 2⭐)", "Mức 2: Thứ tự (giữ 2⭐)", "Mức 3: Công thức (giảm 1⭐)" };
            for (int i = 0; i < 3; i++)
            {
                btns[i] = MakeButton($"Tier{i + 1}Button", card.transform, tierLabels[i], new Color(0.5f, 0.4f, 0.75f), new Vector2(0, 130 - i * 100), new Vector2(540, 80));
                // Find label component to bind separately if needed
                var t = btns[i].GetComponentInChildren<TMP_Text>();
                labels[i] = t;
            }

            var revealGo = MakeUI("RevealedText", card.transform);
            Anchor(revealGo, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.4f), Vector2.zero, Vector2.zero);
            var revealT = AddText(revealGo, "Chọn mức gợi ý.", 24, new Color(0.9f, 0.95f, 1f), TextAlignmentOptions.Center);

            var closeBtn = MakeButton("CloseButton", card.transform, "Đóng", new Color(0.55f, 0.3f, 0.3f), new Vector2(0, -200), new Vector2(220, 70));

            var ui = root.AddComponent<HintPopupUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("revealedText").objectReferenceValue = revealT;
            so.FindProperty("card").objectReferenceValue = crt;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            var btnsProp = so.FindProperty("tierButtons");
            btnsProp.arraySize = 3;
            var labelsProp = so.FindProperty("tierLabels");
            labelsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                btnsProp.GetArrayElementAtIndex(i).objectReferenceValue = btns[i];
                labelsProp.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
            }
            so.ApplyModifiedProperties();
            return SavePrefab(root, $"{UI_DIR}/HintPopup.prefab");
        }

        public static GameObject BuildSettingsPopupPrefab()
        {
            var (root, card, cg) = MakePopupShell("SettingsPopup", new Vector2(560, 420));
            var crt = card.GetComponent<RectTransform>();

            var titleGo = MakeUI("Title", card.transform);
            Anchor(titleGo, new Vector2(0, 0.82f), new Vector2(1, 0.96f), Vector2.zero, Vector2.zero);
            AddText(titleGo, "CÀI ĐẶT", 42, new Color(1f, 0.95f, 0.7f));

            // Music
            var musicLabelGo = MakeUI("MusicLabel", card.transform);
            Anchor(musicLabelGo, new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.68f), Vector2.zero, Vector2.zero);
            var musicLabelT = AddText(musicLabelGo, "Nhạc: 70%", 26, Color.white, TextAlignmentOptions.Left);

            var musicSliderGo = new GameObject("MusicSlider", typeof(RectTransform), typeof(Slider));
            musicSliderGo.transform.SetParent(card.transform, false);
            var msRt = musicSliderGo.GetComponent<RectTransform>();
            msRt.anchorMin = new Vector2(0.05f, 0.46f); msRt.anchorMax = new Vector2(0.95f, 0.56f);
            msRt.offsetMin = Vector2.zero; msRt.offsetMax = Vector2.zero;
            var msSlider = musicSliderGo.GetComponent<Slider>();
            msSlider.minValue = 0f; msSlider.maxValue = 1f; msSlider.value = 0.7f;
            // Create handle/fill stubs
            BuildSliderVisuals(musicSliderGo, msSlider);

            // SFX
            var sfxLabelGo = MakeUI("SfxLabel", card.transform);
            Anchor(sfxLabelGo, new Vector2(0.05f, 0.31f), new Vector2(0.95f, 0.41f), Vector2.zero, Vector2.zero);
            var sfxLabelT = AddText(sfxLabelGo, "Âm: 85%", 26, Color.white, TextAlignmentOptions.Left);

            var sfxSliderGo = new GameObject("SfxSlider", typeof(RectTransform), typeof(Slider));
            sfxSliderGo.transform.SetParent(card.transform, false);
            var ssRt = sfxSliderGo.GetComponent<RectTransform>();
            ssRt.anchorMin = new Vector2(0.05f, 0.19f); ssRt.anchorMax = new Vector2(0.95f, 0.29f);
            ssRt.offsetMin = Vector2.zero; ssRt.offsetMax = Vector2.zero;
            var ssSlider = sfxSliderGo.GetComponent<Slider>();
            ssSlider.minValue = 0f; ssSlider.maxValue = 1f; ssSlider.value = 0.85f;
            BuildSliderVisuals(sfxSliderGo, ssSlider);

            var closeBtn = MakeButton("CloseButton", card.transform, "Đóng", new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -160), new Vector2(220, 70));

            var ui = root.AddComponent<SettingsPopupUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("musicSlider").objectReferenceValue = msSlider;
            so.FindProperty("sfxSlider").objectReferenceValue = ssSlider;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("musicLabel").objectReferenceValue = musicLabelT;
            so.FindProperty("sfxLabel").objectReferenceValue = sfxLabelT;
            so.FindProperty("card").objectReferenceValue = crt;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.ApplyModifiedProperties();
            return SavePrefab(root, $"{UI_DIR}/SettingsPopup.prefab");
        }

        private static void BuildSliderVisuals(GameObject sliderGo, Slider slider)
        {
            // Background
            var bg = MakeUI("Background", sliderGo.transform);
            Anchor(bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(bg, new Color(0.25f, 0.3f, 0.4f, 1f));
            // Fill Area
            var fillArea = MakeUI("Fill Area", sliderGo.transform);
            Anchor(fillArea, Vector2.zero, Vector2.one, new Vector2(10, 5), new Vector2(-10, -5));
            var fill = MakeUI("Fill", fillArea.transform);
            Anchor(fill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fillImg = AddImage(fill, new Color(0.4f, 0.7f, 0.95f, 1f));
            // Handle
            var handleArea = MakeUI("Handle Slide Area", sliderGo.transform);
            Anchor(handleArea, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var handle = MakeUI("Handle", handleArea.transform);
            var hrt = handle.GetComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(24, 40);
            var handleImg = AddImage(handle, Color.white);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
        }

        public static GameObject BuildTutorialPrefab()
        {
            var (root, card, cg) = MakePopupShell("TutorialOverlay", new Vector2(820, 540));
            var crt = card.GetComponent<RectTransform>();

            var titleGo = MakeUI("Title", card.transform);
            Anchor(titleGo, new Vector2(0, 0.85f), new Vector2(1, 0.97f), Vector2.zero, Vector2.zero);
            AddText(titleGo, "Chào mừng đến phòng lab!", 38, new Color(1f, 0.95f, 0.7f));

            var bodyGo = MakeUI("BodyText", card.transform);
            Anchor(bodyGo, new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.83f), Vector2.zero, Vector2.zero);
            var bodyT = AddText(bodyGo, "", 22, new Color(0.92f, 0.95f, 1f), TextAlignmentOptions.TopLeft);
            bodyT.richText = true;

            var btn = MakeButton("GotItButton", card.transform, "Hiểu rồi!", new Color(0.32f, 0.7f, 0.4f), new Vector2(0, -200), new Vector2(260, 80));

            var ui = root.AddComponent<TutorialOverlay>();
            var so = new SerializedObject(ui);
            so.FindProperty("bodyText").objectReferenceValue = bodyT;
            so.FindProperty("gotItButton").objectReferenceValue = btn;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.FindProperty("card").objectReferenceValue = crt;
            so.ApplyModifiedProperties();
            return SavePrefab(root, $"{UI_DIR}/TutorialOverlay.prefab");
        }

        public static GameObject BuildToastPrefab()
        {
            var root = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup));
            Anchor(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var card = MakeUI("Card", root.transform);
            var crt = Anchor(card, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero, Vector2.zero);
            crt.sizeDelta = new Vector2(620, 100);
            var bg = AddImage(card, new Color(0.2f, 0.3f, 0.45f, 0.95f));
            var msg = AddText(card, "", 26, Color.white);

            var ui = root.AddComponent<ToastUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("messageText").objectReferenceValue = msg;
            so.FindProperty("background").objectReferenceValue = bg;
            so.FindProperty("canvasGroup").objectReferenceValue = root.GetComponent<CanvasGroup>();
            so.FindProperty("card").objectReferenceValue = crt;
            so.ApplyModifiedProperties();
            return SavePrefab(root, $"{UI_DIR}/Toast.prefab");
        }

        // ===== Register all in UIRegistry =====
        private static void RegisterAll(GameObject menu, GameObject levelSel, GameObject gameplay,
            GameObject pause, GameObject result, GameObject wrong, GameObject hint, GameObject settings, GameObject toast)
        {
            var regGuids = AssetDatabase.FindAssets("t:Luzart.UIRegistrySO");
            UIRegistrySO reg;
            if (regGuids.Length == 0)
            {
                reg = ScriptableObject.CreateInstance<UIRegistrySO>();
                AssetDatabase.CreateAsset(reg, "Assets/_Project/ScriptableObjects/UIRegistry.asset");
            }
            else reg = AssetDatabase.LoadAssetAtPath<UIRegistrySO>(AssetDatabase.GUIDToAssetPath(regGuids[0]));

            // Access entries via reflection (private field).
            var fld = typeof(UIRegistrySO).GetField("entries",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = (System.Collections.Generic.List<UIConfig>)fld.GetValue(reg);

            void AddEntry(UIId id, GameObject prefab, UILayer lane, UICachePolicy policy)
            {
                var existing = list.Find(c => c != null && c.Id == id);
                if (existing == null) { existing = new UIConfig(); list.Add(existing); }
                existing.Id = id;
                existing.AssetRef = prefab;
                existing.Lane = lane;
                existing.CachePolicy = policy;
                existing.AllowMultiInstance = false;
                existing.DismissByEscape = true;
                existing.PreloadOnBoot = false;
                existing.StringId = id.ToString();
            }

            AddEntry(UIId.CG_MainMenu,          menu,     UILayer.Screen, UICachePolicy.PoolOnClose);
            AddEntry(UIId.CG_LevelSelect,       levelSel, UILayer.Screen, UICachePolicy.PoolOnClose);
            AddEntry(UIId.CG_Gameplay,          gameplay, UILayer.Screen, UICachePolicy.PoolOnClose);
            AddEntry(UIId.CG_PausePopup,        pause,    UILayer.Popup,  UICachePolicy.PoolOnClose);
            AddEntry(UIId.CG_ResultPopup,       result,   UILayer.Popup,  UICachePolicy.PoolOnClose);
            AddEntry(UIId.CG_WrongProductPopup, wrong,    UILayer.Popup,  UICachePolicy.PoolOnClose);
            AddEntry(UIId.CG_HintPopup,         hint,     UILayer.Popup,  UICachePolicy.PoolOnClose);
            AddEntry(UIId.CG_SettingsPopup,     settings, UILayer.Popup,  UICachePolicy.PoolOnClose);
            // Toast uses Luzart's built-in UIId.Toast (4000)
            AddEntry(UIId.Toast,                toast,    UILayer.Toast,  UICachePolicy.KeepLoaded);

            // Override Toast properties to allow multi-instance
            var toastConfig = list.Find(c => c.Id == UIId.Toast);
            if (toastConfig != null) toastConfig.AllowMultiInstance = false;

            fld.SetValue(reg, list);
            EditorUtility.SetDirty(reg);
        }

        private static void LinkGameManagerLevels()
        {
            // Find GameManager in scene
            var gm = Object.FindObjectOfType<GameManager>();
            if (gm == null) return;
            var levels = new System.Collections.Generic.List<LevelConfig>();
            var guids = AssetDatabase.FindAssets("t:ChemistryGame.Chemistry.LevelConfig");
            System.Array.Sort(guids, (a, b) => {
                var pa = AssetDatabase.GUIDToAssetPath(a);
                var pb = AssetDatabase.GUIDToAssetPath(b);
                return string.Compare(pa, pb);
            });
            foreach (var g in guids)
                levels.Add(AssetDatabase.LoadAssetAtPath<LevelConfig>(AssetDatabase.GUIDToAssetPath(g)));
            var so = new SerializedObject(gm);
            var arr = so.FindProperty("levels");
            arr.arraySize = levels.Count;
            for (int i = 0; i < levels.Count; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(gm);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gm.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gm.gameObject.scene);
        }
    }
}
#endif
