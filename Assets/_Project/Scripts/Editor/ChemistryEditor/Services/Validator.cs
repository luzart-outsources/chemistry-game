using System.Collections.Generic;
using System.Linq;
using ChemistryGame.Chemistry;
using UnityEditor;

namespace ChemistryGame.EditorTools.ContentEditor
{
    /// <summary>Pure validation rules. No GUI, no asset writing.</summary>
    public static class Validator
    {
        public static List<ValidationIssue> ValidateSubstance(SubstanceData s, IAssetIndexReadOnly idx)
        {
            var list = new List<ValidationIssue>();
            if (s == null) return list;

            if (string.IsNullOrEmpty(s.Id))
                list.Add(ValidationIssue.Error("SUB_ID_EMPTY", "Id rỗng — chất cần Id duy nhất."));
            else if (s.Id.Trim() != s.Id || s.Id.Contains(" "))
                list.Add(ValidationIssue.Warning("SUB_ID_HAS_WHITESPACE", $"Id '{s.Id}' chứa khoảng trắng."));

            if (!string.IsNullOrEmpty(s.Id))
            {
                var dups = idx.GetAllSubstances().Where(x => x != null && x != s && x.Id == s.Id).ToList();
                if (dups.Count > 0)
                    list.Add(ValidationIssue.Error("SUB_ID_DUPLICATE", $"Id '{s.Id}' trùng với {dups.Count} chất khác."));
            }

            var path = AssetDatabase.GetAssetPath(s);
            if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(s.Id))
            {
                var fname = System.IO.Path.GetFileNameWithoutExtension(path);
                var expected = $"Sub_{s.Id}";
                if (fname != expected)
                    list.Add(ValidationIssue.Warning("SUB_FILENAME_MISMATCH", $"Tên file '{fname}' khác Sub_{s.Id}."));
            }

            if (idx.GetReactionsUsingInput(s).Count == 0 && idx.GetReactionsProducing(s).Count == 0)
                list.Add(ValidationIssue.Warning("SUB_ORPHAN", "Chất không xuất hiện trong phản ứng nào."));

            if (s.Phase == SubstancePhase.Aqueous && s.CrystalForm == null)
                list.Add(ValidationIssue.Info("SUB_AQUEOUS_NO_CRYSTAL", "Aqueous nhưng chưa có CrystalForm — không cô cạn được bằng đèn khò."));

            return list;
        }

        public static List<ValidationIssue> ValidateReaction(ReactionRule r, IAssetIndexReadOnly idx)
        {
            var list = new List<ValidationIssue>();
            if (r == null) return list;

            if (r.Inputs == null || r.Inputs.Count == 0)
                list.Add(ValidationIssue.Error("RX_INPUTS_EMPTY", "Reaction không có Input nào."));
            if (r.Outputs == null || r.Outputs.Count == 0)
                list.Add(ValidationIssue.Error("RX_OUTPUTS_EMPTY", "Reaction không có Output nào."));

            if (r.Inputs != null)
                for (int i = 0; i < r.Inputs.Count; i++)
                {
                    var st = r.Inputs[i];
                    if (st == null || st.Substance == null)
                        list.Add(ValidationIssue.Error("RX_INPUT_NULL", $"Input #{i + 1} chưa chọn chất."));
                    else if (st.Ratio <= 0)
                        list.Add(ValidationIssue.Error("RX_RATIO_NONPOSITIVE", $"Input '{st.Substance.Id}' có Ratio ≤ 0."));
                }

            if (r.Outputs != null)
                for (int i = 0; i < r.Outputs.Count; i++)
                {
                    var st = r.Outputs[i];
                    if (st == null || st.Substance == null)
                        list.Add(ValidationIssue.Error("RX_OUTPUT_NULL", $"Output #{i + 1} chưa chọn chất."));
                    else if (st.Ratio <= 0)
                        list.Add(ValidationIssue.Error("RX_RATIO_NONPOSITIVE", $"Output '{st.Substance.Id}' có Ratio ≤ 0."));
                }

            return list;
        }

        public static List<ValidationIssue> ValidateLevel(LevelConfig l, IAssetIndexReadOnly idx)
        {
            var list = new List<ValidationIssue>();
            if (l == null) return list;

            var dups = idx.GetAllLevels().Where(x => x != null && x != l && x.LevelIndex == l.LevelIndex).ToList();
            if (dups.Count > 0)
                list.Add(ValidationIssue.Error("LV_INDEX_DUPLICATE", $"LevelIndex {l.LevelIndex} trùng với level khác."));

            if (l.PurityRule == null || l.PurityRule.TargetProduct == null)
                list.Add(ValidationIssue.Error("LV_TARGET_NULL", "PurityRule.TargetProduct chưa set."));
            else if (l.PurityRule.ForbiddenSubstances != null && l.PurityRule.ForbiddenSubstances.Contains(l.PurityRule.TargetProduct))
                list.Add(ValidationIssue.Error("LV_TARGET_IN_FORBIDDEN", "Target product nằm trong ForbiddenSubstances — mâu thuẫn."));

            if (l.Traps != null)
                for (int i = 0; i < l.Traps.Count; i++)
                    if (l.Traps[i] == null || l.Traps[i].TriggerProduct == null)
                        list.Add(ValidationIssue.Error("LV_TRAP_TRIGGER_NULL", $"Trap #{i + 1} không có TriggerProduct."));

            if (l.Bottles == null || l.Bottles.Count == 0)
                list.Add(ValidationIssue.Warning("LV_NO_BOTTLES", "Level không có lọ chất nào."));
            if (l.Tools == null || l.Tools.Count == 0)
                list.Add(ValidationIssue.Warning("LV_NO_TOOLS", "Level không có công cụ nào."));
            if (l.Hints == null)
                list.Add(ValidationIssue.Warning("LV_HINTS_NULL", "Level chưa có HintBundle."));

            return list;
        }
    }
}
