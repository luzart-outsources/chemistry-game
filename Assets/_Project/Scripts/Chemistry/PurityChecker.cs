using System.Collections.Generic;

namespace ChemistryGame.Chemistry
{
    public class PurityResult
    {
        public bool TargetProductCreated;
        public bool IsPure;
        public List<SubstanceData> Impurities = new List<SubstanceData>();
        public string Reason;
    }

    public class PurityChecker
    {
        public PurityResult Check(WorkspaceState state, PurityRule rule)
        {
            var result = new PurityResult();
            if (rule == null || rule.TargetProduct == null)
            {
                result.Reason = "Không có rule";
                return result;
            }

            var targetAmount = state.GetAmount(rule.TargetProduct).Value;
            result.TargetProductCreated = targetAmount >= rule.MinTargetAmount;

            if (!result.TargetProductCreated)
            {
                result.Reason = $"Chưa tạo được {rule.TargetProduct.Formula}";
                return result;
            }

            // Check tạp chất
            result.IsPure = true;
            foreach (var forbidden in rule.ForbiddenSubstances)
            {
                if (forbidden == null) continue;
                if (state.GetAmount(forbidden).Value > rule.ForbiddenTolerance)
                {
                    result.Impurities.Add(forbidden);
                    result.IsPure = false;
                }
            }

            // Check kết tủa bất kỳ
            if (rule.RejectAnyPrecipitate)
            {
                foreach (var kv in state.Contents)
                {
                    if (kv.Key == rule.TargetProduct) continue;
                    if (kv.Key.IsPrecipitate && kv.Value.Value > rule.ForbiddenTolerance)
                    {
                        if (!result.Impurities.Contains(kv.Key))
                            result.Impurities.Add(kv.Key);
                        result.IsPure = false;
                    }
                }
            }

            if (!result.IsPure)
            {
                var names = string.Join(", ", result.Impurities.ConvertAll(x => x.Formula));
                result.Reason = $"Còn tạp: {names}";
            }
            else
            {
                result.Reason = "Sản phẩm tinh khiết";
            }
            return result;
        }
    }
}
