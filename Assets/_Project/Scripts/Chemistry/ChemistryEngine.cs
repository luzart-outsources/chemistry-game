using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame.Chemistry
{
    public class ReactionEvent
    {
        public ReactionRule Rule;
        public Dictionary<SubstanceData, float> Consumed = new Dictionary<SubstanceData, float>();
        public Dictionary<SubstanceData, float> Produced = new Dictionary<SubstanceData, float>();
        public float Factor;
    }

    public class StateChangeEvent
    {
        public WorkspaceState State;
        public List<ReactionEvent> RecentReactions = new List<ReactionEvent>();
    }

    /// <summary>
    /// Engine xử lý phản ứng. Pure C#.
    /// Workflow:
    ///   1. Caller gọi AddSubstance/ApplyHeat/Filter/Bubble/...
    ///   2. Engine auto-run reactions cho đến khi không còn match (steady state).
    ///   3. Engine emit ReactionOccurred + StateChanged events.
    /// </summary>
    public class ChemistryEngine
    {
        private readonly WorkspaceState _state;
        private ReactionMatcher _matcher;

        public WorkspaceState State => _state;

        public event Action<ReactionEvent> ReactionOccurred;
        public event Action<StateChangeEvent> StateChanged;

        public ChemistryEngine(IEnumerable<ReactionRule> rules)
        {
            _state = new WorkspaceState();
            _matcher = new ReactionMatcher(rules);
        }

        public void SetRules(IEnumerable<ReactionRule> rules)
        {
            _matcher = new ReactionMatcher(rules);
        }

        public void Reset()
        {
            _state.Clear();
            EmitStateChanged(new List<ReactionEvent>());
        }

        public void AddSubstance(SubstanceData s, float amount)
        {
            if (s == null || amount <= 0f) return;
            _state.Add(s, new Amount(amount));
            _state.RecordAction(new ActionRecord
            {
                Action = "Add", Subject = s, Amount = amount, Timestamp = Time.time
            });
            RunReactionsToSteadyState();
        }

        public void ApplyHeat(bool on, float seconds = 1f)
        {
            _state.IsHeated = on;
            _state.Temperature = on ? 600f : 25f;
            _state.RecordAction(new ActionRecord
            {
                Action = on ? "HeatOn" : "HeatOff",
                Timestamp = Time.time
            });
            RunReactionsToSteadyState();
        }

        public void Filter()
        {
            _state.IsFiltered = true;
            // remove all solid precipitates from solution (giữ riêng để check purity)
            // ở đây chỉ flag — view có thể giữ kết tủa hay không tuỳ visual; engine giữ data đầy đủ.
            _state.RecordAction(new ActionRecord { Action = "Filter", Timestamp = Time.time });
            EmitStateChanged(new List<ReactionEvent>());
        }

        public void BubbleGas(SubstanceData gas, float amount)
        {
            if (gas == null) return;
            _state.Add(gas, new Amount(amount));
            _state.RecordAction(new ActionRecord
            {
                Action = "BubbleGas", Subject = gas, Amount = amount, Timestamp = Time.time
            });
            RunReactionsToSteadyState();
        }

        public void CollectGasInto()
        {
            _state.HasGasCollector = true;
            // Tìm và move các chất gas ra collector
            var toMove = new List<SubstanceData>();
            foreach (var kv in _state.Contents)
                if (kv.Key.IsGas && kv.Value.IsPositive) toMove.Add(kv.Key);

            foreach (var g in toMove)
            {
                _state.CollectedGases.Add(g);
                _state.Consume(g, _state.GetAmount(g));
            }
            EmitStateChanged(new List<ReactionEvent>());
        }

        public void EvaporateWater()
        {
            // Cô cạn HẾT ngay lập tức (legacy). Cho continuous evaporation, dùng EvaporateStep.
            var toRemove = new List<SubstanceData>();
            foreach (var kv in _state.Contents)
            {
                if (kv.Key.Phase == SubstancePhase.Liquid &&
                    kv.Key.Category == SubstanceCategoryType.Solvent)
                    toRemove.Add(kv.Key);
            }
            foreach (var k in toRemove) _state.Consume(k, _state.GetAmount(k));
            _state.RecordAction(new ActionRecord { Action = "Evaporate", Timestamp = Time.time });
            EmitStateChanged(new List<ReactionEvent>());
        }

        /// <summary>
        /// Bốc hơi từng phần — gọi mỗi frame với amount = rate * Time.deltaTime.
        /// Trả về tổng nước thực sự bốc hơi (có thể nhỏ hơn amount nếu hết nước).
        /// </summary>
        public float EvaporateStep(float amount)
        {
            if (amount <= 0f) return 0f;
            float remaining = amount;
            float evaporated = 0f;
            // Snapshot keys vì dictionary có thể bị modify trong loop.
            var solvents = new List<SubstanceData>();
            foreach (var kv in _state.Contents)
            {
                if (kv.Key.Phase == SubstancePhase.Liquid &&
                    kv.Key.Category == SubstanceCategoryType.Solvent)
                    solvents.Add(kv.Key);
            }
            foreach (var s in solvents)
            {
                if (remaining <= 0f) break;
                var avail = _state.GetAmount(s).Value;
                var consume = Mathf.Min(remaining, avail);
                if (consume <= 0f) continue;
                _state.Consume(s, new Amount(consume));
                remaining -= consume;
                evaporated += consume;
            }
            if (evaporated > 0f) EmitStateChanged(new List<ReactionEvent>());
            return evaporated;
        }

        public bool HasEvaporableSolvent()
        {
            foreach (var kv in _state.Contents)
            {
                if (kv.Key.Phase == SubstancePhase.Liquid &&
                    kv.Key.Category == SubstanceCategoryType.Solvent &&
                    kv.Value.IsPositive)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Kết tinh từng phần: chuyển Aqueous → CrystalForm theo tốc độ.
        /// Chỉ áp dụng cho substances có Phase=Aqueous + CrystalForm khác null.
        /// Trả về lượng đã kết tinh.
        /// </summary>
        public float CrystallizeStep(float amount)
        {
            if (amount <= 0f) return 0f;
            float remaining = amount;
            float total = 0f;
            var aqs = new List<SubstanceData>();
            foreach (var kv in _state.Contents)
            {
                if (kv.Key.Phase == SubstancePhase.Aqueous &&
                    kv.Key.CrystalForm != null &&
                    kv.Value.IsPositive)
                    aqs.Add(kv.Key);
            }
            foreach (var s in aqs)
            {
                if (remaining <= 0f) break;
                var avail = _state.GetAmount(s).Value;
                var consume = Mathf.Min(remaining, avail);
                if (consume <= 0f) continue;
                _state.Consume(s, new Amount(consume));
                _state.Add(s.CrystalForm, new Amount(consume));
                remaining -= consume;
                total += consume;
            }
            if (total > 0f)
            {
                _state.RecordAction(new ActionRecord { Action = "Crystallize", Timestamp = Time.time });
                EmitStateChanged(new List<ReactionEvent>());
            }
            return total;
        }

        public bool HasAqueousToCrystallize()
        {
            foreach (var kv in _state.Contents)
            {
                if (kv.Key.Phase == SubstancePhase.Aqueous &&
                    kv.Key.CrystalForm != null &&
                    kv.Value.IsPositive)
                    return true;
            }
            return false;
        }

        private void RunReactionsToSteadyState(int maxIterations = 20)
        {
            var recent = new List<ReactionEvent>();
            for (int i = 0; i < maxIterations; i++)
            {
                var matches = _matcher.FindMatches(_state);
                if (matches.Count == 0) break;

                // Chọn match có factor cao nhất (đơn giản hoá; có thể priority hoá thêm)
                ReactionMatch best = null;
                foreach (var m in matches)
                {
                    if (best == null || m.LimitingFactor > best.LimitingFactor) best = m;
                }
                if (best == null || best.LimitingFactor <= 0f) break;

                var evt = ApplyReaction(best);
                recent.Add(evt);
                ReactionOccurred?.Invoke(evt);
            }
            EmitStateChanged(recent);
        }

        private ReactionEvent ApplyReaction(ReactionMatch m)
        {
            var evt = new ReactionEvent { Rule = m.Rule, Factor = m.LimitingFactor };
            foreach (var input in m.Rule.Inputs)
            {
                if (input?.Substance == null) continue;
                var used = input.Ratio * m.LimitingFactor;
                _state.Consume(input.Substance, new Amount(used));
                evt.Consumed[input.Substance] = used;
            }
            foreach (var output in m.Rule.Outputs)
            {
                if (output?.Substance == null) continue;
                var produced = output.Ratio * m.LimitingFactor;
                _state.Add(output.Substance, new Amount(produced));
                evt.Produced[output.Substance] = produced;
            }
            return evt;
        }

        private void EmitStateChanged(List<ReactionEvent> recent)
        {
            StateChanged?.Invoke(new StateChangeEvent { State = _state, RecentReactions = recent });
        }
    }
}
