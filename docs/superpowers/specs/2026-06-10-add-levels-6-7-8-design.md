# Thiết kế: Thêm Màn 6, 7, 8 + visual cho chất rắn

> Ngày: 2026-06-10 · Nguồn: `D:\Downloads\Bổ sung.docx` · Trạng thái: đã duyệt hướng, đang triển khai

## 1. Mục tiêu & phạm vi

Thêm 3 màn chơi mới (theo tài liệu "Bổ sung") vào game hoá học, **không sửa engine hoá học**:

- **Màn 6 — Nhận biết ion SO₄**: tạo & chứng minh kết tủa BaSO₄ trắng.
- **Màn 7 — Điều chế Cu(OH)₂ & nhiệt phân**: tạo CuO đen từ CuSO₄ (kết tủa → nung).
- **Màn 8 — Phân biệt CO₂/SO₂**: thu đúng CO₂, chứng minh bằng nước vôi, tránh nhầm SO₂.

Kèm 1 yêu cầu bổ sung (do user nêu):
- **Visual chất rắn**: chất rắn / kết tủa không được hiển thị như chất lỏng trong ống nghiệm.

### Non-goals (KHÔNG làm trong đợt này)
- KHÔNG sửa `ChemistryEngine`, `ReactionMatcher`, `PurityChecker`, `WorkspaceState` (engine giữ nguyên).
- KHÔNG biến thao tác "lọc" / "thu khí" thành ràng buộc thắng-thua (giữ cosmetic như Màn 2/4 hiện tại).
- KHÔNG vẽ art mới phức tạp / không viết lại shader (visual chất rắn dùng giải pháp code an toàn, tinh chỉnh sau trong Unity).

## 2. Quyết định "có cần code core không?"

**Engine: 0 thay đổi.** Mọi cơ chế đã có & đã wire: kết tủa (`SubstancePhase.Precipitate`), phản ứng nhiệt (`ReactionConditionType.Heat` → `Cu_O2` đã dùng), thu khí (`GasCollector`→`CollectGasInto`), lọc (`FilterPaper`), quỳ tím, chấm tinh khiết + bẫy + sao, mở khoá màn (`IsLevelUnlocked` tổng quát).

Các điểm "tích hợp" (không phải engine) phải đụng:

| # | Việc | Loại | File |
|---|------|------|------|
| 1 | Thêm 7 chất + 8 phản ứng + 3 màn + 3 hint | Content tooling | `ChemistrySeeder.cs` |
| 2 | Đăng ký 3 màn vào `GameManager.levels` (scene) | Scene data | `Main.unity` (qua editor script) |
| 3 | `15` → `24` (ngưỡng bằng khen + nhãn "/ N sao") | Hằng số hiển thị | `SaveSystem.cs`, `LevelSelectUI.cs` |
| 4 | Xoá stub trống `Level_06_New_Level_6.asset` | Dọn dẹp | asset |
| 5 | Visual chất rắn khác chất lỏng | View code | `DraggableBottle.cs` (+ tùy chọn `LayeredLiquidView.cs`) |

## 3. Chất mới (7) — thêm vào `SeedSubstances()`

Chữ ký: `S(id, formula, display, Category, Phase, pH, Color)`

| id | formula | display | Category | Phase | pH | màu (gợi ý) |
|----|---------|---------|----------|-------|----|----|
| `BaCl2` | BaCl₂ | Bari clorua | Salt | Aqueous | 7 | xanh nhạt trong |
| `BaSO4` | BaSO₄ | Bari sunfat | Salt | **Precipitate** | 7 | **trắng** đục |
| `BaCO3` | BaCO₃ | Bari cacbonat | Salt | **Precipitate** | 7 | **trắng** (gần như BaSO₄ → đó là bẫy) |
| `Na2SO3` | Na₂SO₃ | Natri sunfit | Salt | Aqueous | 9 | trong nhạt |
| `SO2` | SO₂ | Khí sunfurơ | Gas | **Gas** | 3 | vàng-lục nhạt |
| `CaSO3` | CaSO₃ | Canxi sunfit | Salt | **Precipitate** | 7 | trắng đục |
| `CaSO4` | CaSO₄ | Canxi sunfat | Salt | **Precipitate** | 7 | trắng đục |

Không cần `CrystalForm` cho chất nào (không có bước cô cạn→tinh thể trong 3 màn này).

## 4. Phản ứng mới (8) — thêm vào `SeedReactions()`

Tái dùng (đã có): `CuSO4_NaOH`, `CuO_HCl`, `CaCO3_HCl`, `CO2_CaOH2`.

| id | Inputs | Outputs | Condition | FX | Ghi chú |
|----|--------|---------|-----------|----|----|
| `Na2SO4_BaCl2` | Na2SO4·1 + BaCl2·1 | BaSO4·1 + NaCl_aq·2 | — | PrecipitateForm | **đường đúng M6** |
| `BaCl2_Na2CO3` | BaCl2·1 + Na2CO3·1 | BaCO3·1 + NaCl_aq·2 | — | PrecipitateForm | bẫy M6 (trắng nhầm) |
| `H2SO4_BaCl2` | H2SO4·1 + BaCl2·1 | BaSO4·1 + HCl·2 | — | PrecipitateForm | bẫy M6 (dư acid) |
| `CuOH2_Heat` | CuOH2·1 | CuO·1 + H2O·1 | **Heat** | SmokeWhite | **nhiệt phân M7** |
| `CuOH2_HCl` | CuOH2·1 + HCl·2 | CuCl2·1 + H2O·2 | — | ColorFlash | bẫy M7 (tan Cu(OH)₂) |
| `Na2SO3_HCl` | Na2SO3·1 + HCl·2 | NaCl_aq·2 + SO2·1 + H2O·1 | — | BubblesLarge | bẫy M8 (sai khí) |
| `SO2_CaOH2` | SO2·1 + CaOH2·1 | CaSO3·1 + H2O·1 | — | PrecipitateForm | bẫy M8 (đục nhưng sai) |
| `H2SO4_CaCO3` | H2SO4·1 + CaCO3·1 | CaSO4·1 + CO2·1 + H2O·1 | — | BubblesSmall | bẫy M8 (CaSO₄ ít tan); `SlowReaction=true` |

## 5. Màn chơi mới (3) — thêm vào `SeedLevels()`

### Màn 6 — `Level_06_BaSO4` (index 6)
- DisplayName: "Nhận biết ion SO₄"; Objective: "Tạo và chứng minh kết tủa BaSO₄ trắng."
- Bottles: Na2SO4(40), BaCl2(40), H2SO4(40), NaCl(30), Na2CO3(40), HCl(40)
- Tools: Litmus, FilterPaper, DistilledWater
- Reactions: `Na2SO4_BaCl2`, `BaCl2_Na2CO3`, `H2SO4_BaCl2`
- Purity: Target **BaSO4**, Min 5; Forbidden {BaCO3, HCl}; Tol 0.5
- Traps: `BaCO3_trap`→BaCO3 ("BaCO₃ cũng trắng — là cacbonat, không phải sunfat."); `AcidExcess`→HCl ("Dùng H₂SO₄ dư còn HCl — không tinh khiết.")
- 3★ blocking: cả 2 · Hints Lv6

### Màn 7 — `Level_07_CuO` (index 7)
- DisplayName: "Điều chế CuO"; Objective: "Tạo CuO đen: kết tủa Cu(OH)₂ rồi nung."
- Bottles: CuSO4(40), NaOH(50), HCl(40), H2SO4(40)
- Tools: FilterPaper, Burner, Litmus, DistilledWater
- Reactions: `CuSO4_NaOH`, `CuOH2_Heat`, `CuOH2_HCl`, `CuO_HCl`
- Purity: Target **CuO**, Min 5; Forbidden {CuCl2, NaOH}; Tol 1
- Traps: `HClDissolve`→CuCl2 ("HCl làm tan Cu(OH)₂ → CuCl₂, sai mục tiêu."); `ExcessBase`→NaOH ("Dư NaOH — còn bazơ, sản phẩm bẩn.")
- 3★ blocking: cả 2 · Hints Lv7
- **Ghi chú bẫy "không lọc → còn Na₂SO₄": chỉ để trong hint (engine không ép lọc); Na₂SO₄ KHÔNG forbidden.**

### Màn 8 — `Level_08_CO2_SO2` (index 8)
- DisplayName: "Phân biệt CO₂ và SO₂"; Objective: "Thu đúng CO₂, chứng minh bằng nước vôi; tránh nhầm SO₂."
- Bottles: CaCO3(40), Na2CO3(40), Na2SO3(40), HCl(60), H2SO4(50), CaOH2(50)
- Tools: GasCollector, Litmus, DistilledWater
- Reactions: `CaCO3_HCl`, `CO2_CaOH2`, `Na2SO3_HCl`, `SO2_CaOH2`, `H2SO4_CaCO3`
- Purity: Target **CaCO3** (tủa nước vôi = bằng chứng), Min 5; Forbidden {SO2, CaSO3, CaSO4}; Tol 0.5
- Traps: `WrongGasSO2`→CaSO3 ("Na₂SO₃ → SO₂ (không phải CO₂); nước vôi đục bởi CaSO₃."); `SulfateStall`→CaSO4 ("H₂SO₄+CaCO₃ → CaSO₄ ít tan, phản ứng chậm/dừng.")
- 3★ blocking: `WrongGasSO2` · Hints Lv8
- **Looseness đã biết** (giống Màn 3): CaCO3 vừa là lọ vừa là target → về lý thuyết có thể "thắng" mà không phản ứng. Chấp nhận theo pattern hiện có.

## 6. Tích hợp

### 6.1 Đăng ký màn vào GameManager
Thêm `[MenuItem("ChemistryGame/Seed/Register Levels To GameManager")]`:
mở `Assets/_Project/Scenes/Main.unity`, `FindObjectOfType<GameManager>()`, dùng `SerializedObject` set property `levels` = tất cả `LevelConfig` (sort theo `LevelIndex`), `MarkSceneDirty` + `SaveScene`. Gọi luôn trong `SeedAll()`.

### 6.2 Hằng số 15 → 24
- `SaveSystem.ReportLevelResult`: `TotalStars() >= 15` → `>= 24`.
- `LevelSelectUI.BuildCards`: `"{...} / 15 sao"` → `/ 24 sao`.
(Tùy chọn: rút hằng số ra một chỗ, nhưng YAGNI — chỉ sửa 2 literal.)

### 6.3 Xoá stub
Xoá `Level_06_New_Level_6.asset` + `.meta`.

## 7. Visual chất rắn (giải pháp an toàn, không art mới)

Vấn đề: `DraggableBottle.Refresh()` + tube vẽ mọi chất như chất lỏng.

Giải pháp (code-only, tinh chỉnh sau trong Unity):
- `DraggableBottle`: nếu `Substance.IsSolid || Substance.IsPrecipitate` → render khác:
  - fill **đục (alpha=1)**, **đỉnh phẳng** (không mặt cong chất lỏng), bottom-anchored — đọc như "bột/khối rắn".
  - ẩn `LayeredLiquidView` con (mặt shader chất lỏng) cho chất rắn.
  - thêm tag nhỏ "(rắn)" / ký hiệu vào label để rõ.
  - thêm `[SerializeField]` tunables (màu viền, độ đậm) để user chỉnh trong Inspector.
- Dữ liệu: set `Phase` đúng cho 7 chất mới (đã có ở §3) → tube tự xếp chúng xuống đáy (logic `IsSettled` đã có).
- **Verify**: cần mở Unity để xem (không verify được headless). Nếu muốn "bột hạt" thật (procedural sprite/shader grain) → làm đợt sau với Unity mở để xem trực tiếp.

## 8. Cách chạy (handoff cho user trong Unity)
1. Mở Unity (project tự compile code mới).
2. Menu `ChemistryGame/Seed/All` → tạo chất/phản ứng/màn + auto đăng ký vào GameManager.
3. Vào Play → Level Select thấy 8 màn, "/ 24 sao".
4. (Visual chất rắn) xem trong bottles & tube, chỉnh tunables nếu cần.

## 9. Kiểm thử
- Logic (review tay): từng màn — đường đúng tạo target; mỗi bẫy tạo đúng forbidden product.
- Compile: code C# hợp lệ, mọi `id` tham chiếu trong seeder đều tồn tại.
- In-Unity (user): seed chạy không lỗi; 3 màn chơi & thắng được; bẫy hiện popup đúng; sao cap đúng.

## 10. Rủi ro
- Không chạy được Unity ở đây → seed & visual phải user xác nhận.
- Sửa scene bằng script: dùng `SerializedObject` an toàn; vẫn nên commit trước khi chạy.
- Visual chất rắn: giải pháp mặc định có thể cần user tinh chỉnh thẩm mỹ.
