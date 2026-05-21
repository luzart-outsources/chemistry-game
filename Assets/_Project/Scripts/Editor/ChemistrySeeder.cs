#if UNITY_EDITOR
using System.Collections.Generic;
using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;

namespace ChemistryGame.EditorTools
{
    /// <summary>
    /// Seeds toàn bộ ScriptableObject: Substances, Reactions, Tools, Levels.
    /// Idempotent: tạo nếu thiếu, update nếu đã có.
    /// Menu: ChemistryGame > Seed > All / Substances / Reactions / Tools / Levels.
    /// </summary>
    public static class ChemistrySeeder
    {
        private const string SUBSTANCE_DIR = "Assets/_Project/ScriptableObjects/Substances";
        private const string REACTION_DIR  = "Assets/_Project/ScriptableObjects/Reactions";
        private const string TOOL_DIR      = "Assets/_Project/ScriptableObjects/Tools";
        private const string LEVEL_DIR     = "Assets/_Project/ScriptableObjects/Levels";

        [MenuItem("ChemistryGame/Seed/All")]
        public static void SeedAll()
        {
            SeedSubstances();
            SeedTools();
            SeedReactions();
            SeedLevels();
            Debug.Log("[ChemistrySeeder] All seeded.");
        }

        [MenuItem("ChemistryGame/Seed/Substances")]
        public static Dictionary<string, SubstanceData> SeedSubstances()
        {
            var subs = LoadAll<SubstanceData>(SUBSTANCE_DIR);

            void S(string id, string formula, string display, SubstanceCategoryType cat,
                   SubstancePhase phase, float ph, Color color)
            {
                if (!subs.TryGetValue(id, out var so))
                {
                    so = ScriptableObject.CreateInstance<SubstanceData>();
                    AssetDatabase.CreateAsset(so, $"{SUBSTANCE_DIR}/Sub_{id}.asset");
                    subs[id] = so;
                }
                so.Id = id; so.Formula = formula; so.DisplayName = display;
                so.Category = cat; so.Phase = phase; so.PH = ph; so.VisualColor = color;
                EditorUtility.SetDirty(so);
            }

            S("H2O",    "H₂O",    "Nước",                       SubstanceCategoryType.Solvent,   SubstancePhase.Liquid,      7f,  new Color(0.7f, 0.9f, 1f, 0.3f));
            S("HCl",    "HCl",    "Axit clohidric",             SubstanceCategoryType.Acid,      SubstancePhase.Aqueous,     1f,  new Color(0.95f, 0.4f, 0.4f, 0.7f));
            S("NaOH",   "NaOH",   "Natri hidroxit",             SubstanceCategoryType.Base,      SubstancePhase.Aqueous,    13f,  new Color(0.45f, 0.55f, 0.9f, 0.7f));
            S("Na2CO3", "Na₂CO₃", "Natri cacbonat",             SubstanceCategoryType.Base,      SubstancePhase.Aqueous,    11f,  new Color(0.55f, 0.75f, 0.95f, 0.7f));
            S("NaCl_aq","NaCl(dd)","Muối ăn (dd)",              SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     7f,  new Color(0.85f, 0.95f, 1f, 0.6f));
            S("NaCl",   "NaCl",   "Muối ăn (rắn)",              SubstanceCategoryType.Salt,      SubstancePhase.Crystal,     7f,  new Color(1f, 1f, 1f, 0.95f));
            S("AgNO3",  "AgNO₃",  "Bạc nitrat",                 SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     6f,  new Color(0.85f, 0.9f, 0.95f, 0.5f));
            S("AgCl",   "AgCl",   "Bạc clorua",                 SubstanceCategoryType.Salt,      SubstancePhase.Precipitate, 7f,  new Color(0.96f, 0.96f, 0.96f, 0.95f));
            S("NaNO3",  "NaNO₃",  "Natri nitrat",               SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     7f,  new Color(0.85f, 0.95f, 1f, 0.5f));
            S("Ag2O",   "Ag₂O",   "Bạc oxit",                   SubstanceCategoryType.Oxide,     SubstancePhase.Precipitate, 7f,  new Color(0.18f, 0.16f, 0.14f, 0.95f));
            S("Ag2CO3", "Ag₂CO₃", "Bạc cacbonat",               SubstanceCategoryType.Salt,      SubstancePhase.Precipitate, 7f,  new Color(0.93f, 0.85f, 0.42f, 0.95f));
            S("HNO3",   "HNO₃",   "Axit nitric",                SubstanceCategoryType.Acid,      SubstancePhase.Aqueous,     1f,  new Color(0.95f, 0.7f, 0.5f, 0.7f));
            S("CaCO3",  "CaCO₃",  "Canxi cacbonat",             SubstanceCategoryType.Salt,      SubstancePhase.Solid,       7f,  new Color(0.9f, 0.9f, 0.85f, 1f));
            S("CaCl2",  "CaCl₂",  "Canxi clorua",               SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     7f,  new Color(0.85f, 0.95f, 1f, 0.5f));
            S("CO2",    "CO₂",    "Khí cacbonic",               SubstanceCategoryType.Gas,       SubstancePhase.Gas,         5f,  new Color(0.8f, 0.8f, 0.85f, 0.4f));
            S("H2",     "H₂",     "Khí hidro",                  SubstanceCategoryType.Gas,       SubstancePhase.Gas,         7f,  new Color(0.95f, 0.95f, 1f, 0.3f));
            S("Zn",     "Zn",     "Kẽm",                        SubstanceCategoryType.Metal,     SubstancePhase.Solid,       7f,  new Color(0.65f, 0.7f, 0.75f, 1f));
            S("ZnCl2",  "ZnCl₂",  "Kẽm clorua",                 SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     5f,  new Color(0.8f, 0.92f, 0.98f, 0.6f));
            S("CaOH2",  "Ca(OH)₂","Canxi hidroxit (nước vôi)",  SubstanceCategoryType.Base,      SubstancePhase.Aqueous,    12f,  new Color(0.9f, 0.95f, 1f, 0.5f));
            S("H2SO4",  "H₂SO₄",  "Axit sunfuric",              SubstanceCategoryType.Acid,      SubstancePhase.Aqueous,     1f,  new Color(0.95f, 0.78f, 0.45f, 0.7f));
            S("Cu",     "Cu",     "Đồng",                       SubstanceCategoryType.Metal,     SubstancePhase.Solid,       7f,  new Color(0.78f, 0.45f, 0.2f, 1f));
            S("CuO",    "CuO",    "Đồng oxit",                  SubstanceCategoryType.Oxide,     SubstancePhase.Solid,       7f,  new Color(0.1f, 0.1f, 0.12f, 1f));
            S("CuSO4",  "CuSO₄",  "Đồng sunfat",                SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     6f,  new Color(0.18f, 0.6f, 0.92f, 0.85f));
            S("CuOH2",  "Cu(OH)₂","Đồng hidroxit",              SubstanceCategoryType.Base,      SubstancePhase.Precipitate, 9f,  new Color(0.32f, 0.6f, 0.92f, 0.95f));
            S("CuCl2",  "CuCl₂",  "Đồng clorua",                SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     6f,  new Color(0.3f, 0.85f, 0.4f, 0.8f));
            S("Na2SO4", "Na₂SO₄", "Natri sunfat",               SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     7f,  new Color(0.85f, 0.95f, 1f, 0.5f));
            S("O2",     "O₂",     "Khí oxi",                    SubstanceCategoryType.Gas,       SubstancePhase.Gas,         7f,  new Color(0.85f, 0.95f, 1f, 0.3f));
            S("Fe",     "Fe",     "Sắt",                        SubstanceCategoryType.Metal,     SubstancePhase.Solid,       7f,  new Color(0.45f, 0.4f, 0.4f, 1f));
            S("FeCl2",  "FeCl₂",  "Sắt(II) clorua",             SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     5f,  new Color(0.5f, 0.8f, 0.55f, 0.7f));
            S("FeCl3",  "FeCl₃",  "Sắt(III) clorua",            SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     5f,  new Color(0.92f, 0.78f, 0.2f, 0.85f));
            S("FeOH2",  "Fe(OH)₂","Sắt(II) hidroxit",           SubstanceCategoryType.Base,      SubstancePhase.Precipitate, 9f,  new Color(0.5f, 0.78f, 0.45f, 0.95f));
            S("FeOH3",  "Fe(OH)₃","Sắt(III) hidroxit",          SubstanceCategoryType.Base,      SubstancePhase.Precipitate, 9f,  new Color(0.62f, 0.34f, 0.18f, 0.95f));
            S("Cl2",    "Cl₂",    "Khí clo",                    SubstanceCategoryType.Gas,       SubstancePhase.Gas,         4f,  new Color(0.85f, 0.95f, 0.6f, 0.4f));
            S("KSCN",   "KSCN",   "Kali thioxianat",            SubstanceCategoryType.Indicator, SubstancePhase.Aqueous,     7f,  new Color(0.9f, 0.92f, 0.95f, 0.5f));
            S("FeSCN3", "Fe(SCN)₃","Sắt(III) thioxianat",       SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     5f,  new Color(0.78f, 0.1f, 0.12f, 0.95f));
            S("KCl",    "KCl",    "Kali clorua",                SubstanceCategoryType.Salt,      SubstancePhase.Aqueous,     7f,  new Color(0.85f, 0.95f, 1f, 0.5f));

            // ===== Crystallization links (Aqueous → Crystal khi cô cạn) =====
            // NaCl_aq → NaCl crystal: classic L1 goal "thu NaCl tinh thể".
            // Các Aqueous khác (CuSO4, FeCl3) cố tình KHÔNG có CrystalForm → burner không
            // crystallize được → bảo vệ target aqueous của L4, L5.
            if (subs.TryGetValue("NaCl_aq", out var naclAq) && subs.TryGetValue("NaCl", out var naclCr))
            {
                naclAq.CrystalForm = naclCr;
                EditorUtility.SetDirty(naclAq);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[ChemistrySeeder] Substances: {subs.Count}");
            return subs;
        }

        [MenuItem("ChemistryGame/Seed/Tools")]
        public static Dictionary<string, ToolData> SeedTools()
        {
            var subs = LoadAll<SubstanceData>(SUBSTANCE_DIR);
            var tools = LoadAll<ToolData>(TOOL_DIR);

            void T(string id, string name, ToolFunctionType fn, string assocSub, string desc)
            {
                if (!tools.TryGetValue(id, out var t))
                {
                    t = ScriptableObject.CreateInstance<ToolData>();
                    AssetDatabase.CreateAsset(t, $"{TOOL_DIR}/Tool_{id}.asset");
                    tools[id] = t;
                }
                t.Id = id; t.DisplayName = name; t.FunctionType = fn;
                if (!string.IsNullOrEmpty(assocSub) && subs.ContainsKey(assocSub))
                    t.AssociatedSubstance = subs[assocSub];
                t.ShortDescription = desc;
                EditorUtility.SetDirty(t);
            }

            T("Litmus",       "Quỳ tím",        ToolFunctionType.IndicatorPaper, null,   "Test acid/bazơ qua đổi màu");
            T("Burner",       "Đèn khò",        ToolFunctionType.Burner,         null,   "Đun nóng, làm bay hơi");
            T("FilterPaper",  "Giấy lọc",       ToolFunctionType.FilterPaper,    null,   "Tách kết tủa khỏi dung dịch");
            T("DistilledWater","Nước cất",      ToolFunctionType.DistilledWater, "H2O",  "Thêm nước cất pha loãng / rửa");
            T("GasCollector", "Bình thu khí",   ToolFunctionType.GasCollector,   null,   "Thu khí thoát ra");
            T("KSCN",         "Thuốc thử KSCN", ToolFunctionType.ReagentDropper, "KSCN", "Test Fe³⁺ → đỏ máu");
            T("HNO3_drop",    "HNO₃ pha loãng", ToolFunctionType.ReagentDropper, "HNO3", "Acid hoá nhẹ");
            T("Bubble_Cl2",   "Sục Cl₂",        ToolFunctionType.GasBubbler,     "Cl2",  "Sục khí clo");
            T("Bubble_O2",    "Sục O₂",         ToolFunctionType.GasBubbler,     "O2",   "Sục khí oxi");

            AssetDatabase.SaveAssets();
            Debug.Log($"[ChemistrySeeder] Tools: {tools.Count}");
            return tools;
        }

        [MenuItem("ChemistryGame/Seed/Reactions")]
        public static Dictionary<string, ReactionRule> SeedReactions()
        {
            var subs = LoadAll<SubstanceData>(SUBSTANCE_DIR);
            var rxs  = LoadAll<ReactionRule>(REACTION_DIR);

            ReactionRule R(string id, string desc, string equation, SideEffectType fx)
            {
                if (!rxs.TryGetValue(id, out var r))
                {
                    r = ScriptableObject.CreateInstance<ReactionRule>();
                    AssetDatabase.CreateAsset(r, $"{REACTION_DIR}/Rx_{id}.asset");
                    rxs[id] = r;
                }
                r.Id = id; r.Description = desc; r.ReactionEquation = equation;
                r.PrimarySideEffect = fx; r.Inputs.Clear(); r.Outputs.Clear(); r.Conditions.Clear();
                EditorUtility.SetDirty(r);
                return r;
            }

            ReactionStoich Sto(string subId, float ratio)
            {
                return new ReactionStoich { Substance = subs.TryGetValue(subId, out var x) ? x : null, Ratio = ratio };
            }

            // ===== Màn 1 =====
            var r1 = R("HCl_NaOH",       "HCl + NaOH → NaCl + H₂O",  "HCl + NaOH → NaCl + H₂O",   SideEffectType.ColorFlash);
            r1.Inputs.Add(Sto("HCl",1));   r1.Inputs.Add(Sto("NaOH",1));
            r1.Outputs.Add(Sto("NaCl_aq",1));r1.Outputs.Add(Sto("H2O",1));

            var r2 = R("Na2CO3_HCl",     "Na₂CO₃ + 2HCl → 2NaCl + CO₂↑ + H₂O", "Na₂CO₃ + 2HCl → 2NaCl + CO₂ + H₂O", SideEffectType.BubblesLarge);
            r2.Inputs.Add(Sto("Na2CO3",1));r2.Inputs.Add(Sto("HCl",2));
            r2.Outputs.Add(Sto("NaCl_aq",2));r2.Outputs.Add(Sto("CO2",1));r2.Outputs.Add(Sto("H2O",1));

            // ===== Màn 2 =====
            var r3 = R("AgNO3_NaCl",     "AgNO₃ + NaCl → AgCl↓ + NaNO₃", "AgNO₃ + NaCl → AgCl↓ + NaNO₃", SideEffectType.PrecipitateForm);
            r3.Inputs.Add(Sto("AgNO3",1));r3.Inputs.Add(Sto("NaCl_aq",1));
            r3.Outputs.Add(Sto("AgCl",1));r3.Outputs.Add(Sto("NaNO3",1));

            var r4 = R("AgNO3_NaOH",     "2AgNO₃ + 2NaOH → Ag₂O↓ + 2NaNO₃ + H₂O", "2AgNO₃ + 2NaOH → Ag₂O + 2NaNO₃ + H₂O", SideEffectType.PrecipitateForm);
            r4.Inputs.Add(Sto("AgNO3",2));r4.Inputs.Add(Sto("NaOH",2));
            r4.Outputs.Add(Sto("Ag2O",1));r4.Outputs.Add(Sto("NaNO3",2));r4.Outputs.Add(Sto("H2O",1));

            var r5 = R("AgNO3_Na2CO3",   "2AgNO₃ + Na₂CO₃ → Ag₂CO₃↓ + 2NaNO₃", "2AgNO₃ + Na₂CO₃ → Ag₂CO₃ + 2NaNO₃", SideEffectType.PrecipitateForm);
            r5.Inputs.Add(Sto("AgNO3",2));r5.Inputs.Add(Sto("Na2CO3",1));
            r5.Outputs.Add(Sto("Ag2CO3",1));r5.Outputs.Add(Sto("NaNO3",2));

            // ===== Màn 3 =====
            var r6 = R("CaCO3_HCl",      "CaCO₃ + 2HCl → CaCl₂ + CO₂↑ + H₂O", "CaCO₃ + 2HCl → CaCl₂ + CO₂ + H₂O", SideEffectType.BubblesLarge);
            r6.Inputs.Add(Sto("CaCO3",1));r6.Inputs.Add(Sto("HCl",2));
            r6.Outputs.Add(Sto("CaCl2",1));r6.Outputs.Add(Sto("CO2",1));r6.Outputs.Add(Sto("H2O",1));

            var r7 = R("Zn_HCl",         "Zn + 2HCl → ZnCl₂ + H₂↑", "Zn + 2HCl → ZnCl₂ + H₂", SideEffectType.BubblesSmall);
            r7.Inputs.Add(Sto("Zn",1));r7.Inputs.Add(Sto("HCl",2));
            r7.Outputs.Add(Sto("ZnCl2",1));r7.Outputs.Add(Sto("H2",1));

            var r8 = R("CO2_CaOH2",      "CO₂ + Ca(OH)₂ → CaCO₃↓ + H₂O", "CO₂ + Ca(OH)₂ → CaCO₃ + H₂O (nước vôi đục)", SideEffectType.PrecipitateForm);
            r8.Inputs.Add(Sto("CO2",1));r8.Inputs.Add(Sto("CaOH2",1));
            r8.Outputs.Add(Sto("CaCO3",1));r8.Outputs.Add(Sto("H2O",1));

            // ===== Màn 4 =====
            var r9 = R("CuO_H2SO4",      "CuO + H₂SO₄ → CuSO₄ + H₂O", "CuO + H₂SO₄ → CuSO₄ + H₂O", SideEffectType.ColorFlash);
            r9.Inputs.Add(Sto("CuO",1));r9.Inputs.Add(Sto("H2SO4",1));
            r9.Outputs.Add(Sto("CuSO4",1));r9.Outputs.Add(Sto("H2O",1));

            var r10 = R("Cu_O2",          "2Cu + O₂ -t°→ 2CuO", "2Cu + O₂ --t°→ 2CuO", SideEffectType.Flame);
            r10.Inputs.Add(Sto("Cu",2));r10.Inputs.Add(Sto("O2",1));
            r10.Outputs.Add(Sto("CuO",2));
            r10.Conditions.Add(new ReactionCondition { Type = ReactionConditionType.Heat });

            var r11 = R("CuSO4_NaOH",     "CuSO₄ + 2NaOH → Cu(OH)₂↓ + Na₂SO₄", "CuSO₄ + 2NaOH → Cu(OH)₂ + Na₂SO₄ (xanh kết tủa)", SideEffectType.PrecipitateForm);
            r11.Inputs.Add(Sto("CuSO4",1));r11.Inputs.Add(Sto("NaOH",2));
            r11.Outputs.Add(Sto("CuOH2",1));r11.Outputs.Add(Sto("Na2SO4",1));

            var r12 = R("CuO_HCl",        "CuO + 2HCl → CuCl₂ + H₂O", "CuO + 2HCl → CuCl₂ + H₂O (sai muối)", SideEffectType.ColorFlash);
            r12.Inputs.Add(Sto("CuO",1));r12.Inputs.Add(Sto("HCl",2));
            r12.Outputs.Add(Sto("CuCl2",1));r12.Outputs.Add(Sto("H2O",1));

            // ===== Màn 5 =====
            var r13 = R("Fe_HCl",         "Fe + 2HCl → FeCl₂ + H₂↑", "Fe + 2HCl → FeCl₂ + H₂", SideEffectType.BubblesSmall);
            r13.Inputs.Add(Sto("Fe",1));r13.Inputs.Add(Sto("HCl",2));
            r13.Outputs.Add(Sto("FeCl2",1));r13.Outputs.Add(Sto("H2",1));

            var r14 = R("FeCl2_Cl2",      "2FeCl₂ + Cl₂ → 2FeCl₃", "2FeCl₂ + Cl₂ → 2FeCl₃ (oxi hoá)", SideEffectType.ColorFlash);
            r14.Inputs.Add(Sto("FeCl2",2));r14.Inputs.Add(Sto("Cl2",1));
            r14.Outputs.Add(Sto("FeCl3",2));

            var r15 = R("FeCl2_NaOH",     "FeCl₂ + 2NaOH → Fe(OH)₂↓ + 2NaCl", "FeCl₂ + 2NaOH → Fe(OH)₂ + 2NaCl (xanh lá)", SideEffectType.PrecipitateForm);
            r15.Inputs.Add(Sto("FeCl2",1));r15.Inputs.Add(Sto("NaOH",2));
            r15.Outputs.Add(Sto("FeOH2",1));r15.Outputs.Add(Sto("NaCl_aq",2));

            var r16 = R("FeCl3_NaOH",     "FeCl₃ + 3NaOH → Fe(OH)₃↓ + 3NaCl", "FeCl₃ + 3NaOH → Fe(OH)₃ + 3NaCl (nâu đỏ)", SideEffectType.PrecipitateForm);
            r16.Inputs.Add(Sto("FeCl3",1));r16.Inputs.Add(Sto("NaOH",3));
            r16.Outputs.Add(Sto("FeOH3",1));r16.Outputs.Add(Sto("NaCl_aq",3));

            var r17 = R("FeCl3_KSCN",     "FeCl₃ + 3KSCN → Fe(SCN)₃ + 3KCl", "FeCl₃ + 3KSCN → Fe(SCN)₃ + 3KCl (đỏ máu)", SideEffectType.ColorFlash);
            r17.Inputs.Add(Sto("FeCl3",1));r17.Inputs.Add(Sto("KSCN",3));
            r17.Outputs.Add(Sto("FeSCN3",1));r17.Outputs.Add(Sto("KCl",3));

            AssetDatabase.SaveAssets();
            Debug.Log($"[ChemistrySeeder] Reactions: {rxs.Count}");
            return rxs;
        }

        [MenuItem("ChemistryGame/Seed/Levels")]
        public static void SeedLevels()
        {
            var subs   = LoadAll<SubstanceData>(SUBSTANCE_DIR);
            var rxs    = LoadAll<ReactionRule>(REACTION_DIR);
            var tools  = LoadAll<ToolData>(TOOL_DIR);
            var levels = LoadAll<LevelConfig>(LEVEL_DIR);
            var hints  = LoadAll<HintBundle>(LEVEL_DIR);

            HintBundle MakeHint(string id, string t1, string t2, string t3)
            {
                if (!hints.TryGetValue(id, out var h))
                {
                    h = ScriptableObject.CreateInstance<HintBundle>();
                    AssetDatabase.CreateAsset(h, $"{LEVEL_DIR}/Hint_{id}.asset");
                    hints[id] = h;
                }
                h.Tier1_Reagent = t1; h.Tier2_Order = t2; h.Tier3_Formula = t3;
                EditorUtility.SetDirty(h);
                return h;
            }

            LevelConfig L(string fileName, int idx, string display, string objective)
            {
                if (!levels.TryGetValue(fileName, out var l))
                {
                    l = ScriptableObject.CreateInstance<LevelConfig>();
                    AssetDatabase.CreateAsset(l, $"{LEVEL_DIR}/{fileName}.asset");
                    levels[fileName] = l;
                }
                l.LevelIndex = idx; l.DisplayName = display; l.ObjectiveText = objective;
                l.Bottles.Clear(); l.Tools.Clear(); l.AvailableReactions.Clear();
                l.Traps.Clear(); l.ThreeStarBlockingTraps.Clear();
                EditorUtility.SetDirty(l);
                return l;
            }

            BottleSpawn B(string subId, float amt, bool mask = false, string maskLabel = "?")
            {
                return new BottleSpawn { Substance = subs[subId], InitialAmount = amt, MaskLabel = mask, MaskedLabel = maskLabel };
            }
            ToolSpawn T(string id) { return new ToolSpawn { Tool = tools[id] }; }
            TrapDefinition Trap(string id, string triggerSubId, string explanation) {
                return new TrapDefinition {
                    TrapId = id,
                    TriggerProduct = subs.TryGetValue(triggerSubId, out var x) ? x : null,
                    ExplanationVi = explanation
                };
            }

            // ===== Màn 1 — NaCl tinh khiết =====
            var lv1 = L("Level_01_NaCl", 1, "Điều chế NaCl tinh khiết", "Thu NaCl tinh khiết từ 3 lọ mất nhãn.");
            lv1.Bottles.Add(B("HCl",   60f, true, "Lọ A"));
            lv1.Bottles.Add(B("NaOH",  60f, true, "Lọ B"));
            lv1.Bottles.Add(B("Na2CO3",60f, true, "Lọ C"));
            lv1.Tools.Add(T("Litmus")); lv1.Tools.Add(T("Burner")); lv1.Tools.Add(T("DistilledWater"));
            lv1.AvailableReactions.Add(rxs["HCl_NaOH"]); lv1.AvailableReactions.Add(rxs["Na2CO3_HCl"]);
            // Target = NaCl tinh thể (per GDD: "Cô cạn bằng đèn khò để thu NaCl tinh thể").
            // Người chơi phải: HCl+NaOH → NaCl(dd) → khò → NaCl crystal.
            // Forbidden: tạp acid/base/carbonate. NaCl_aq KHÔNG forbidden — chỉ là intermediate.
            lv1.PurityRule.TargetProduct = subs["NaCl"];
            lv1.PurityRule.MinTargetAmount = 10f;
            lv1.PurityRule.ForbiddenSubstances = new List<SubstanceData> { subs["Na2CO3"], subs["HCl"], subs["NaOH"] };
            lv1.PurityRule.ForbiddenTolerance = 1f;
            lv1.Traps.Add(Trap("ExcessAcid", "HCl", "Dư HCl — môi trường còn acid"));
            lv1.Traps.Add(Trap("ExcessBase", "NaOH", "Dư NaOH — môi trường còn bazơ"));
            lv1.Traps.Add(Trap("Carbonate",  "Na2CO3", "Còn Na₂CO₃ — không tinh khiết"));
            lv1.ThreeStarBlockingTraps.AddRange(lv1.Traps);
            lv1.Hints = MakeHint("Lv1",
                "Dùng quỳ tím để xác định acid/bazơ trong 3 lọ.",
                "Trộn HCl với NaOH (vừa đủ), sau đó cô cạn để thu tinh thể.",
                "HCl + NaOH → NaCl + H₂O. Đong cẩn thận, dùng quỳ kiểm tra trung tính.");

            // ===== Màn 2 — AgCl tinh khiết =====
            var lv2 = L("Level_02_AgCl", 2, "Lọc AgCl tinh khiết", "Thu AgCl không lẫn kết tủa khác.");
            lv2.Bottles.Add(B("AgNO3", 60f));
            lv2.Bottles.Add(B("NaCl_aq", 60f));
            lv2.Bottles.Add(B("NaOH",  40f));
            lv2.Bottles.Add(B("Na2CO3",40f));
            lv2.Bottles.Add(B("HNO3",  30f));
            lv2.Tools.Add(T("Litmus")); lv2.Tools.Add(T("FilterPaper")); lv2.Tools.Add(T("DistilledWater"));
            lv2.AvailableReactions.Add(rxs["AgNO3_NaCl"]); lv2.AvailableReactions.Add(rxs["AgNO3_NaOH"]); lv2.AvailableReactions.Add(rxs["AgNO3_Na2CO3"]);
            lv2.PurityRule.TargetProduct = subs["AgCl"];
            lv2.PurityRule.MinTargetAmount = 5f;
            lv2.PurityRule.ForbiddenSubstances = new List<SubstanceData> { subs["Ag2O"], subs["Ag2CO3"], subs["AgNO3"] };
            lv2.PurityRule.RejectAnyPrecipitate = false;
            lv2.Traps.Add(Trap("Ag2O_trap",   "Ag2O",   "Bạn tạo Ag₂O (đen) thay vì AgCl."));
            lv2.Traps.Add(Trap("Ag2CO3_trap", "Ag2CO3", "Bạn tạo Ag₂CO₃ (vàng nhạt) thay vì AgCl."));
            lv2.ThreeStarBlockingTraps.AddRange(lv2.Traps);
            lv2.Hints = MakeHint("Lv2",
                "Chỉ 1 lọ tạo AgCl trắng. Hai lọ khác cho kết tủa màu khác.",
                "Trộn AgNO₃ với NaCl, sau đó lọc.",
                "AgNO₃ + NaCl → AgCl↓ + NaNO₃. Đừng dùng NaOH/Na₂CO₃.");

            // ===== Màn 3 — CO2 chứng minh =====
            var lv3 = L("Level_03_CO2", 3, "Điều chế CO₂", "Thu CO₂ và chứng minh bằng nước vôi.");
            lv3.Bottles.Add(B("CaCO3", 40f));
            lv3.Bottles.Add(B("Na2CO3",40f));
            lv3.Bottles.Add(B("HCl",   60f));
            lv3.Bottles.Add(B("H2SO4", 60f));
            lv3.Bottles.Add(B("CaOH2", 50f));
            lv3.Bottles.Add(B("Zn",    30f));
            lv3.Tools.Add(T("GasCollector")); lv3.Tools.Add(T("Litmus"));
            lv3.AvailableReactions.Add(rxs["CaCO3_HCl"]); lv3.AvailableReactions.Add(rxs["Zn_HCl"]); lv3.AvailableReactions.Add(rxs["CO2_CaOH2"]); lv3.AvailableReactions.Add(rxs["Na2CO3_HCl"]);
            lv3.PurityRule.TargetProduct = subs["CaCO3"];  // Chứng minh bằng tủa CaCO3 trong nước vôi → đạt
            lv3.PurityRule.MinTargetAmount = 5f;
            lv3.PurityRule.ForbiddenSubstances = new List<SubstanceData> { subs["ZnCl2"] };
            lv3.Traps.Add(Trap("WrongMetal", "ZnCl2", "Bạn dùng Zn — sinh H₂ chứ không phải CO₂."));
            lv3.ThreeStarBlockingTraps.AddRange(lv3.Traps);
            lv3.Hints = MakeHint("Lv3",
                "Quỳ tím để phân acid; chú ý CO₂ và H₂ khác nhau.",
                "Dùng CaCO₃ + HCl để sinh khí. Sục vào Ca(OH)₂.",
                "CaCO₃ + 2HCl → CaCl₂ + CO₂ + H₂O. Sau đó CO₂ + Ca(OH)₂ → CaCO₃↓ + H₂O.");

            // ===== Màn 4 — CuSO4 =====
            var lv4 = L("Level_04_CuSO4", 4, "CuSO₄ tinh khiết", "Tạo dung dịch CuSO₄ xanh, không kết tủa.");
            lv4.Bottles.Add(B("Cu",   30f));
            lv4.Bottles.Add(B("CuO",  30f));
            lv4.Bottles.Add(B("H2SO4",60f));
            lv4.Bottles.Add(B("NaOH", 40f));
            lv4.Bottles.Add(B("HCl",  40f));
            lv4.Tools.Add(T("Burner")); lv4.Tools.Add(T("FilterPaper")); lv4.Tools.Add(T("Litmus"));
            lv4.Tools.Add(T("Bubble_O2"));
            lv4.AvailableReactions.Add(rxs["CuO_H2SO4"]); lv4.AvailableReactions.Add(rxs["Cu_O2"]); lv4.AvailableReactions.Add(rxs["CuSO4_NaOH"]); lv4.AvailableReactions.Add(rxs["CuO_HCl"]);
            lv4.PurityRule.TargetProduct = subs["CuSO4"];
            lv4.PurityRule.MinTargetAmount = 10f;
            lv4.PurityRule.ForbiddenSubstances = new List<SubstanceData> { subs["CuOH2"], subs["CuCl2"], subs["NaOH"], subs["HCl"] };
            lv4.Traps.Add(Trap("Hydroxide", "CuOH2", "Bạn tạo Cu(OH)₂ — kết tủa xanh."));
            lv4.Traps.Add(Trap("WrongSalt", "CuCl2", "Bạn tạo CuCl₂ thay vì CuSO₄."));
            lv4.ThreeStarBlockingTraps.AddRange(lv4.Traps);
            lv4.Hints = MakeHint("Lv4",
                "H₂SO₄ là acid sunfat — chỉ acid này tạo muối sunfat đúng.",
                "Đường ngắn: CuO + H₂SO₄. Đường khó (3⭐): Cu → CuO → CuSO₄.",
                "CuO + H₂SO₄ → CuSO₄ + H₂O. Tránh NaOH (tạo Cu(OH)₂).");

            // ===== Màn 5 — FeCl3 =====
            var lv5 = L("Level_05_FeCl3", 5, "FeCl₃ không lẫn FeCl₂", "Tạo FeCl₃, kiểm bằng KSCN cho đỏ máu.");
            lv5.Bottles.Add(B("Fe",    30f));
            lv5.Bottles.Add(B("HCl",   60f));
            lv5.Bottles.Add(B("NaOH",  30f));
            lv5.Tools.Add(T("Litmus")); lv5.Tools.Add(T("KSCN"));
            lv5.Tools.Add(T("Bubble_Cl2"));
            lv5.AvailableReactions.Add(rxs["Fe_HCl"]); lv5.AvailableReactions.Add(rxs["FeCl2_Cl2"]); lv5.AvailableReactions.Add(rxs["FeCl2_NaOH"]); lv5.AvailableReactions.Add(rxs["FeCl3_NaOH"]); lv5.AvailableReactions.Add(rxs["FeCl3_KSCN"]);
            lv5.PurityRule.TargetProduct = subs["FeCl3"];
            lv5.PurityRule.MinTargetAmount = 8f;
            lv5.PurityRule.ForbiddenSubstances = new List<SubstanceData> { subs["FeCl2"], subs["FeOH2"], subs["FeOH3"], subs["NaOH"] };
            lv5.Traps.Add(Trap("LeftOverFe2", "FeCl2", "Còn FeCl₂ — sục thêm Cl₂."));
            lv5.Traps.Add(Trap("Hydroxide_Fe2", "FeOH2", "Bạn tạo Fe(OH)₂ (xanh lá) — tránh NaOH."));
            lv5.Traps.Add(Trap("Hydroxide_Fe3", "FeOH3", "Bạn tạo Fe(OH)₃ (nâu đỏ) — tránh NaOH."));
            lv5.ThreeStarBlockingTraps.AddRange(lv5.Traps);
            lv5.Hints = MakeHint("Lv5",
                "Fe + HCl chỉ tạo FeCl₂ (sắt II). Cần oxi hoá thêm.",
                "Sục Cl₂ đủ để chuyển FeCl₂ → FeCl₃. Test KSCN cho đỏ máu.",
                "Fe + 2HCl → FeCl₂ + H₂. Sau đó 2FeCl₂ + Cl₂ → 2FeCl₃. KSCN xác nhận Fe³⁺.");

            // Save
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ChemistrySeeder] Levels: {levels.Count}");
        }

        private static Dictionary<string, T> LoadAll<T>(string folder) where T : ScriptableObject
        {
            var dict = new Dictionary<string, T>();
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var so = AssetDatabase.LoadAssetAtPath<T>(path);
                if (so == null) continue;
                var key = System.IO.Path.GetFileNameWithoutExtension(path);
                if (key.StartsWith("Sub_")) key = key.Substring(4);
                else if (key.StartsWith("Tool_")) key = key.Substring(5);
                else if (key.StartsWith("Rx_")) key = key.Substring(3);
                else if (key.StartsWith("Hint_")) key = key.Substring(5);
                dict[key] = so;
            }
            return dict;
        }
    }
}
#endif
