# Content Editor — Smoke Test Checklist

Run after pulling the changes. Open the project in Unity, wait for the asset DB to refresh, then walk through these steps. Each item ends with the expected outcome — if reality differs, file an issue.

## Pre-flight

- [ ] Unity Console has **no compile errors**.
- [ ] Menu **`ChemistryGame → Open Content Editor`** is visible.

## 1. Open the window

- [ ] Click menu → window opens at ≥1100×620, title "Content Editor".
- [ ] Top tab bar shows `Substances (N)`, `Reactions (N)`, `Levels (5)`, `+ New`, where N matches asset counts under `Assets/_Project/ScriptableObjects/`.

## 2. List + search

- [ ] Click `Substances` tab → left column populated; e.g. `HCl  (Aqueous)`, `NaOH  (Aqueous)`.
- [ ] Type `fe` in search → list filters to FeCl₂, FeCl₃, FeOH₂, FeOH₃, FeSCN₃, Fe.
- [ ] Clear search → list restores.

## 3. Substance form + cross-reference navigation

- [ ] Click `HCl` → middle column shows form (Id, Formula, Phase, Color etc.) and the bottle preview (red-ish liquid).
- [ ] Section "Phản ứng liên quan / AS INPUT" lists `HCl_NaOH`, `Na2CO3_HCl`, `CaCO3_HCl`, etc.
- [ ] Click any reaction link → window jumps to Reactions tab with that rule selected.
- [ ] Click any "Dùng trong Level" link → window jumps to Levels tab with that level selected.

## 4. Validation panel

- [ ] Right column "Validation" shows live findings.
- [ ] Pick `NaCl` → "AQUEOUS_NO_CRYSTAL" should NOT appear (NaCl is Crystal phase) — but `NaCl_aq` Aqueous DOES show Info `SUB_AQUEOUS_NO_CRYSTAL` only if no `CrystalForm`.
- [ ] Edit a Substance to give it the same `Id` as another → "SUB_ID_DUPLICATE" error appears immediately.
- [ ] Undo the change (Ctrl+Z) → error disappears.

## 5. + New flow

- [ ] On Substances tab, click `+ New` → new asset `Sub_NewSub_<ts>.asset` appears in Project + list + selected automatically.
- [ ] Edit Id to "MyTest", Phase to Aqueous, pick a color → preview updates.
- [ ] Switch to Reactions tab → `+ New` → new `Rx_NewRx_<ts>.asset` appears.
- [ ] Switch to Levels tab → `+ New` → new level with `LevelIndex = max+1` (i.e. 6).

## 6. Reaction editing with Inline Create overlay

- [ ] Select a reaction, e.g. `HCl_NaOH`. Add 1 more Input.
- [ ] Click the empty Substance picker on the new row → menu of all substances appears, plus `+ Tạo chất mới…` at the bottom.
- [ ] Click `+ Tạo chất mới…` → overlay opens, rest of UI dimmed and disabled.
- [ ] Fill Id="OverlayTest", Phase=Aqueous, color=red, click `Tạo & dùng`.
- [ ] Overlay closes, the Input slot now shows `OverlayTest`, asset `Sub_OverlayTest.asset` exists in Project.

## 7. Auto equation

- [ ] On a reaction with valid Inputs and Outputs, click `Auto-fill equation từ Inputs/Outputs`.
- [ ] `ReactionEquation` field gets `"<ratio><formula> + … → <ratio><formula> + …"`; Gas gets `↑`, Precipitate gets `↓`.

## 8. Level form + Add to GameManager

- [ ] Select existing `Level_05_FeCl3`. Verify Bottles list shows 3 substances, Tools list shows 3, Available Reactions shows 5.
- [ ] Add a new bottle via dropdown picker → save (Ctrl+S or just edit anything).
- [ ] Click `+ New` to create a fresh Level 6.
- [ ] In the right column Quick Actions, click `Add to GameManager`. Dialog confirms success.
- [ ] Open scene `Assets/_Project/Scenes/Main.unity` → select GameManager → list `Levels` now has 6 entries.

## 9. Duplicate

- [ ] Select any asset → right column `Duplicate` → new asset `<name>_Copy.asset` is created and auto-selected.

## 10. Delete

- [ ] Select a temp asset → right column `Delete asset` → confirm → asset moves to trash, list refreshes, form clears.

## 11. Domain-reload survival

- [ ] With an asset selected, modify any script (e.g. add a comment) → Unity recompiles.
- [ ] After recompile, the window restores the same tab + same selection.

## 12. Unsaved changes prompt

- [ ] Edit a field in the form.
- [ ] Close the Content Editor window → dialog asks `Lưu / Bỏ / Huỷ đóng`.
- [ ] Choose `Huỷ đóng` → window reopens with edits intact.
- [ ] Close again, choose `Lưu` → asset is saved.

## 13. Undo (Ctrl+Z)

- [ ] After `+ New Substance` → Ctrl+Z → asset is destroyed.
- [ ] After `Duplicate` → Ctrl+Z → duplicate is destroyed.

## 14. External asset changes

- [ ] With window open, delete a Substance from Project window. List refreshes within 1 second; selection cleared if it was the deleted one.
- [ ] Rename a Substance asset from Project window. The list label updates after refresh.

## 15. Unit tests

- [ ] Open **Window → General → Test Runner → EditMode → Run All**.
- [ ] All tests in `ChemistryEditor.Tests` assembly pass.

---

## Known v1 limitations (acceptable)

- `SUB_FILENAME_MISMATCH` warning does not have an auto-fix button (manual rename).
- `+ Add Reaction` in Level form starts with a null slot — pick from dropdown after.
- ToolData / HintBundle do not have a dedicated tab; use Unity Inspector for those.
- Element-balance validation not implemented (Rx ratios are user responsibility).
- No CSV/JSON import-export — deferred to v2.
