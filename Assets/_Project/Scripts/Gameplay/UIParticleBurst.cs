using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.Gameplay
{
    /// <summary>
    /// UI-friendly particle burst. Renders trong cùng Canvas (ScreenSpaceOverlay-compatible)
    /// — KHÔNG dùng ParticleSystem (bị overlay canvas che).
    ///
    /// Spawn N Image instances, mỗi cái có DOTween-driven motion + fade. Khi tween xong
    /// auto destroy. Hỗ trợ các pattern: rise (bốc hơi), fall (kết tủa rơi),
    /// burst (sparkle explode), bubble (rise wavy).
    /// </summary>
    public enum BurstPattern
    {
        Rise,       // bốc hơi: lên + fade
        Fall,       // kết tủa: rơi xuống + fade
        Burst,      // sparkle: nổ tung tròn + fade
        Bubble,     // bọt khí: rise + hơi lượn
        Drop        // giọt: rơi nhanh + fade
    }

    [System.Serializable]
    public class BurstSpec
    {
        public BurstPattern Pattern = BurstPattern.Rise;
        public int Count = 12;
        public Color StartColor = Color.white;
        public Color EndColor = new Color(1, 1, 1, 0);
        public float SizeMin = 8f;
        public float SizeMax = 18f;
        public float Distance = 80f;
        public float Duration = 1.3f;
        public float Spread = 50f; // bán kính spawn ban đầu (px)
    }

    public class UIParticleBurst : MonoBehaviour
    {
        /// <summary>Spawn 1 burst tại worldPos. parentCanvas dùng để xác định coordinate space.</summary>
        public static void Play(BurstSpec spec, RectTransform parent, Vector2 anchoredPos, Sprite sprite = null)
        {
            if (spec == null || parent == null) return;
            for (int i = 0; i < spec.Count; i++)
            {
                var go = new GameObject($"P_{spec.Pattern}_{i}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
                var rt = (RectTransform)go.transform;
                rt.SetParent(parent, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                var size = Random.Range(spec.SizeMin, spec.SizeMax);
                rt.sizeDelta = new Vector2(size, size);

                // Spawn position: anchoredPos + random within spread
                var spawn = anchoredPos + Random.insideUnitCircle * spec.Spread;
                rt.anchoredPosition = spawn;

                var img = go.GetComponent<Image>();
                img.sprite = sprite;
                img.color = spec.StartColor;
                img.raycastTarget = false;

                var cg = go.GetComponent<CanvasGroup>();
                cg.alpha = spec.StartColor.a;

                Vector2 target;
                Ease ease;
                switch (spec.Pattern)
                {
                    case BurstPattern.Rise:
                        target = spawn + new Vector2(Random.Range(-20f, 20f), spec.Distance);
                        ease = Ease.OutQuad;
                        break;
                    case BurstPattern.Fall:
                        target = spawn + new Vector2(Random.Range(-30f, 30f), -spec.Distance);
                        ease = Ease.InQuad;
                        break;
                    case BurstPattern.Burst:
                        var dir = Random.insideUnitCircle.normalized * spec.Distance;
                        target = spawn + dir;
                        ease = Ease.OutCubic;
                        break;
                    case BurstPattern.Bubble:
                        target = spawn + new Vector2(Random.Range(-15f, 15f), spec.Distance * 0.8f);
                        ease = Ease.InOutSine;
                        break;
                    case BurstPattern.Drop:
                    default:
                        target = spawn + new Vector2(Random.Range(-10f, 10f), -spec.Distance * 1.2f);
                        ease = Ease.InQuad;
                        break;
                }

                rt.DOAnchorPos(target, spec.Duration).SetEase(ease);
                rt.DOScale(0.4f, spec.Duration).SetEase(Ease.InQuad);
                cg.DOFade(spec.EndColor.a, spec.Duration).SetEase(Ease.InQuad);
                img.DOColor(spec.EndColor, spec.Duration).SetEase(Ease.Linear);

                // Bubble pattern: add slight sideways wobble
                if (spec.Pattern == BurstPattern.Bubble)
                {
                    rt.DOAnchorPosX(spawn.x + Random.Range(-15f, 15f), spec.Duration * 0.5f)
                      .SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo);
                }

                Destroy(go, spec.Duration + 0.1f);
            }
        }
    }
}
