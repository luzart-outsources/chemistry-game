using System.Collections.Generic;
using System.Linq;

namespace ChemistryGame.Chemistry
{
    public class ReactionMatch
    {
        public ReactionRule Rule;
        public float LimitingFactor; // tỉ lệ phản ứng có thể xảy ra (0..1+)
    }

    /// <summary>
    /// Tìm các phản ứng có thể xảy ra ở thời điểm hiện tại trong workspace.
    /// Pure logic — không depends Unity beyond ScriptableObject reference.
    /// </summary>
    public class ReactionMatcher
    {
        private readonly List<ReactionRule> _rules;

        public ReactionMatcher(IEnumerable<ReactionRule> rules)
        {
            _rules = rules?.Where(r => r != null).ToList() ?? new List<ReactionRule>();
        }

        public List<ReactionMatch> FindMatches(WorkspaceState state)
        {
            var matches = new List<ReactionMatch>();
            foreach (var rule in _rules)
            {
                if (!CheckConditions(rule, state)) continue;
                var factor = CalculateLimitingFactor(rule, state);
                if (factor <= 0f) continue;
                matches.Add(new ReactionMatch { Rule = rule, LimitingFactor = factor });
            }
            return matches;
        }

        private bool CheckConditions(ReactionRule rule, WorkspaceState state)
        {
            // Inputs phải hiện diện đủ
            foreach (var input in rule.Inputs)
            {
                if (input?.Substance == null) continue;
                if (!state.Contains(input.Substance)) return false;
            }

            // Conditions (heat, light, ...)
            foreach (var cond in rule.Conditions)
            {
                switch (cond.Type)
                {
                    case ReactionConditionType.Heat:
                        if (!state.IsHeated) return false;
                        break;
                    case ReactionConditionType.BubbleGasThrough:
                        // tự bubbler đã add gas vào state, không cần check riêng
                        break;
                }
            }
            return true;
        }

        private float CalculateLimitingFactor(ReactionRule rule, WorkspaceState state)
        {
            float min = float.MaxValue;
            foreach (var input in rule.Inputs)
            {
                if (input?.Substance == null) continue;
                var have = state.GetAmount(input.Substance).Value;
                var need = input.Ratio;
                if (need <= 0f) continue;
                var possible = have / need;
                if (possible < min) min = possible;
            }
            return min == float.MaxValue ? 0f : min;
        }
    }
}
