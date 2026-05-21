# Ống Nghiệm — Game Design Document

> **Tagline:** *Hoá học để chơi, không phải để học thuộc.*
> **Version:** 0.1 (Design Draft) · **Date:** 2026-05-20 · **Author:** galuathancongz

---

## Table of Contents

1. [Pillars & Pitch](#1-pillars--pitch)
2. [Core Gameplay Loop](#2-core-gameplay-loop)
3. [UI/UX Layout](#3-uiux-layout)
4. [Game Flow & State Machine](#4-game-flow--state-machine)
5. [Level Design (5 màn)](#5-level-design-5-màn)
6. [Progression, Scoring & Save](#6-progression-scoring--save)
7. [Technical Architecture (Luzart Integration)](#7-technical-architecture-luzart-integration)
8. [Asset & Sound List](#8-asset--sound-list)
9. [Roadmap & Milestones](#9-roadmap--milestones)
10. [References](#10-references)

---

## 1. Pillars & Pitch

### Pitch (60s)

Puzzle game hoá học cho học sinh THPT Việt Nam. Mỗi màn, người chơi nhận **mục tiêu** (vd: "thu NaCl tinh khiết") và **tủ thuốc** chứa nhiều hoá chất. Họ phải kéo-thả đúng chất, đúng lượng, đúng thứ tự — tránh các "bẫy" tạo sản phẩm sai. Cảm giác như đứng trong phòng thí nghiệm thật, nhưng được phép sai bao nhiêu lần cũng được.

### Target Audience

- **Chính:** Học sinh THPT VN, lớp 9-12 đang học hoá vô cơ cơ bản (HCl, NaOH, kim loại–phi kim, kết tủa, khí).
- **Phụ:** Giáo viên hoá dùng demo trên lớp.
- **Casual:** Puzzle gamer thích chemistry theme (như fan Sokobond).

### Platform

- **MVP:** PC / WebGL, 16:9 landscape, ~1920×1080.
- **Sau MVP:** mobile port (responsive sang 9:16).
- **Distribution:** Itch.io (WebGL build).

### Design Pillars (4 nguyên tắc bất biến)

1. **Đúng hoá học** — Mọi phản ứng chính xác theo SGK lớp 9-12. Không tạo phản ứng giả vì "game-y".
2. **Nhiều đường sai để dạy lựa chọn** — Mỗi màn ít nhất 2 "bẫy" hợp lý. Sai cũng học được điều gì.
3. **Sai là OK** — Không trừ điểm vì retry. Tâm lý an toàn → khám phá nhiều → học nhiều.
4. **Đơn giản trên bề mặt, sâu khi đào** — Ai cũng kéo-thả được trong 30s. Nhưng 3⭐ đòi hỏi hiểu thật.

### Anti-Pillars (KHÔNG làm)

- ✗ Không narrative/character backstory (no story dialogue trừ tutorial).
- ✗ Không real-time / không countdown timer.
- ✗ Không simulation lỏng thật (no real fluid dynamics) — **NHƯNG ✓ có fake-physics kiểu Water Sort**: liquid level tween mượt, color blend khi trộn, particle FX khi rót/sôi/sủi bọt.
- ✗ Không monetization (free, không quảng cáo, không IAP).

---

## 2. Core Gameplay Loop

### The Loop (1 màn, ~2-5 phút lần đầu)

1. **ĐỌC** mục tiêu (vd: "Thu NaCl tinh khiết")
2. **QUAN SÁT** tủ thuốc (lọ + dụng cụ)
3. **THỬ NGHIỆM** (kéo, thả, đo, đốt)
4. **NHẬN feedback** (màu/khí/kết tủa thay đổi)
5. **KIỂM TRA** mục tiêu (sản phẩm đúng + tinh khiết?)
6. **WIN ⭐ hoặc RETRY** (popup giải thích)

### Player Verbs (10 hành động)

| Verb | Tương tác |
|---|---|
| 🤏 Kéo lọ | Drag chai chất lên ống chính |
| 💧 Đổ với lượng | Slider 0-100% trước khi thả |
| 🔄 Khuấy/Trộn | Auto khi 2 chất tiếp xúc |
| 🌡 Đun | Click button + drag lên ống |
| 📜 Lọc | Drag onto ống → kết tủa tách |
| 📋 Test thuốc thử | Drag quỳ tím / KSCN / HNO₃ |
| 💨 Thu khí | Click → ống dẫn xuất hiện |
| ↶ Undo | Button góc dưới |
| 🔄 Restart | Trong pause menu |
| 💡 Hint (3 mức) | Button góc dưới (tốn ⭐) |

### Session Length Targets

- Lần đầu chơi Màn 1: **5-8 phút** (gồm tutorial)
- Chơi 1 màn mới (Màn 2-5): **3-6 phút**
- Replay 1 màn cho 3⭐: **30s-1.5ph**
- Tổng full game: **~30-45 phút** lần đầu, **~10ph** speedrun

### Core Decision Framework — "3 lớp quyết định"

Mọi "bẫy" trong brief rơi vào 1 trong 3 lớp này → khung thiết kế nhất quán cho cả 5 màn.

1. **Chọn CHẤT đúng** — đâu là HCl trong 3 lọ mất nhãn? (test bằng quỳ tím)
2. **Chọn LƯỢNG đúng** — bao nhiêu là "vừa đủ"? (test giữa chừng)
3. **Chọn THỨ TỰ đúng** — KSCN phải SAU khi có Fe³⁺, không phải trước

---

## 3. UI/UX Layout

### Art Style

**Flat outline minimalist** — phong cách Sokobond + Two Dots. Đường nét sạch, màu pastel, gradient phẳng. Vỏ container "im lặng" để **trạng thái bên trong** (màu dung dịch, kết tủa, bọt) trở thành ngôn ngữ chính. Đã có asset ống nghiệm reference.

### 7 Screens

1. **Title / Main Menu** — Logo + 3 button (Chơi / Cài đặt / Về game) + version + sound toggle.
2. **Level Select** — Grid 5 màn theo Linear unlock, hiện ⭐ đã đạt, header có total stars 0/15.
3. **In-Game HUD** ⭐ (màn hình quan trọng nhất) — Layout 4 vùng cố định:
   - **Top bar** (9% chiều cao): Pause | Mục tiêu | Star tracker
   - **Trái** (18% chiều rộng): Tủ thuốc UI Panel — lọ chất + dụng cụ
   - **Giữa** (48%): Workspace với ống nghiệm chính + slider lượng (hiện khi drag)
   - **Phải** (18%): State panel — pH, nhiệt độ, lượng, lịch sử actions
   - **Bottom** (13%): Gợi ý / Undo / Restart / Nộp bài
4. **Win Popup** — 3⭐ display + tóm tắt phản ứng + Chơi lại / Màn kế
5. **Wrong Product Popup** (soft fail) — "Sản phẩm chưa đạt" + sản phẩm tạo ra + lý do + Thử lại / Level select
6. **Hint Tiers Popup** — 3 mức (thuốc thử / thứ tự / công thức), tốn ⭐ tăng dần
7. **Pause Menu** — Tiếp tục / Restart / Xem mục tiêu / Cài đặt / Level select

### 5 UI Principles cố định cho cả game

1. **Layout 4 vùng KHÔNG đổi vị trí** giữa các màn — học sinh học 1 lần, dùng cho cả 5 màn.
2. **Color coding nhất quán** — đọc từ `ColorPalette.asset`:
   - Xanh dương = dung dịch trung tính
   - Đỏ = acid (pH<7)
   - Tím xanh = bazơ (pH>7)
   - Trắng đục = kết tủa
   - Vàng/đỏ = nguy hiểm hoặc positive (KSCN đỏ máu)
3. **Tối thiểu text** — Mỗi popup ≤ 3 dòng. Icon + màu là ngôn ngữ chính.
4. **Tooltip on hover** — Hover vào chất/dụng cụ → tooltip nhỏ hiện tên + chức năng.
5. **Animation feedback** — Mọi action có visual feedback <100ms: drag start = scale 1.05, drop success = green flash, drop fail = red shake.

---

## 4. Game Flow & State Machine

### Macro Flow — Single-Scene + Prefab Swapping

Game có **1 scene Unity duy nhất** (`Main.scene`). Mọi "screen" là prefab spawn/destroy bởi `Luzart.UIManager`.

```
Main.scene (persistent)
   ├── GameManager + AudioManager + SaveSystem
   └── Canvas (Luzart UIManager + 6 lane roots)
         ↑ ↓ Show/Hide prefab via UIManager.ShowAsync(UIId.X)
         
   MainMenu.prefab → LevelSelect.prefab → Gameplay.prefab
                                              + Pause/Result/Hint/WrongProduct (overlay popup)
```

### Micro Flow — 6 States trong 1 màn (Gameplay.prefab)

```
SETUP ──auto 1.5s──> PLAYING ──click "Nộp bài"──> SUBMITTING
                       ↕                              ↓
                     PAUSED                  RESULT_WIN | RESULT_FAIL
                                                  ↓             ↓
                                            next level | restart | LevelSelect
```

| State | Behavior |
|---|---|
| **SETUP** | Load LevelConfig, spawn tủ thuốc/ống nghiệm/dụng cụ, hiện intro mục tiêu, disable input |
| **PLAYING** | Enable input, ChemistryEngine cập nhật, tween fill/color/FX |
| **SUBMITTING** | Disable input, ChemistryEngine.Evaluate() chấm điểm |
| **RESULT_WIN** | Tính sao, lưu PlayerData, hiện Win popup, particle 🎉 |
| **RESULT_FAIL** | Hiện Wrong Product popup với lý do, 2 nút: Thử lại / Level select |
| **PAUSED** | Freeze tweens, Pause menu overlay |

### Chemistry Engine Data Flow

```
Drop HCl vào ống ──> OnSubstanceAdded(HCl, 20mL) [event]
                  ──> WorkspaceState.contents.Add()
                  ──> ChemistryEngine.MatchReactionRules()
                  ──> if match: emit OnReactionOccurred(rule)
                  ──> WorkspaceState.Apply(reaction.outputs)
                  ──> emit OnStateChanged(newState)
                  ──> LiquidView re-render (tween fill, blend color)
                  ──> StatePanel UI update (pH, lượng, log)
                  ──> FXPlayer spawn particle theo rule.sideEffects
```

Model–View tách rõ. **ChemistryEngine = pure C# (không kế thừa MonoBehaviour)** → dễ unit test.

---

## 5. Level Design (5 màn)

### Mechanics Introduction Schedule

| Màn | Mechanic mới | Mechanic kế thừa | #Bẫy |
|---|---|---|---|
| 1 | Drag-drop, slider, quỳ tím, đèn khò | — | 3 |
| 2 | Giấy lọc, đọc màu kết tủa, HNO₃ | Drag, slider, quỳ tím | 3 |
| 3 | Bình thu khí, ống dẫn, test khí | Drag, slider, quỳ tím | 3 |
| 4 | Đốt kim loại + O₂, branching path | Drag, slider, đèn khò, lọc | 5 |
| 5 | Sục khí, KSCN, oxidation 2 bước | Tất cả ở trên | 4 |

### Màn 1 — Điều chế NaCl tinh khiết (Trung bình · ~5 phút)

**Mục tiêu:** Thu NaCl rắn tinh khiết.
**Tủ thuốc:** 3 lọ mất nhãn (HCl, NaOH, Na₂CO₃) + Nước cất.
**Dụng cụ:** Quỳ tím, Đèn khò.

**Đường giải đúng:**
1. Test 3 lọ bằng quỳ tím → xác định HCl (đỏ), NaOH (xanh), Na₂CO₃ (xanh nhạt)
2. Trộn HCl + NaOH **vừa đủ**: `HCl + NaOH → NaCl + H₂O`
3. Test lại bằng quỳ tím → trung tính (tím)
4. Cô cạn bằng đèn khò → thu tinh thể NaCl

**Bẫy:**
- B1: Na₂CO₃ + 2HCl → 2NaCl + CO₂ + H₂O (cũng ra NaCl nhưng có Na₂CO₃ dư → không tinh khiết)
- B2: Dư HCl hoặc dư NaOH → tinh thể có tạp acid/bazơ
- B3: Cô cạn nước cất một mình → không có sản phẩm

**3⭐ condition:** Chỉ dùng HCl + NaOH, dùng quỳ tím xác định lọ, không dùng hint.

### Màn 2 — AgCl không lẫn kết tủa khác (Trung bình khá · ~6 phút)

**Mục tiêu:** Thu AgCl tinh khiết (lọc khô).
**Tủ thuốc:** AgNO₃, NaCl, NaOH, Na₂CO₃, HNO₃, Nước cất.
**Dụng cụ:** Quỳ tím, Giấy lọc, Đèn khò.

**Đường giải đúng:**
1. `AgNO₃ + NaCl → AgCl↓ + NaNO₃`
2. Lọc bằng giấy lọc → thu kết tủa AgCl
3. (3⭐) Rửa kết tủa bằng nước cất để loại NaNO₃ còn dính

**Bẫy:**
- B1: AgNO₃ + NaOH → Ag₂O nâu đen (lầm với AgCl)
- B2: AgNO₃ + Na₂CO₃ → Ag₂CO₃ vàng nhạt
- B3: Dư NaCl → cảnh báo "có thừa NaCl"

**Cơ chế UX đặc biệt:** Tooltip kết tủa chỉ hiện "kết tủa *màu*" thay vì tên — buộc người chơi suy luận. HNO₃ có thể acid hoá môi trường để loại Ag₂CO₃ trước khi tủa AgCl (3⭐ path).

**3⭐ condition:** Chỉ dùng AgNO₃ + NaCl + nước rửa, không dùng hint.

### Màn 3 — Điều chế CO₂ và chứng minh (Khá · ~6 phút)

**Mục tiêu:** Thu khí CO₂ vào bình + **chứng minh** bằng nước vôi trong.
**Tủ thuốc:** CaCO₃, Na₂CO₃, HCl, H₂SO₄, Ca(OH)₂, Zn, Nước cất.
**Dụng cụ:** Bình thu khí, Ống dẫn khí, Quỳ tím.

**Đường giải đúng:**
1. `CaCO₃ + 2HCl → CaCl₂ + CO₂ + H₂O`
2. Đặt bình thu khí + ống dẫn → khí CO₂ vào bình
3. Dẫn khí vào Ca(OH)₂ → vẩn đục: `CO₂ + Ca(OH)₂ → CaCO₃↓ + H₂O`

**Bẫy:**
- B1: Zn + 2HCl → ZnCl₂ + H₂ (cùng sủi bọt nhưng nước vôi không đục)
- B2: CaCO₃ + H₂SO₄ → tạo lớp CaSO₄ ít tan, phản ứng chậm → "phản ứng dừng" warning
- B3: Sinh khí xong không chứng minh = chỉ 1⭐

**3⭐ condition:** CaCO₃ + HCl + chứng minh bằng Ca(OH)₂, không H₂SO₄, không hint.

### Màn 4 — CuSO₄ tinh khiết, tránh Cu(OH)₂ (Khá-Khó · ~7 phút)

**Mục tiêu:** Dung dịch CuSO₄ xanh lam, không có kết tủa.
**Tủ thuốc:** Cu (rắn), CuO (rắn), O₂, H₂SO₄ loãng, NaOH, HCl, Nước cất.
**Dụng cụ:** Đèn khò, Giấy lọc, Quỳ tím.

**Đường giải đúng (2 đường):**
- **Đường dễ (2⭐):** `CuO + H₂SO₄ → CuSO₄ + H₂O`
- **Đường khó (3⭐):** `2Cu + O₂ --nhiệt--> 2CuO`, rồi `CuO + H₂SO₄ → CuSO₄`

**Bẫy:**
- B1: CuSO₄ + 2NaOH → Cu(OH)₂↓ xanh kết tủa
- B2: Cu + H₂SO₄ loãng → không phản ứng ("phản ứng không xảy ra")
- B3: Dùng HCl → tạo CuCl₂ (sai muối)
- B4: Dư CuO không lọc → có cặn rắn
- B5: Dư H₂SO₄ → môi trường acid mạnh

**3⭐ condition:** Đi đường khó (Cu → CuO → CuSO₄), lọc cặn nếu cần, không hint.

### Màn 5 — FeCl₃ không lẫn FeCl₂ (Khó nhất · ~8 phút)

**Mục tiêu:** Dung dịch FeCl₃, không còn FeCl₂ dư, xác nhận bằng KSCN (đỏ máu).
**Tủ thuốc:** Fe (rắn), HCl, Cl₂ (khí), NaOH, Nước cất.
**Dụng cụ:** KSCN, Quỳ tím.

**Đường giải đúng:**
1. `Fe + 2HCl → FeCl₂ + H₂`
2. Sục Cl₂: `2FeCl₂ + Cl₂ → 2FeCl₃`
3. Sục Cl₂ ĐỦ — đến khi không còn Fe²⁺ dư
4. Test KSCN → đỏ máu = có Fe³⁺ ✓

**Bẫy:**
- B1: FeCl₂ + 2NaOH → Fe(OH)₂ xanh lá
- B2: FeCl₃ + 3NaOH → Fe(OH)₃ nâu đỏ
- B3: Sục Cl₂ thiếu → còn FeCl₂. KSCN đỏ nhạt (không "đỏ máu") → 1-2⭐
- B4: Dùng KSCN trước khi sục Cl₂ → không phản ứng đặc trưng

**3⭐ condition:** Sục Cl₂ vừa đủ (gauge bar), KSCN đỏ máu rõ, không hint.

---

## 6. Progression, Scoring & Save

### Star Calculation Algorithm

```
function CalculateStars(levelResult):
  if not levelResult.targetProductCreated:
    return 0  // không đạt → RESULT_FAIL

  stars = 1  // đạt mục tiêu cơ bản

  if levelResult.isPure:
    stars = 2

  if stars == 2 AND levelResult.trapsTriggered == 0 AND levelResult.hintsUsed == 0:
    stars = 3

  return stars
```

### Star Cap by Hint Tier

| Hành vi | Max ⭐ |
|---|---|
| Không hint + không trap + tinh khiết | **3⭐** |
| Hint tier 1 (thuốc thử) | 2⭐ max |
| Hint tier 2 (thứ tự) | 2⭐ max |
| Hint tier 3 (công thức) | **1⭐ max** |
| Không tinh khiết | 1⭐ max |
| Không tạo được mục tiêu | 0⭐ (FAIL, không lưu) |

### Unlock Logic

- Mở Màn N+1 khi đạt **tối thiểu 1⭐** ở Màn N.
- Replay Màn đã qua bất cứ lúc nào; **chỉ ghi đè nếu star cao hơn** — không bao giờ giảm.
- **Bonus:** 3⭐ cả 5 màn → unlock "Lab Diploma" cosmetic trong Main Menu.

### PlayerData Schema (PlayerPrefs JSON)

```json
{
  "version": 1,
  "levels": {
    "1": { "stars": 3, "completed": true, "attempts": 4, "hintsUsedEver": 0 },
    "2": { "stars": 2, "completed": true, "attempts": 7, "hintsUsedEver": 1 },
    "3": { "stars": 0, "completed": false, "attempts": 0, "hintsUsedEver": 0 },
    "4": null,
    "5": null
  },
  "settings": { "musicVol": 0.6, "sfxVol": 0.8, "lang": "vi" },
  "diplomaUnlocked": false
}
```

Lưu vào `PlayerPrefs` key `"OngNghiem_PlayerData"` dạng JSON string.

### Total Progress Tracker

Max stars = 5 màn × 3⭐ = **15⭐**. Hiện ở Main Menu + Level Select. Mốc 5/10/15 có small celebration FX. 15/15 → unlock diploma.

### Soft Fail Behavior

Không có "Game Over". Khi sai (vd: tạo Cu(OH)₂ thay vì CuSO₄), hiện popup ấm áp: "Sản phẩm chưa đạt: Cu(OH)₂ thay vì CuSO₄. Thử lại?" → restart màn. Không trừ điểm sao của lần làm sau.

### Hint System

Button "💡 Gợi ý" góc dưới luôn có sẵn. Click → chọn 1 trong 3 tier (theo tăng dần độ giúp). Dùng hint = star cap như bảng ở trên.

---

## 7. Technical Architecture (Luzart Integration)

### Triết lý kiến trúc

**Code biết "làm thế nào". SO biết "với data nào". Prefab biết "hiện ra như thế nào".**

- **SO** cho data + luật (bản chất hoá học, config màn, bảng màu, text)
- **Prefab** cho visual (ống nghiệm, chai, popup, FX) + đính kèm SO config
- **Code** chỉ implement domain logic (ChemistryEngine, GameplayController, view binding)

Tránh over-engineer: không SO hoá runtime state, tween constants, hoặc theme. Phân biệt bằng câu hỏi: *"Cái này có cần Inspector visual để chỉnh scale/position/component không?"*

### Single Unity Scene + Prefab Swapping

Game có **1 scene duy nhất** (`Main.scene`). Mọi screen/popup là prefab spawn/destroy bởi Luzart `UIManager`.

```
Main.scene (persistent)
  ├── [Bootstrap] GameManager + AudioManager + SaveSystem (DontDestroyOnLoad)
  └── Canvas → UIManager (Luzart) + 6 lane RectTransform:
       WorldOverlay / Screen / Hud / Popup / System / Toast
```

### Luzart Framework — Có sẵn, KHÔNG viết lại

Tận dụng tối đa các module trong `Assets/Luzart/`:

| Module | Vai trò |
|---|---|
| **UIManager + NinjaUI** | UI framework đầy đủ: 6 lane, async-first, registry-driven, popup queue, lifecycle |
| **UIBase\<TContext\>** | Lifecycle hooks: `OnCreateAsync` → `OnBeforeShowAsync` → `OnShownAsync` → `OnPause/Resume` → `OnHide` |
| **UIRegistrySO** | Map `UIId → prefab/layer/cache` |
| **UIHandle, UIPopupQueue, UIBlockService, UIInputRouter** | Orchestration sẵn |
| **BaseSelect / SelectToggle / SelectSwitch** + concrete adapters | Widget pattern cho tab, toggle, button group |
| **TweenAnimationBase + SequenceTweenAnimation + TweenAnimationCaller** | Tween framework với ITweenSettings |
| **Attributes** | `[ReadOnly] [Conditional] [InfoBox] [ProgressBar] [Button] [ShowInInspector]` |
| **AssetModifier** | Editor tooling |

### UI Map vào UIRegistrySO

| UI | UIId | UILayer | Cache |
|---|---|---|---|
| Main Menu | `MainMenu` | Screen | No |
| Level Select | `LevelSelect` | Screen | Yes |
| Gameplay HUD | `GameplayHUD` | Screen | Yes (reuse cho 5 màn) |
| Pause | `PausePopup` | Popup | Yes |
| Result Win | `ResultPopup` | Popup | Yes |
| Wrong Product | `WrongProductPopup` | Popup | Yes |
| Hint | `HintPopup` | Popup | Yes |
| Tutorial overlay | `TutorialOverlay` | System | No |
| Toast | `Toast` | Toast | Yes |
| Loading | `Loading` | System | Yes |

### ScriptableObject Catalog (~100 file)

Cấu hình toàn bộ dưới `Assets/_Project/ScriptableObjects/`:

| Loại SO | Số file | Mô tả |
|---|---|---|
| **SubstanceData** | ~25 | 1 chất hoá học (id, formula, pH, color, icon, category) |
| **SubstanceCategory** | ~7 | Acid, Base, Salt, Metal, Oxide, Gas, Indicator |
| **ReactionRule** | ~30 | A+B→C+D (inputs, conditions, outputs, sideEffects, fxPrefab) |
| **ReactionCondition** | ~5 | Heat, Light, Catalyst, AquaticOnly |
| **ToolData** | ~8 | 1 dụng cụ (id, name, function type, icon) |
| **PurityRule** | ~5 | Luật "tinh khiết" (no excess, no precipitate, ...) |
| **LevelConfig** | 5 | Toàn bộ data 1 màn (5 file) |
| **TrapDefinition** | ~18 | 1 bẫy + lý do |
| **HintBundle** | 5 | 3 mức hint per màn (nested hoặc riêng) |
| **WinConditionData** | ~5 | Target sản phẩm + tolerance |
| **ColorPalette** | 1 | Singleton bảng màu |
| **LocalizationTable** | 1 | Key→text vi/en |
| **Event Channels** | ~4 | OnReactionOccurred, OnLevelCompleted, OnLevelFailed, OnStateChanged (chỉ những cái cần wire Inspector) |

### Prefab Catalog (~25 file)

| Loại Prefab | Số file |
|---|---|
| Screens: MainMenu, LevelSelect, Gameplay | 3 |
| Overlays: Pause, Result, Hint, WrongProduct | 4 |
| Gameplay: TestTube, Bottle, ToolButton, InventoryPanel, WorkspaceArea, StatePanel | ~8 |
| FX: Bubble, Pouring, Crystallize, Flame, Precipitate, Smoke, Sparkle | ~10 |

### Code do GAME tự viết (~25 file C#)

```
Assets/_Project/Scripts/
├── Core/
│   ├── GameManager.cs        # singleton, currentLevelId, isPaused
│   ├── AudioManager.cs       # Luzart-agnostic, AudioMixer-based
│   └── SaveSystem.cs         # PlayerPrefs JSON
├── Chemistry/                # pure C#, no MonoBehaviour
│   ├── ChemistryEngine.cs
│   ├── WorkspaceState.cs
│   ├── ReactionMatcher.cs
│   ├── PurityChecker.cs
│   └── Models/ (Substance, Amount, Reaction, ...)
├── Gameplay/
│   ├── GameplayController.cs # FSM trong màn
│   ├── DraggableBottle.cs
│   ├── DroppableTube.cs
│   ├── BottleView.cs         # Bind(SubstanceData)
│   ├── LiquidView.cs         # fillAmount + DOTween blend
│   ├── ToolButton.cs         # Bind(ToolData)
│   ├── StatePanel.cs
│   ├── HintController.cs
│   └── FXPlayer.cs
└── UI/                       # kế thừa Luzart UIBase
    ├── MainMenuUI.cs
    ├── LevelSelectUI.cs
    ├── GameplayHUD.cs
    ├── ResultPopup.cs
    ├── WrongProductPopup.cs
    ├── HintPopup.cs
    └── PausePopup.cs
```

### Liquid Animation System (fake-physics)

`LiquidView.cs` (~150 dòng) trên `TestTube.prefab`:

- 2 child Image: `liquidFill` (Filled, Vertical bottom→top) + `liquidSurface` (mặt nước)
- API `SetLevel(float 0-1, float duration)` → `liquidFill.DOFillAmount(target, dur).SetEase(Ease.OutQuad)`
- API `SetColor(Color target, float duration)` → `liquidFill.DOColor(target, dur)`
- Khi rót: spawn `FX_Pouring` particle ở miệng chai
- Khi reaction: `FXPlayer` đọc `ReactionRule.sideEffects` → spawn bubble/smoke/precipitate/sparkle

### BaseSelect Áp Dụng

| Vị trí | Loại |
|---|---|
| Tab Inventory ("Hoá chất" ↔ "Dụng cụ") | `SelectSwitch` + `SelectSwitchGameObject` |
| Hint tier picker (3 mức) | `SelectSwitch` + `SelectSwitchTMP_Text` |
| Mute Music/SFX | `SelectToggle` + `SelectToggleImage` |
| Star display 0-3⭐ | `SelectSwitch` (int) + `SelectSwitchImage` |

### TweenAnimationBase Áp Dụng

- Popup Show/Hide: mỗi popup attach `SequenceTweenAnimation` → `UIBase.AnimateShowAsync` gọi `showAnim.Show().ToUniTask()`
- Bottle hover scale, Star celebration, Level unlock, Wrong product shake
- **Ngoại lệ:** `LiquidView.fillAmount` viết tay DOTween — cần custom blend color theo reaction

### Bootstrap Sequence

```csharp
// GameBootstrap.cs (gắn trong Main.scene)
async void Start() {
  SaveSystem.Load();                 // PlayerData từ PlayerPrefs JSON
  AudioManager.Init();
  await UIManager.Instance.PreloadAsync(UIId.Loading);
  await UIManager.Instance.ShowAsync(UIId.MainMenu);
}
```

### Recommended Packages

| Package | Bắt buộc | Mục đích |
|---|---|---|
| DOTween (Free) | ✅ | Tween animation |
| TextMeshPro | ✅ (có sẵn) | Text rendering |
| Cysharp UniTask | ✅ (Luzart phụ thuộc) | Async/await |
| com.unity.vectorgraphics | 👍 | Import SVG nếu dùng vector asset |
| Cinemachine | ⚪ | Camera shake (tuỳ) |
| Addressables | ⚪ | Over-engineer cho MVP |

### Testing Strategy

- **Unit test ChemistryEngine** (Edit Mode tests) — pure C#, test mọi reaction + 18 bẫy. Mục tiêu coverage >90%.
- **Playmode tests** cho FSM transition: SETUP → PLAYING → SUBMITTING → RESULT.
- **Manual QA checklist** — ~30 test case (drag, undo, restart, mọi nút...).

---

## 8. Asset & Sound List

### Sprites (~67 file)

| Loại | Tên file | Số lượng |
|---|---|---|
| Container | tube_main, beaker, erlenmeyer, gas_jar, dropper | 5 |
| Chai chất lỏng | bottle_blank + 11 label (HCl, NaOH, Na2CO3, AgNO3, NaCl, H2SO4, HNO3, CaOH2, CuSO4, question_mark, ...) | 1+11 |
| Chất rắn | solid_Zn, solid_Cu, solid_CuO, solid_CaCO3, solid_Fe, solid_NaCl_crystal, solid_AgCl_precipitate | 7 |
| Khí (icon) | gas_O2, gas_Cl2, gas_CO2, gas_H2 | 4 |
| Thuốc thử | tool_litmus_strip, tool_KSCN_dropper, tool_HNO3_dropper | 3 |
| Dụng cụ | tool_burner, tool_filter_paper, tool_distilled_water, tool_gas_tube | 4 |
| UI Frame (9-slice) | frame_inventory_panel, frame_workspace, frame_state_panel, frame_top_bar, frame_bottom_bar | 5 |
| UI Button | btn_play, btn_pause, btn_hint, btn_undo, btn_restart, btn_submit, btn_close, btn_settings, btn_back, btn_next | 10 |
| Star icons | star_filled, star_empty, star_lock | 3 |
| Background | bg_menu, bg_levelselect, bg_gameplay | 3 |
| Level cards | card_level_1..5 + 5 thumbnail mục tiêu | 6 |
| FX particle textures | fx_bubble, fx_smoke, fx_sparkle, fx_precipitate_dot, fx_flame, fx_steam | 6 |

### Sound (~25 SFX + 3 music)

| Category | SFX |
|---|---|
| UI | button_click, button_hover, popup_open, popup_close, tab_switch, star_pop ×3, level_unlock, back |
| Chemistry Action | pour_liquid, pour_into_empty, drop_solid, litmus_dip, burner_ignite, burner_loop, filter_paper, dropper_drop |
| Reaction | bubble_small, bubble_large, precipitate_form, gas_evolve, evaporate, color_change, burst_celebration, wrong_product |
| Music | menu_calm (loop ~2min), gameplay_focus (loop ~3min), win_short (sting ~5s) |

**Nguồn miễn phí:**
- Sprite: [Kenney.nl](https://kenney.nl) (CC0), [game-icons.net](https://game-icons.net) (CC-BY 4000+ icon)
- Audio: [freesound.org](https://freesound.org), [Kenney audio packs](https://kenney.nl/assets?q=audio)
- Music: [incompetech.com](https://incompetech.com) (Kevin MacLeod, CC-BY)

### Font

- **Tiếng Việt:** Be Vietnam Pro / Nunito / Open Sans (Google Fonts, hỗ trợ đầy đủ dấu)

---

## 9. Roadmap & Milestones

### Phases (~10-12 tuần solo dev)

#### Phase 0: Foundation Setup — 3-4 ngày

- Verify Luzart UIManager + DOTween hoạt động (tạo Main.scene, gắn UIManager + 6 lane RectTransform)
- Tạo UIRegistrySO entries cho 10 UI
- Tạo ColorPalette, LocalizationTable (vi only ở MVP)
- Setup folder structure `Assets/_Project/{Scenes, Scripts, ScriptableObjects, Prefabs, Art, Audio}`
- Bootstrap script (load save, init audio, show MainMenu)

**Output:** Game khởi động, hiện main menu rỗng.

#### Phase 1: Core Chemistry Engine — 1.5 tuần

- Pure C# `Chemistry/` namespace: ChemistryEngine, WorkspaceState, ReactionMatcher, PurityChecker
- Tạo các SO base class: SubstanceData, ReactionRule, ToolData, ReactionCondition, PurityRule
- Tạo ~25 SubstanceData asset + ~30 ReactionRule asset
- Unit test: assert mọi reaction theo brief, test 18 bẫy

**Output:** `ChemistryEngine.Mix(...)` chạy đúng mọi case từ test.

#### Phase 2: Gameplay Prefab + Màn 1 Playable — 2 tuần

- Prefab: TestTube, Bottle, ToolButton, InventoryPanel, WorkspaceArea, StatePanel
- Components: DraggableBottle, DroppableTube, BottleView, ToolButton, LiquidView (fillAmount + DOTween), StatePanel
- GameplayController FSM (SETUP → PLAYING → SUBMITTING → RESULT_WIN/FAIL → PAUSED)
- GameplayHUD (kế thừa UIBase) — load LevelConfig, spawn prefabs theo data
- Level 1 (NaCl) data + play through start-to-finish

**Output:** Vertical slice Màn 1 chơi được full loop.

#### Phase 3: 4 Màn Còn Lại + Polish — 3 tuần

- Bổ sung mechanic cho 4 màn: lọc giấy (M2), bình thu khí + ống dẫn (M3), đốt kim loại + O₂ (M4), sục khí + KSCN (M5)
- LevelConfig + TrapDefinition + HintBundle cho 5 màn
- FX Prefab: bubble, pouring, crystallize, flame, precipitate
- Sound integration (~25 SFX + 3 music)
- ResultPopup, WrongProductPopup, HintPopup với TweenAnimationBase show/hide
- Tutorial overlay cho Màn 1 (~6 step)

**Output:** 5 màn chơi được, có sound + FX, có hint.

#### Phase 4: Progression + Save System + Polish UI — 1.5 tuần

- SaveSystem.cs (PlayerPrefs JSON), PlayerData schema
- LevelSelect screen: hiện star, unlock logic, replay support
- Star calculation + StarCap theo hint tier
- MainMenu polish: total stars tracker, settings (volume), credits, diploma unlock badge
- Tooltip system on hover
- Animation polish: star celebration, level unlock, popup show/hide

**Output:** Game "complete" về tính năng. Save/load OK.

#### Phase 5: QA + Balance + Build — 1-1.5 tuần

- Internal playtest 5 màn, ghi nhận bug + frustration point
- External playtest với 3-5 học sinh THPT thật
- Balance: chỉnh tolerance "vừa đủ", chỉnh hint text, thêm/bớt bẫy nếu cần
- WebGL build + test trên Chrome/Firefox/Safari
- itch.io page setup, screenshot, trailer 30s
- Final: ZIP for thầy cô / Build standalone Windows nếu cần

**Output:** v1.0 launch trên itch.io.

### Milestone Calendar (giả định start 2026-05-20)

| Mốc | Tuần | Date | Deliverable |
|---|---|---|---|
| M0 - Foundation | 1 | ~2026-05-24 | UIManager + Bootstrap chạy |
| M1 - Engine | 2-3 | ~2026-06-07 | ChemistryEngine + 25 SubstanceData + 30 ReactionRule, unit test xanh |
| M2 - Vertical Slice (Màn 1) | 4-5 | ~2026-06-21 | Màn 1 full loop |
| M3 - All 5 Levels | 6-8 | ~2026-07-12 | 5 màn playable + FX + sound |
| M4 - Polish + Save | 9-10 | ~2026-07-26 | Progression + UI polish hoàn thiện |
| M5 - Launch v1.0 | 11-12 | ~2026-08-09 | WebGL build, itch.io live |

### Risk Register

| Risk | Mức | Mitigation |
|---|---|---|
| Hoá học sai khiến mất uy tín | **Cao** | Unit test mọi reaction; nhờ giáo viên hoá review trước launch |
| "Bẫy" quá khó → frustrate | Trung | Hint system + Wrong Product popup với lý do; playtest 5 học sinh |
| Asset không kịp (solo dev) | Trung | Dùng Kenney + game-icons.net làm base, tinh chỉnh sau |
| WebGL build chậm/lỗi shader | Thấp | Tránh complex shader, dùng UI Image + sprite phẳng |
| Luzart UIManager bug ẩn | Thấp | Đã production-tested ở Ninja School Online; có Luzart docs để debug |

---

## 10. References

### Game Design Documents (tham khảo)

- [Game Design Document Template — Nuclino](https://www.nuclino.com/articles/game-design-document-template)
- [Indie GDD Guide — Indie Game Academy](https://indiegameacademy.com/free-game-design-document-template-how-to-guide/)
- [Crafting GDDs in 2026 — Hitem3D](https://www.hitem3d.ai/blog/en-What-is-a-Game-Design-Document-GDD-How-to-Write-an-Effective-Game-Design-Document/)

### Chemistry Puzzle Games (cảm hứng gameplay)

- [Sokobond — Thinky Games](https://thinkygames.com/games/sokobond/)
- [Sokobond Wikipedia](https://en.wikipedia.org/wiki/Sokobond)
- [Chemistry-Lab Unity Project — GitHub](https://github.com/PanMig/Chemistry-Lab)
- [Elemental Home Chemistry Game — NCBI](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC12355905/)

### Technical Patterns (kiến trúc)

- *"Game Architecture with Scriptable Objects"* — Ryan Hipple, Unite 2017 (data-driven SO pattern)
- *"Game Feel"* — Steve Swink (player verbs, perceived fidelity)
- Luzart UIFramework — `Assets/Luzart/UIFramework/ninja_school_online_ui_base_technical.md` (UI Manager design reference)

### Asset Sources

- [Kenney.nl](https://kenney.nl) — CC0 sprite packs
- [game-icons.net](https://game-icons.net) — CC-BY 4000+ icons
- [freesound.org](https://freesound.org), [Kenney Audio](https://kenney.nl/assets?q=audio) — SFX
- [incompetech.com](https://incompetech.com) — Music (Kevin MacLeod, CC-BY)

---

*Document version 0.1 — Design Draft. Sẽ update khi vào implementation. Cập nhật theo nguyên tắc "living document" — mỗi quyết định lớn add 1 entry vào changelog.*

## Changelog

- **2026-05-20** — v0.1 initial draft after brainstorming session (9 sections, all approved).
