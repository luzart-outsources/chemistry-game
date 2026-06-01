# Chemistry Content Editor Tool — Implementation Plan

> **For agentic workers:** Use superpowers:subagent-driven-development to implement task-by-task. Steps use `- [ ]`.

**Goal:** One Unity `EditorWindow` letting designers CRUD Substances/Reactions/Levels with cross-references, live validation, inline substance creation from reaction picker, bottle preview, and quick actions — no Inspector needed.

**Architecture:** IMGUI `EditorWindow` (`ContentEditorWindow`), 3-column layout × 3 tabs. Drawers are stateless static classes; the window owns all state in `[SerializeField]` for domain-reload survival. Pure-C# `Services` (`AssetIndex`, `Validator`, `AssetWriter`, `QuickActions`) are unit-tested without GUI.

**Spec:** `docs/superpowers/specs/2026-06-01-content-editor-tool-design.md`

**Tech:** Unity (existing project version), C#, `EditorGUILayout`, `SerializedObject`, `AssetPostprocessor`, `com.unity.test-framework 1.1.33`.

---

## Conventions

- Namespace: `ChemistryGame.EditorTools.ContentEditor` (tests: `…ContentEditor.Tests`).
- Tab IDs: `0=Substances, 1=Reactions, 2=Levels`.
- Asset dirs (already used by `ChemistrySeeder`):
  - `Assets/_Project/ScriptableObjects/{Substances,Reactions,Levels}/`
- Existing types (read-only): `ChemistryGame.Chemistry.{SubstanceData,ReactionRule,LevelConfig,BottleSpawn,ToolSpawn,PurityRule,TrapDefinition,HintBundle,ToolData}`. `ChemistryGame.Core.GameManager` (private `List<LevelConfig> levels`).
- Commit prefixes: `feat(editor):`, `test(editor):`, `docs(editor):`.

---

## File Layout

```
Assets/_Project/Scripts/Editor/ChemistryEditor/
├── ChemistryEditor.Editor.asmdef
├── ContentEditorWindow.cs
├── Drawers/{ListPanelDrawer, SubstanceFormDrawer, ReactionFormDrawer,
│           LevelFormDrawer, ValidationPanelDrawer, QuickActionsPanelDrawer,
│           InlineCreateOverlayDrawer, BottlePreviewDrawer}.cs
├── Services/{AssetIndex, AssetIndexPostprocessor, ValidationIssue,
│             Validator, AssetWriter, QuickActions}.cs
└── Util/{SearchableDropdown, RichTextFormatter, EditorStyles_Chemistry}.cs

Assets/_Project/Tests/Editor/ChemistryEditor/
├── ChemistryEditor.Tests.asmdef
├── ValidatorTests.cs
├── AssetIndexTests.cs
└── (QuickActionsTests deferred — needs scene fixture)
```

---

## Task 1 — Asmdef scaffolding

**Files (create):**
- `Assets/_Project/Scripts/Editor/ChemistryEditor/ChemistryEditor.Editor.asmdef`
- `Assets/_Project/Tests/Editor/ChemistryEditor/ChemistryEditor.Tests.asmdef`

- [ ] **1.1** Create editor asmdef:
```json
{
  "name": "ChemistryGame.EditorTools.ContentEditor",
  "rootNamespace": "ChemistryGame.EditorTools.ContentEditor",
  "references": [],
  "includePlatforms": ["Editor"],
  "autoReferenced": true,
  "noEngineReferences": false
}
```
- [ ] **1.2** Create tests asmdef:
```json
{
  "name": "ChemistryGame.EditorTools.ContentEditor.Tests",
  "rootNamespace": "ChemistryGame.EditorTools.ContentEditor.Tests",
  "references": ["ChemistryGame.EditorTools.ContentEditor", "UnityEngine.TestRunner", "UnityEditor.TestRunner"],
  "includePlatforms": ["Editor"],
  "overrideReferences": true,
  "precompiledReferences": ["nunit.framework.dll"],
  "autoReferenced": false,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```
- [ ] **1.3** Open Unity → verify no compile errors.
- [ ] **1.4** Commit: `feat(editor): scaffold ChemistryEditor asmdefs`

---

## Task 2 — `ValidationIssue` + `Severity`

**File (create):** `Services/ValidationIssue.cs`

- [ ] **2.1** Implement with `enum Severity { Error, Warning, Info }`, struct fields `{Severity, string Code, string Message, Action QuickFix, string QuickFixLabel}`, and static factories `Error/Warning/Info`.
- [ ] **2.2** Commit.

---

## Task 3 — `AssetIndex` + `IAssetIndexReadOnly` (TDD)

**Files (create):** `Services/AssetIndex.cs`, `Tests/.../AssetIndexTests.cs`

- [ ] **3.1** Write failing tests for: `GetAllSubstances`, `GetSubstanceById`, `GetReactionsUsingInput`. Tests use `AssetDatabase.CreateFolder("Assets","TempChemTest_<guid>")` for isolation, teardown deletes folder.
- [ ] **3.2** Run → fail (class doesn't exist).
- [ ] **3.3** Define `IAssetIndexReadOnly` interface (GetAll*, GetSubstanceById, GetReactionsUsingInput, GetReactionsProducing, GetLevelsUsingSubstance, GetLevelsUsingReaction).
- [ ] **3.4** Implement `AssetIndex : IAssetIndexReadOnly` with caches:
  - Lists for substances/reactions/levels (loaded via `AssetDatabase.FindAssets("t:SubstanceData")` etc.).
  - Dict `substanceById`.
  - Reverse maps: `reactionsByInput`, `reactionsByOutput`, `levelsBySubstance` (from `Level.Bottles`), `levelsByReaction` (from `Level.AvailableReactions`).
  - `Invalidate()` sets dirty + fires `OnIndexChanged`. `EnsureFresh()` rebuilds if dirty.
- [ ] **3.5** Run tests → pass.
- [ ] **3.6** Commit.

---

## Task 4 — `AssetIndexPostprocessor`

**File (create):** `Services/AssetIndexPostprocessor.cs`

- [ ] **4.1** `internal class AssetIndexPostprocessor : AssetPostprocessor` with `internal static AssetIndex Active;` and `OnPostprocessAllAssets(...)` calling `Active?.Invalidate()`.
- [ ] **4.2** Commit.

---

## Task 5 — `Validator` (TDD)

**Files (create):** `Services/Validator.cs`, `Tests/.../ValidatorTests.cs`

- [ ] **5.1** Tests cover codes: `SUB_ID_EMPTY`, `SUB_ID_DUPLICATE`, `SUB_ORPHAN`, `RX_INPUT_NULL`, `RX_INPUTS_EMPTY`, `LV_TARGET_NULL`, `LV_TARGET_IN_FORBIDDEN`, `LV_INDEX_DUPLICATE`, `LV_TRAP_TRIGGER_NULL`. Use `FakeIndex : IAssetIndexReadOnly` with public mutable lists.
- [ ] **5.2** Implement `Validator` static class with `ValidateSubstance/ValidateReaction/ValidateLevel(asset, IAssetIndexReadOnly)` returning `List<ValidationIssue>`. Rule logic per spec §5; quick-fixes left null in v1 (wired later).
- [ ] **5.3** All tests pass.
- [ ] **5.4** Commit.

---

## Task 6 — `AssetWriter`

**File (create):** `Services/AssetWriter.cs`

- [ ] **6.1** Implement `static class AssetWriter` with:
  - Constants `SubstanceDir/ReactionDir/LevelDir`.
  - `CreateSubstance(id, configure?)`, `CreateReaction(id, configure?)`, `CreateLevel(int idx, displayName)` — all use `AssetDatabase.GenerateUniqueAssetPath`, `CreateAsset`, `Undo.RegisterCreatedObjectUndo`, `SaveAssets`.
  - `Duplicate<T>(T original, string suffix="_Copy")` via `CopyAsset` + `RegisterCreatedObjectUndo`.
  - `DeleteAsset(Object)` via `MoveAssetToTrash`.
  - `MarkDirty(Object)` via `Undo.RegisterCompleteObjectUndo` + `EditorUtility.SetDirty`.
  - Private `EnsureFolder` (recursive `AssetDatabase.CreateFolder`).
- [ ] **6.2** Commit.

---

## Task 7 — `QuickActions`

**File (create):** `Services/QuickActions.cs`

- [ ] **7.1** Implement:
  - `DuplicateSubstance/Reaction/Level` → `AssetWriter.Duplicate`.
  - `AddLevelToGameManager(LevelConfig)`:
    - `MainScenePath = "Assets/_Project/Scenes/Main.unity"`.
    - If scene not loaded → `EditorSceneManager.OpenScene(path, Additive)` (track `openedTemporarily`).
    - Find `GameManager` via `FindObjectsOfType<GameManager>().First(g => g.gameObject.scene == scene)`.
    - Use `SerializedObject` + `FindProperty("levels")` (private field — `SerializedObject` sees it).
    - Skip if already in list. Else `arraySize++`, set `objectReferenceValue`, `ApplyModifiedProperties`, `MarkSceneDirty`, `SaveScene`.
    - If opened temporarily: `EditorSceneManager.CloseScene(scene, true)`.
  - `FindReferences(Object, IAssetIndexReadOnly) → List<Object>` (via index reverse maps).
- [ ] **7.2** Commit.

---

## Task 8 — `ContentEditorWindow` shell

**File (create):** `ContentEditorWindow.cs`

- [ ] **8.1** Implement:
  - `[MenuItem("ChemistryGame/Open Content Editor")] public static void Open()` → `GetWindow<ContentEditorWindow>("Content Editor")` with `minSize=(1100,600)`.
  - `[SerializeField] int _tab; [SerializeField] string _selectedGuid; [SerializeField] string _searchText="";`
  - `OnEnable`: `_index = new AssetIndex(); AssetIndexPostprocessor.Active = _index; _index.OnIndexChanged += Repaint;` + call `RestoreSelection()` (stub for now).
  - `OnDisable`: unsubscribe, clear `AssetIndexPostprocessor.Active`.
  - `OnGUI`: toolbar with 3 tab toggles + `+ New` button → `CreateNew()`. Then `BeginHorizontal` with 3 vertical groups at widths `0.25/0.50/0.25` of `position.width`. Each has its own `BeginScrollView`.
  - Left column draws label + search textfield + `DrawList()`.
  - Center draws `DrawDetailForm()` (HelpBox if `_selectedAsset==null`).
  - Right draws `Validation` label + `DrawSidePanel()`.
  - Switching tab clears `_selectedAsset/_targetSO/_selectedGuid`.
  - Stub methods `DrawList/DrawDetailForm/DrawSidePanel/CreateNew/RestoreSelection` (filled by later tasks).
- [ ] **8.2** Manual: open menu → empty window appears.
- [ ] **8.3** Commit.

---

## Task 9 — `ListPanelDrawer` + wire `DrawList`

**Files:** Create `Drawers/ListPanelDrawer.cs`. Modify `ContentEditorWindow.cs`.

- [ ] **9.1** `static class ListPanelDrawer` with `bool Draw<T>(IReadOnlyList<T> items, T selected, string search, Func<T,string> labelOf, out T selectedOut)` rendering filtered items as `GUILayout.Button` (style switches between `EditorStyles.helpBox` for selected vs `EditorStyles.label`).
- [ ] **9.2** In `ContentEditorWindow.DrawList()`, switch on `_tab`:
  - 0: `ListPanelDrawer.Draw(_index.GetAllSubstances(), _selectedAsset as SubstanceData, _searchText, s => string.IsNullOrEmpty(s.Id)?s.name:$"{s.Id}  ({s.Phase})", out var x)` → `SetSelection(x)`.
  - 1: label `r.Id ?? r.name`.
  - 2: label `$"L{l.LevelIndex:D2} — {l.DisplayName}"`.
- [ ] **9.3** Add `SetSelection(Object)`: stores `_selectedAsset`, computes `_selectedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset))`, sets `_targetSO = new SerializedObject(asset)`.
- [ ] **9.4** Manual: list populates across all 3 tabs; search filters.
- [ ] **9.5** Commit.

---

## Task 10 — `SubstanceFormDrawer` + cross-references

**Files:** Create `Drawers/SubstanceFormDrawer.cs`. Modify `ContentEditorWindow.cs`.

- [ ] **10.1** `static class SubstanceFormDrawer` with `Draw(SerializedObject so, SubstanceData s, IAssetIndexReadOnly idx, Action<Object> onNavigate)`:
  - `so.Update()`. Render fields via `EditorGUILayout.PropertyField` for Id/Formula/DisplayName/Category/Phase/PH/VisualColor/IconSprite/BottleSprite/CrystalForm/ShortDescription. `so.ApplyModifiedProperties()`.
  - Cross-ref section: "Phản ứng liên quan" → AS INPUT list via `idx.GetReactionsUsingInput(s)`, AS OUTPUT via `GetReactionsProducing(s)`. Each item is `GUILayout.Button` with `EditorStyles.linkLabel` → calls `onNavigate(r)`.
  - "Dùng trong Level" via `idx.GetLevelsUsingSubstance(s)`.
- [ ] **10.2** `ContentEditorWindow.DrawDetailForm()` switch:
  - 0 → `SubstanceFormDrawer.Draw(_targetSO, _selectedAsset as SubstanceData, _index, NavigateTo)`.
  - 1,2 → placeholder `HelpBox`.
- [ ] **10.3** `NavigateTo(Object)`: sets `_tab` by type, calls `SetSelection(asset)`.
- [ ] **10.4** Manual: select HCl → fields editable, cross-ref links jump tabs.
- [ ] **10.5** Commit.

---

## Task 11 — `BottlePreviewDrawer` + integrate into Substance form

**Files:** Create `Drawers/BottlePreviewDrawer.cs`. Modify `Drawers/SubstanceFormDrawer.cs`.

- [ ] **11.1** Implement `BottlePreviewDrawer.Draw(Rect r, SubstanceData s)`:
  - Outline: `EditorGUI.DrawRect` with gray border (top + sides).
  - Fill bottom 70% based on `s.Phase`:
    - `Liquid`/`Aqueous`: solid `s.VisualColor`.
    - `Solid`/`Crystal`: 5-7 small colored blocks at bottom 30%.
    - `Precipitate`: gradient (3 horizontal bands top→bottom, increasing alpha).
    - `Gas`: 4-6 small bubbles (filled circles) rising.
  - Label `s.Formula` at top using `EditorStyles.boldLabel` with `richText=true`.
- [ ] **11.2** In `SubstanceFormDrawer.Draw`, reserve a `Rect previewRect = GUILayoutUtility.GetRect(80, 120, GUILayout.ExpandWidth(false))` above `Visual Assets` section and call `BottlePreviewDrawer.Draw(previewRect, s)`.
- [ ] **11.3** Manual: editing color → preview updates next repaint.
- [ ] **11.4** Commit.

---

## Task 12 — `ValidationPanelDrawer` + `QuickActionsPanelDrawer` + wire side panel

**Files:** Create `Drawers/ValidationPanelDrawer.cs`, `Drawers/QuickActionsPanelDrawer.cs`. Modify `ContentEditorWindow.cs`.

- [ ] **12.1** `ValidationPanelDrawer.Draw(List<ValidationIssue>)`:
  - Group by Severity, render each: colored icon (`MessageType.Error/Warning/Info` via `EditorGUILayout.HelpBox`).
  - If `issue.QuickFix != null`: `GUILayout.Button(issue.QuickFixLabel)` → invoke.
- [ ] **12.2** `QuickActionsPanelDrawer.Draw(Object selected, IAssetIndexReadOnly idx, Action onChanged)`:
  - Buttons: `[Duplicate]` (→ `QuickActions.Duplicate*` based on type → `onChanged()`), `[Find references]` (logs list to console via `Debug.Log`).
  - If `selected is LevelConfig`: also `[Add to GameManager]` → `QuickActions.AddLevelToGameManager(l)`.
- [ ] **12.3** `ContentEditorWindow.DrawSidePanel()`:
  - Recompute `_issues` whenever `_selectedAsset` changes or every N frames (simplest: every OnGUI — fine for small lists).
  - Call `Validator.Validate*` based on `_tab`. Then `ValidationPanelDrawer.Draw(_issues)`. `EditorGUILayout.Space`. `QuickActionsPanelDrawer.Draw(_selectedAsset, _index, () => _index.Invalidate())`.
- [ ] **12.4** Manual: editing field → issues update; Duplicate creates copy in list.
- [ ] **12.5** Commit.

---

## Task 13 — `+ New` button creates asset for current tab

**File:** Modify `ContentEditorWindow.cs` (`CreateNew()`).

- [ ] **13.1** Implement:
  - Tab 0: `var s = AssetWriter.CreateSubstance($"NewSub_{ts}"); _index.Invalidate(); SetSelection(s);`
  - Tab 1: `var r = AssetWriter.CreateReaction($"NewRx_{ts}"); _index.Invalidate(); SetSelection(r);`
  - Tab 2: `int idx = _index.GetAllLevels().Select(x=>x.LevelIndex).DefaultIfEmpty(0).Max()+1; var l = AssetWriter.CreateLevel(idx, "New Level"); _index.Invalidate(); SetSelection(l);`
  - Use `ts = DateTime.Now.Ticks.ToString().Substring(8)` for uniqueness.
- [ ] **13.2** Manual: each tab `+ New` creates a fresh asset, immediately selected.
- [ ] **13.3** Commit.

---

## Task 14 — `SearchableDropdown` util + `ReactionFormDrawer`

**Files:** Create `Util/SearchableDropdown.cs`, `Drawers/ReactionFormDrawer.cs`. Modify `ContentEditorWindow.cs` (route Reaction tab).

- [ ] **14.1** `SearchableDropdown.Draw<T>(Rect or layout, T current, IReadOnlyList<T> items, Func<T,string> labelOf, Action<T> onPick, Action<string> onCreateNew = null)`:
  - Render as button with current label.
  - On click → `GenericMenu`:
    - Items: each picks `onPick(item)`.
    - Separator + `"+ Tạo mới…"` → calls `onCreateNew(currentSearchText)` (use `EditorWindow.focusedWindow`'s search text — pass via parameter for simplicity).
  - For v1 use `GenericMenu` (no search box inside — UX acceptable for ~30 substances; can upgrade later).
- [ ] **14.2** `ReactionFormDrawer.Draw(SerializedObject so, ReactionRule r, IAssetIndexReadOnly idx, Action<string> openInlineCreate, Action<Object> onNavigate)`:
  - Fields `Id/Description/ReactionEquation/PrimarySideEffect/FlashColor/LimitedByLowestInput/SlowReaction`.
  - "Inputs": draw each `ReactionStoich` as a row with substance picker (`SearchableDropdown`), ratio float field, `[X]` remove button. `+ Add Input` button.
  - "Outputs": same.
  - "Conditions": list of `ReactionConditionType` enum + value float.
  - Auto-generate `ReactionEquation` button (Task 16 fills it).
  - `so.ApplyModifiedProperties()`.
- [ ] **14.3** Window routes Tab 1 → `ReactionFormDrawer.Draw(_targetSO, _selectedAsset as ReactionRule, _index, OpenInlineCreate, NavigateTo)`. `OpenInlineCreate(string id)` is stub for Task 15.
- [ ] **14.4** Manual: select existing reaction → fields editable; substance pickers show all substances.
- [ ] **14.5** Commit.

---

## Task 15 — `InlineCreateOverlayDrawer` + wire from Reaction picker

**Files:** Create `Drawers/InlineCreateOverlayDrawer.cs`. Modify `ContentEditorWindow.cs`.

- [ ] **15.1** State in `ContentEditorWindow`:
  ```csharp
  [SerializeField] bool _overlayOpen;
  [SerializeField] string _overlayPrefilledId;
  Action<SubstanceData> _overlayOnCreated; // set when opened; NOT serialized — re-set in OnEnable not needed since overlay closes on domain reload
  ```
  Plus a serialized backing for the in-progress new substance (Id/Formula/Phase/VisualColor) — simplest is to write a temporary asset on overlay open and edit it directly; if user cancels we delete it. For v1 use a non-serialized in-memory `SubstanceData _draftSubstance = ScriptableObject.CreateInstance<SubstanceData>()` (rebuilt on domain reload — acceptable, overlay closes).
- [ ] **15.2** `OpenInlineCreate(string prefilledId, Action<SubstanceData> onCreated)`:
  ```csharp
  _overlayOpen = true; _overlayPrefilledId = prefilledId; _overlayOnCreated = onCreated;
  _draftSubstance = ScriptableObject.CreateInstance<SubstanceData>();
  _draftSubstance.Id = prefilledId;
  ```
- [ ] **15.3** `InlineCreateOverlayDrawer.Draw(Rect windowRect, SubstanceData draft, out bool save, out bool cancel)`:
  - Dim background with semi-transparent black `EditorGUI.DrawRect`.
  - Centered modal panel ~400×420.
  - Reuse same fields as Substance form (Id/Formula/DisplayName/Category/Phase/VisualColor/PH) — call `SubstanceFormDrawer` mini version OR just inline the fields.
  - `[Huỷ]` `[Tạo & dùng]`.
- [ ] **15.4** In `OnGUI`, after main columns, `if (_overlayOpen) InlineCreateOverlayDrawer.Draw(...)`:
  - On save: `var newSub = AssetWriter.CreateSubstance(_draftSubstance.Id, s => CopyFields(_draftSubstance, s)); _index.Invalidate(); _overlayOnCreated?.Invoke(newSub); _overlayOpen=false;`
  - On cancel: `_overlayOpen=false;`
- [ ] **15.5** In `ReactionFormDrawer` substance picker → `+ Tạo mới` calls `openInlineCreate(searchText, picked => stoich.Substance = picked)`.
- [ ] **15.6** Manual: edit reaction → input picker → + Tạo mới Fe → overlay → save → asset created and input slot filled.
- [ ] **15.7** Commit.

---

## Task 16 — Auto equation string generator

**File:** Modify `Drawers/ReactionFormDrawer.cs`.

- [ ] **16.1** Add button "Auto-fill equation" in Reaction form: builds string `"{ratio}{formula} + {ratio}{formula} → {ratio}{formula} + …"` from Inputs/Outputs. Ratio shown if != 1. Formula falls back to Id if empty. Gas substances append `↑`, Precipitate append `↓`.
- [ ] **16.2** Manual: create reaction Fe + 2HCl → FeCl₂ + H₂ → click button → field populates.
- [ ] **16.3** Commit.

---

## Task 17 — `LevelFormDrawer`

**File (create):** `Drawers/LevelFormDrawer.cs`. Modify `ContentEditorWindow.cs`.

- [ ] **17.1** `LevelFormDrawer.Draw(SerializedObject so, LevelConfig l, IAssetIndexReadOnly idx, Action<string,Action<SubstanceData>> openInlineCreate, Action<Object> onNavigate)`:
  - Identity: `LevelIndex`, `DisplayName`, `ObjectiveText`.
  - Bottles: list with `SearchableDropdown` for Substance, float `InitialAmount`, toggle `MaskLabel`, string `MaskedLabel`. `+ Add Bottle`.
  - Tools: list with `ToolData` picker (use generic `EditorGUILayout.ObjectField` for v1 — ToolData asset count is small). `+ Add Tool`.
  - AvailableReactions: list with `SearchableDropdown` over `idx.GetAllReactions()`. `+ Add Reaction`.
  - PurityRule: `TargetProduct` picker, `MinTargetAmount`, `ForbiddenSubstances` list (substance picker), `ForbiddenTolerance`, `RejectAnyPrecipitate`.
  - Traps: list of `TrapDefinition` (`TrapId` text, `TriggerProduct` picker, `ExplanationVi` TextArea). Same for `ThreeStarBlockingTraps`.
  - Hints: `EditorGUILayout.ObjectField` for `HintBundle` (no Hint editor in v1 — falls back to Inspector if user double-clicks).
- [ ] **17.2** Window routes Tab 2 → `LevelFormDrawer.Draw(...)`.
- [ ] **17.3** Manual: select Level_01 → all fields editable; add bottle; save & inspect asset YAML to confirm.
- [ ] **17.4** Commit.

---

## Task 18 — Unsaved changes prompt + domain reload restore

**File:** Modify `ContentEditorWindow.cs`.

- [ ] **18.1** Implement `RestoreSelection()`: if `_selectedGuid != null`, load asset via `AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_selectedGuid), typeof(Object))`, call `SetSelection`.
- [ ] **18.2** `OnDestroy`: if `_targetSO != null && _targetSO.hasModifiedProperties` → `EditorUtility.DisplayDialogComplex("Có thay đổi chưa lưu", "Lưu trước khi đóng?", "Lưu", "Bỏ", "Huỷ đóng")`. Lưu → `AssetWriter.SaveAll()`. Bỏ → discard (no action; Unity reverts SerializedObject on dispose). Huỷ → `GetWindow<ContentEditorWindow>().Show()` to reopen.
- [ ] **18.3** Manual: edit a field, close window → prompt. Recompile scripts → window restores tab + selection.
- [ ] **18.4** Commit.

---

## Task 19 — Polish: `RichTextFormatter`, `EditorStyles_Chemistry`

**Files:** Create `Util/RichTextFormatter.cs`, `Util/EditorStyles_Chemistry.cs`.

- [ ] **19.1** `RichTextFormatter.ToLabel(string formula)` — Unity rich-text already handles `<sub>`/`<sup>` in `Label` when `richText=true`. Provide helper that returns a `GUIStyle` with `richText = true`.
- [ ] **19.2** `EditorStyles_Chemistry`: static `GUIStyle BoldRichLabel`, `LinkLabelRich`, `ListItemSelected`, lazy-initialized in property getters to avoid `OnEnable` ordering issues.
- [ ] **19.3** Replace direct `EditorStyles.*` in Drawers where rich text is needed (Substance Formula, Reaction Equation, Level DisplayName).
- [ ] **19.4** Commit.

---

## Task 20 — Smoke test checklist doc + final commit

**File:** Create `docs/superpowers/specs/2026-06-01-content-editor-smoke-test.md`.

- [ ] **20.1** Document the 8-step manual smoke test from spec §8.
- [ ] **20.2** Run the checklist in Unity Editor. Fix any defect with focused commits.
- [ ] **20.3** Commit checklist doc: `docs(editor): smoke test checklist`.

---

## Out of Scope (v2)

- Import/export CSV/JSON
- Reaction element-balance check (`RX_UNBALANCED_ELEMENTS`)
- `LV_TRAP_UNREACHABLE` (graph reachability from Bottles via AvailableReactions)
- Heuristic suggestions (`Metal needs acid reaction`)
- HintBundle inline editor / ToolData CRUD
- Automated UI tests
- Quick-fix `QuickFix` actions wired to validation issues (struct field present, no fixes attached in v1)

---

## Self-review notes

- All 20 tasks together cover spec §§1-8. v2 deferrals (§9) align with spec out-of-scope.
- Type names consistent: `AssetIndex`, `IAssetIndexReadOnly`, `Validator`, `ValidationIssue`, `Severity`, `AssetWriter`, `QuickActions`, `ContentEditorWindow`, drawer suffix `*Drawer`.
- Each task names exact file paths and commit messages.
- TDD applied to pure-logic services (Tasks 3, 5). UI tasks use manual verification (IMGUI cannot be unit-tested practically).
- `+ New` (Task 13) intentionally placed after Task 12 so created assets immediately show validation in the side panel.
