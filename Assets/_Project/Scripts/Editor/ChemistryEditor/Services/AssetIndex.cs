using System.Collections.Generic;
using ChemistryGame.Chemistry;
using UnityEditor;
using UnityEngine;

namespace ChemistryGame.EditorTools.ContentEditor
{
    /// <summary>Read-only surface used by Validator and quick actions — allows fake implementations in tests.</summary>
    public interface IAssetIndexReadOnly
    {
        IReadOnlyList<SubstanceData> GetAllSubstances();
        IReadOnlyList<ReactionRule>  GetAllReactions();
        IReadOnlyList<LevelConfig>   GetAllLevels();
        SubstanceData GetSubstanceById(string id);
        IReadOnlyList<ReactionRule>  GetReactionsUsingInput(SubstanceData s);
        IReadOnlyList<ReactionRule>  GetReactionsProducing(SubstanceData s);
        IReadOnlyList<LevelConfig>   GetLevelsUsingSubstance(SubstanceData s);
        IReadOnlyList<LevelConfig>   GetLevelsUsingReaction(ReactionRule r);
    }

    /// <summary>
    /// In-memory cache of all Substance/Reaction/Level assets, with reverse-reference maps.
    /// Rebuilt automatically by AssetIndexPostprocessor on asset import/delete/move.
    /// </summary>
    public class AssetIndex : IAssetIndexReadOnly
    {
        public event System.Action OnIndexChanged;

        private List<SubstanceData> _substances = new List<SubstanceData>();
        private List<ReactionRule>  _reactions  = new List<ReactionRule>();
        private List<LevelConfig>   _levels     = new List<LevelConfig>();

        private readonly Dictionary<string, SubstanceData> _substanceById = new Dictionary<string, SubstanceData>();
        private readonly Dictionary<SubstanceData, List<ReactionRule>> _reactionsByInput  = new Dictionary<SubstanceData, List<ReactionRule>>();
        private readonly Dictionary<SubstanceData, List<ReactionRule>> _reactionsByOutput = new Dictionary<SubstanceData, List<ReactionRule>>();
        private readonly Dictionary<SubstanceData, List<LevelConfig>>  _levelsBySubstance = new Dictionary<SubstanceData, List<LevelConfig>>();
        private readonly Dictionary<ReactionRule,  List<LevelConfig>>  _levelsByReaction  = new Dictionary<ReactionRule,  List<LevelConfig>>();

        private bool _dirty = true;

        public void Invalidate()
        {
            _dirty = true;
            OnIndexChanged?.Invoke();
        }

        public void Rebuild()
        {
            _substances = LoadAll<SubstanceData>();
            _reactions  = LoadAll<ReactionRule>();
            _levels     = LoadAll<LevelConfig>();

            _substanceById.Clear();
            foreach (var s in _substances)
                if (s != null && !string.IsNullOrEmpty(s.Id) && !_substanceById.ContainsKey(s.Id))
                    _substanceById.Add(s.Id, s);

            _reactionsByInput.Clear();
            _reactionsByOutput.Clear();
            foreach (var r in _reactions)
            {
                if (r == null) continue;
                if (r.Inputs != null)
                    foreach (var st in r.Inputs)
                        if (st != null && st.Substance != null) AddToMap(_reactionsByInput, st.Substance, r);
                if (r.Outputs != null)
                    foreach (var st in r.Outputs)
                        if (st != null && st.Substance != null) AddToMap(_reactionsByOutput, st.Substance, r);
            }

            _levelsBySubstance.Clear();
            _levelsByReaction.Clear();
            foreach (var l in _levels)
            {
                if (l == null) continue;
                if (l.Bottles != null)
                    foreach (var b in l.Bottles)
                        if (b != null && b.Substance != null) AddToMap(_levelsBySubstance, b.Substance, l);
                if (l.AvailableReactions != null)
                    foreach (var rx in l.AvailableReactions)
                        if (rx != null) AddToMap(_levelsByReaction, rx, l);
            }

            _dirty = false;
        }

        private void EnsureFresh() { if (_dirty) Rebuild(); }

        public IReadOnlyList<SubstanceData> GetAllSubstances() { EnsureFresh(); return _substances; }
        public IReadOnlyList<ReactionRule>  GetAllReactions()  { EnsureFresh(); return _reactions; }
        public IReadOnlyList<LevelConfig>   GetAllLevels()     { EnsureFresh(); return _levels; }

        public SubstanceData GetSubstanceById(string id)
        {
            EnsureFresh();
            return id != null && _substanceById.TryGetValue(id, out var s) ? s : null;
        }

        public IReadOnlyList<ReactionRule> GetReactionsUsingInput(SubstanceData s)
        {
            EnsureFresh();
            return s != null && _reactionsByInput.TryGetValue(s, out var list) ? (IReadOnlyList<ReactionRule>)list : System.Array.Empty<ReactionRule>();
        }

        public IReadOnlyList<ReactionRule> GetReactionsProducing(SubstanceData s)
        {
            EnsureFresh();
            return s != null && _reactionsByOutput.TryGetValue(s, out var list) ? (IReadOnlyList<ReactionRule>)list : System.Array.Empty<ReactionRule>();
        }

        public IReadOnlyList<LevelConfig> GetLevelsUsingSubstance(SubstanceData s)
        {
            EnsureFresh();
            return s != null && _levelsBySubstance.TryGetValue(s, out var list) ? (IReadOnlyList<LevelConfig>)list : System.Array.Empty<LevelConfig>();
        }

        public IReadOnlyList<LevelConfig> GetLevelsUsingReaction(ReactionRule r)
        {
            EnsureFresh();
            return r != null && _levelsByReaction.TryGetValue(r, out var list) ? (IReadOnlyList<LevelConfig>)list : System.Array.Empty<LevelConfig>();
        }

        private static List<T> LoadAll<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var list = new List<T>(guids.Length);
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var obj = AssetDatabase.LoadAssetAtPath<T>(path);
                if (obj != null) list.Add(obj);
            }
            return list;
        }

        private static void AddToMap<K, V>(Dictionary<K, List<V>> map, K key, V value)
        {
            if (!map.TryGetValue(key, out var list)) { list = new List<V>(); map[key] = list; }
            if (!list.Contains(value)) list.Add(value);
        }
    }
}
