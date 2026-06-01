# Chemistry Content Editor Tool — Design Spec

**Date**: 2026-06-01
**Status**: Approved — ready for implementation plan
**Scope**: Single Unity Editor tool to let game designers (non-developers) create and maintain Substances, Reactions, and Levels without touching Unity Inspector or source code.

---

## 1. Goal & non-goals

### Goal
Provide one EditorWindow that supports the full content-authoring workflow for the chemistry game:
- CRUD on `SubstanceData`, `ReactionRule`, `LevelConfig` assets
- Cross-reference navigation between the three types
- Inline creation of dependencies (create new Substance from inside Reaction picker)
- Live validation with severity levels and quick-fix actions
- Quick actions: duplicate, add-to-GameManager, find-references, simulate reaction chain
- Live visual preview (bottle render) for Substance

### Non-goals (v1)
- Import/export to JSON/CSV (deferred)
- Localization editing (existing `I2Languages.asset` workflow unchanged)
- Tool/HintBundle CRUD (used directly from existing Inspector — out of scope for now)
- Automated UI tests (manual smoke checklist only)

---

## 2. User flow

### Primary flow — create a full new level from scratch

1. Open menu `ChemistryGame → Open Content Editor` → `ContentEditorWindow` appears.
2. **Tab Substances** — create missing chemicals:
   - Click `+ New`, form clears with auto-generated default `Id`.
   - Fill `Id`, `Formula`, `Phase`, `Color` (live preview updates).
   - Save → `Sub_<Id>.asset` is created under `Assets/_Project/ScriptableObjects/Substances/`.
3. **Tab Reactions** — define rules:
   - Click `+ New`. Form shows empty Inputs/Outputs lists.
   - For each slot: click `SearchableDropdown` → pick substance, or click `+ Tạo mới <query>` → `InlineCreateOverlay` opens a compact Substance form. On save, overlay closes and slot auto-fills.
   - Equation string auto-generates from Inputs/Outputs.
   - Save → `Rx_<Id>.asset`.
4. **Tab Levels** — assemble the level:
   - Click `+ New`. `LevelIndex` auto-fills to `max + 1`.
   - Add Bottles (pick Substance + `InitialAmount`).
   - Add Tools (pick `ToolData`).
   - Add Available Reactions (pick `ReactionRule`).
   - Configure `PurityRule` (`TargetProduct`, `MinTargetAmount`, `ForbiddenSubstances`).
   - Add Traps and `ThreeStarBlockingTraps`.
   - Pick a `HintBundle` (or `+ New Hint`).
5. Save → `Level_<index>_<name>.asset`.
6. Click **Quick Action: Add to GameManager** → opens scene `Main.unity` if needed, finds GameManager, appends the new Level to its `levels` list, saves scene, closes.
7. Validation panel stays live throughout — designer sees red ❌ / yellow ⚠ / blue ℹ issues with quick-fix buttons.

### Secondary flow — inspect "Where is Fe used?"

1. Tab Substances → select `Fe`.
2. Detail form's section **"Phản ứng liên quan"** lists all `ReactionRule` referencing Fe, split by `AS INPUT` / `AS OUTPUT`.
3. Section **"Dùng trong Level"** lists all `LevelConfig` containing a Bottle of Fe.
4. Click `[open]` next to any item → tool switches tab and selects that asset.
5. Click `+ Phản ứng mới với Fe` → tab Reactions, new rule, Fe pre-filled in first Input slot.

---

## 3. Architecture

### File layout

```
Assets/_Project/Scripts/Editor/ChemistryEditor/
├── ContentEditorWindow.cs              EditorWindow, menu item, owns all state
│
├── Drawers/                            Stateless IMGUI render functions
│   ├── ListPanelDrawer.cs              Generic <T> list + search box
│   ├── SubstanceFormDrawer.cs          Substance detail form + preview + cross-ref
│   ├── ReactionFormDrawer.cs           Reaction form + inputs/outputs picker
│   ├── LevelFormDrawer.cs              Level form: bottles, tools, reactions, purity, traps
│   ├── ValidationPanelDrawer.cs        Renders List<ValidationIssue> with severity icons
│   ├── QuickActionsPanelDrawer.cs      Action buttons
│   ├── InlineCreateOverlayDrawer.cs    Modal overlay form for creating Substance inline
│   └── BottlePreviewDrawer.cs          Draws fake test tube using Color + Phase
│
├── Services/                           Pure C# logic, no GUI — unit-testable
│   ├── AssetIndex.cs                   Cache of FindAssets + reverse maps
│   ├── AssetIndexPostprocessor.cs      AssetPostprocessor hook → AssetIndex.Invalidate()
│   ├── Validator.cs                    Produces List<ValidationIssue> per asset
│   ├── ValidationIssue.cs              { Severity, Message, Action QuickFix }
│   ├── QuickActions.cs                 Duplicate, AddToGameManager, FindReferences
│   └── AssetWriter.cs                  Wraps AssetDatabase + Undo registration
│
└── Util/
    ├── SearchableDropdown.cs           Filtering dropdown + "+ Create new" footer
    ├── RichTextFormatter.cs            Renders <sub>/<sup> using IMGUI RichText
    └── EditorStyles_Chemistry.cs       Cached GUIStyle (avoid per-frame allocation)
```

### Layout

Single window, 3 columns:

```
+--------------------------------------------------------------+
| [Substances] [Reactions] [Levels]                  [+ New]   |
+------------+-----------------------------+---------------------+
|            |                             |                     |
| LIST       | DETAIL FORM                 | SIDE PANEL          |
| 25%        | 50%                         | 25%                 |
|            |                             |                     |
| [Search..] | Form fields                 | Validation Issues   |
| Item       | Sub-sections                | (live)              |
| Item       | Live preview                |                     |
| > Item sel | Cross-references            | Quick Actions       |
| Item       |                             |                     |
|            |                             |                     |
+------------+-----------------------------+---------------------+
```

When `InlineCreateOverlay` is active: dims the whole window with a semi-transparent overlay, draws a centered modal panel.

### State ownership

`ContentEditorWindow` is the sole state owner:
- `[SerializeField] int activeTabIndex`
- `[SerializeField] string selectedGuid`
- `[SerializeField] string searchText`
- `[SerializeField] bool overlayOpen`
- `[SerializeField] string overlayPrefilledId`
- `SerializedObject serializedTarget` (recreated when selection changes)
- `List<ValidationIssue> currentIssues` (rebuilt on field change)

`[SerializeField]` ensures state survives domain reload (script recompile).

Drawers receive state by parameter — never read it from a static or singleton. This keeps panels independently testable conceptually and avoids state-sync bugs.

### Data flow — selecting an asset

```
User clicks list row
  └─ ContentEditorWindow.SetSelection(guid)
        ├─ Save pending edits if any (or prompt)
        ├─ Resolve guid → asset
        ├─ Create new SerializedObject(asset)
        ├─ Run Validator → currentIssues
        └─ Repaint
```

### Data flow — inline create overlay

```
ReactionFormDrawer sees user click "+ Tạo mới <id>"
  └─ Sets ContentEditorWindow.overlayOpen = true, overlayPrefilledId = <id>
        └─ Next OnGUI: InlineCreateOverlayDrawer runs LAST → covers everything
              ├─ User fills form
              ├─ Clicks "Tạo & dùng"
              ├─ AssetWriter.CreateSubstance(...) → new asset (Undo-wrapped)
              ├─ Window subscribes to AssetIndex.OnIndexChanged → refresh
              ├─ Window resolves new asset → assigns to the pending Reaction slot
              └─ overlayOpen = false
```

---

## 4. AssetIndex (cross-reference service)

Maintains in-memory caches:

```csharp
Dictionary<string, SubstanceData>  substancesByGuid;
Dictionary<string, SubstanceData>  substancesById;
Dictionary<string, ReactionRule>   reactionsByGuid;
Dictionary<string, LevelConfig>    levelsByGuid;

// Reverse indices:
Dictionary<SubstanceData, List<ReactionRule>>  reactionsByInputSubstance;
Dictionary<SubstanceData, List<ReactionRule>>  reactionsByOutputSubstance;
Dictionary<SubstanceData, List<LevelConfig>>   levelsContainingSubstance;
Dictionary<ReactionRule,  List<LevelConfig>>   levelsContainingReaction;
```

Built once on first access via `AssetDatabase.FindAssets("t:<Type>")` + load.
Invalidated by `AssetIndexPostprocessor.OnPostprocessAllAssets()`.
Cheap to rebuild: project has < 100 assets total per type.

---

## 5. Validator

Pure function `ValidateAsset(UnityEngine.Object asset, AssetIndex idx) → List<ValidationIssue>`.

```csharp
public enum Severity { Error, Warning, Info }

public struct ValidationIssue {
    public Severity Severity;
    public string Message;       // user-facing Vietnamese
    public string Code;          // for tests, e.g. "SUB_ID_EMPTY"
    public Action QuickFix;      // nullable — if set, button shows in panel
    public string QuickFixLabel;
}
```

### Rule catalogue (codes)

**Substance** (`SUB_*`):
- `SUB_ID_EMPTY`, `SUB_ID_HAS_WHITESPACE`, `SUB_ID_DUPLICATE`, `SUB_FILENAME_MISMATCH`, `SUB_ORPHAN`, `SUB_AQUEOUS_NO_CRYSTAL`

**Reaction** (`RX_*`):
- `RX_INPUT_NULL`, `RX_OUTPUT_NULL`, `RX_INPUTS_EMPTY`, `RX_OUTPUTS_EMPTY`, `RX_RATIO_NONPOSITIVE`, `RX_UNBALANCED_ELEMENTS`, `RX_DUPLICATE_INPUTS`

**Level** (`LV_*`):
- `LV_INDEX_DUPLICATE`, `LV_TARGET_NULL`, `LV_TARGET_IN_FORBIDDEN`, `LV_TRAP_TRIGGER_NULL`, `LV_TRAP_UNREACHABLE`, `LV_NOT_IN_GAMEMANAGER`, `LV_NO_BOTTLES`, `LV_NO_TOOLS`, `LV_HINTS_NULL`, `LV_REACTION_NOT_TRIGGERABLE`

Each code has at most one quick-fix where automation is meaningful (e.g., `LV_NOT_IN_GAMEMANAGER` → calls `QuickActions.AddLevelToGameManager(level)`).

---

## 6. Edge cases

| Case | Handling |
|---|---|
| Asset deleted externally while selected | AssetPostprocessor clears index. Window shows "Asset đã bị xoá" banner + clears selection. |
| Asset renamed in Project window | References still valid (Unity uses GUID). Index rebuilds. List label updates. |
| Domain reload mid-edit | `[SerializeField]` fields restore tab, selectedGuid, searchText. SerializedObject re-created on first OnGUI. Pending unsaved edits in SerializedObject are lost — same behavior as Unity Inspector. |
| Close window with unsaved changes | `OnDestroy`: if `serializedObject.hasModifiedProperties`, prompt "Save / Discard / Cancel". |
| Two assets with same Id | Both shown in list with red "DUP" chip. Validation issue on each. |
| Undo (Ctrl+Z) on create/duplicate/rename | All `AssetWriter` operations call `Undo.RegisterCompleteObjectUndo` / `Undo.RegisterCreatedObjectUndo`. |
| Add-to-GameManager when scene Main not open | Open scene via `EditorSceneManager.OpenScene` in additive mode, mutate GameManager, save, close. |
| Case-insensitive Id collision (`Fe` vs `fe`) | Inline-create overlay shows warning "Đã có chất tên gần giống: Fe — vẫn tạo?" before commit. |
| Inline overlay opened from Reaction A; user then clicks tab Substances | Overlay blocks tab switch via grayed-out tab bar. Close overlay first. |

---

## 7. Bottle preview

`BottlePreviewDrawer.Draw(Rect, SubstanceData)`:
- Outer rect = test tube outline (rounded rect, gray border).
- Fill bottom 70% with color based on `Phase`:
  - `Liquid` / `Aqueous`: solid `VisualColor` (with alpha).
  - `Crystal` / `Solid`: square blocks at bottom 30%.
  - `Precipitate`: gradient — clear top, colored bottom sediment.
  - `Gas`: small bubbles rising in mostly empty tube.
- Label at top with `Formula` (rich text for `<sub>`).

Implemented purely with `EditorGUI.DrawRect` + `GUI.Label`. No textures needed.

---

## 8. Testing

### Unit tests — `Assets/_Project/Tests/Editor/ChemistryEditor/`

- `ValidatorTests.cs`
  - For each rule code, build a minimal fixture in memory (no asset on disk needed — `ScriptableObject.CreateInstance`) and assert the issue list contains that code.
  - Test that valid fixtures produce empty issue list.
- `AssetIndexTests.cs`
  - Setup: `AssetDatabase.CreateFolder("Assets", "TempChemTest_<guid>")`, create fixture assets.
  - Teardown: delete folder.
  - Verify forward + reverse maps populated correctly.
  - Verify `Invalidate()` triggers rebuild.
- `QuickActionsTests.cs`
  - `Duplicate` produces deep copy with new asset, same content, renamed.
  - `AddLevelToGameManager` requires loading a mock scene fixture.
- `RichTextFormatterTests.cs`
  - Round-trip `<sub>2</sub>` rendering inputs.

### Manual smoke test checklist

1. Open tool, create new Substance "TestSub", save, close & reopen → still exists.
2. Open Reaction tab, `+ New`, add input via "+ Tạo mới" overlay → asset created, overlay closes, slot filled.
3. Create Level, add bottles/tools/reactions, click `Add to GameManager` → verify in scene file.
4. Edit a chất to share `Id` with another → red `SUB_ID_DUPLICATE` appears.
5. Edit a Level's `PurityRule.TargetProduct` into its own `ForbiddenSubstances` → red `LV_TARGET_IN_FORBIDDEN`.
6. Close window with unsaved changes → prompt appears.
7. Create chất via tool, then `Edit → Undo` → asset deleted.
8. Delete chất from Project window while selected in tool → banner appears, list refreshes.

---

## 9. Out-of-scope (parking lot for v2)

- Import/export JSON/CSV
- Reaction equation auto-balancing (currently best-effort warning only)
- Localization editor integration
- HintBundle dedicated form (use Unity Inspector for now)
- ToolData CRUD (existing seeder/Inspector workflow remains)
- Automated UI testing
- Heuristic AI suggestions ("Metal cần rule với acid") — listed but only as Info severity in v1; full coaching system parked

---

## 10. Open assumptions

- Unity version supports IMGUI (always true) and `SerializedObject` Undo (Unity 2019+).
- All Substance/Reaction/Level assets live under `Assets/_Project/ScriptableObjects/{Substances,Reactions,Levels}/` — `AssetWriter` writes here.
- `GameManager` is on a GameObject in `Assets/_Project/Scenes/Main.unity` — `QuickActions.AddLevelToGameManager` opens this scene.
- Project's existing `ChemistrySeeder` continues to work; this tool does not deprecate it. Designers may use either workflow.
