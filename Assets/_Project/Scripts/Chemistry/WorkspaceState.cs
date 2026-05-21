using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryGame.Chemistry
{
    /// <summary>
    /// State của ống nghiệm chính tại thời điểm bất kỳ.
    /// Pure C# — không kế thừa MonoBehaviour, dễ unit test.
    /// </summary>
    public class WorkspaceState
    {
        // SubstanceData -> Amount
        private readonly Dictionary<SubstanceData, Amount> _contents = new Dictionary<SubstanceData, Amount>();

        // History các action (để undo + log)
        private readonly List<ActionRecord> _history = new List<ActionRecord>();

        // Flag conditions
        public bool IsHeated { get; set; }
        public float Temperature { get; set; } = 25f; // C
        public bool IsFiltered { get; set; }
        public bool HasGasCollector { get; set; }
        public List<SubstanceData> CollectedGases { get; } = new List<SubstanceData>();

        public IReadOnlyDictionary<SubstanceData, Amount> Contents => _contents;
        public IReadOnlyList<ActionRecord> History => _history;

        public bool IsEmpty => _contents.Count == 0 || _contents.Values.All(a => a.IsZero);

        public Amount GetAmount(SubstanceData s)
        {
            if (s == null) return Amount.Zero;
            return _contents.TryGetValue(s, out var amt) ? amt : Amount.Zero;
        }

        public bool Contains(SubstanceData s) => GetAmount(s).IsPositive;

        public void Add(SubstanceData s, Amount amount)
        {
            if (s == null || amount.IsZero) return;
            _contents[s] = GetAmount(s) + amount;
        }

        public void Consume(SubstanceData s, Amount amount)
        {
            if (s == null || amount.IsZero) return;
            var remaining = GetAmount(s) - amount;
            if (remaining.IsZero) _contents.Remove(s);
            else _contents[s] = remaining;
        }

        public void RecordAction(ActionRecord action)
        {
            _history.Add(action);
        }

        /// <summary>Tính pH gần đúng — acid mạnh nhất hoặc base mạnh nhất chi phối.</summary>
        public float CalculatePH()
        {
            float acidContribution = 7f;
            float baseContribution = 7f;
            bool hasAcid = false, hasBase = false;

            foreach (var kv in _contents)
            {
                if (kv.Value.IsZero) continue;
                if (kv.Key.IsAcid && kv.Key.PH < acidContribution)
                {
                    acidContribution = kv.Key.PH; hasAcid = true;
                }
                else if (kv.Key.IsBase && kv.Key.PH > baseContribution)
                {
                    baseContribution = kv.Key.PH; hasBase = true;
                }
            }

            if (hasAcid && hasBase) return 7f; // trung hoà gần như
            if (hasAcid) return acidContribution;
            if (hasBase) return baseContribution;
            return 7f;
        }

        public WorkspaceState Clone()
        {
            var c = new WorkspaceState
            {
                IsHeated = IsHeated,
                Temperature = Temperature,
                IsFiltered = IsFiltered,
                HasGasCollector = HasGasCollector
            };
            foreach (var kv in _contents) c._contents[kv.Key] = kv.Value;
            c.CollectedGases.AddRange(CollectedGases);
            c._history.AddRange(_history);
            return c;
        }

        public void Clear()
        {
            _contents.Clear();
            _history.Clear();
            IsHeated = false; Temperature = 25f;
            IsFiltered = false;
            HasGasCollector = false;
            CollectedGases.Clear();
        }
    }

    [Serializable]
    public struct ActionRecord
    {
        public string Action;
        public SubstanceData Subject;
        public float Amount;
        public float Timestamp;
        public override string ToString() => $"[{Timestamp:F1}s] {Action} {Subject?.Id} ({Amount:F1})";
    }
}
