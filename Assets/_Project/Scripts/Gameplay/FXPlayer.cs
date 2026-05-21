using System.Collections.Generic;
using ChemistryGame.Chemistry;
using UnityEngine;

namespace ChemistryGame.Gameplay
{
    /// <summary>Map SideEffectType → particle prefab + play tại world position của tube.</summary>
    public class FXPlayer : MonoBehaviour
    {
        [System.Serializable]
        public class FXEntry
        {
            public SideEffectType Type;
            public GameObject Prefab;
            public float Lifetime = 2f;
            [Range(0f, 2f)] public float Scale = 1f;
        }

        [SerializeField] private List<FXEntry> entries = new List<FXEntry>();
        [SerializeField] private Transform spawnAnchor;

        private readonly Dictionary<SideEffectType, FXEntry> _byType = new Dictionary<SideEffectType, FXEntry>();

        private void Awake()
        {
            foreach (var e in entries)
                if (e?.Prefab != null) _byType[e.Type] = e;
        }

        public void Play(SideEffectType type, Vector3? worldPos = null)
        {
            if (type == SideEffectType.None) return;
            if (!_byType.TryGetValue(type, out var entry)) return;

            var pos = worldPos ?? (spawnAnchor != null ? spawnAnchor.position : transform.position);
            var go = Instantiate(entry.Prefab, pos, Quaternion.identity);
            go.transform.localScale *= entry.Scale;
            if (entry.Lifetime > 0f) Destroy(go, entry.Lifetime);
        }
    }
}
