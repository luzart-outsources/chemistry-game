using ChemistryGame.Chemistry;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.Gameplay
{
    /// <summary>
    /// Maps <see cref="SideEffectType"/> → <see cref="UIParticleBurst"/> với spec phù hợp.
    /// Drop-in replacement cho FXPlayer ở tube/bottle context (ScreenSpaceOverlay canvas).
    /// </summary>
    public class ChemistryFXController : MonoBehaviour
    {
        [SerializeField] private RectTransform burstAnchor; // điểm spawn (miệng tube)
        [SerializeField] private Sprite circleSprite;       // sprite cho particle (white circle)

        public void Play(SideEffectType type)
        {
            if (burstAnchor == null) return;
            // Anchored coord = anchor's anchoredPosition (relative to its parent — usually the tube root).
            var anchorRt = burstAnchor;
            var spawnParent = anchorRt.parent as RectTransform;
            if (spawnParent == null) spawnParent = anchorRt;
            var pos = anchorRt.anchoredPosition;

            switch (type)
            {
                case SideEffectType.BubblesSmall:
                    UIParticleBurst.Play(new BurstSpec {
                        Pattern = BurstPattern.Drop, Count = 8,
                        StartColor = new Color(0.5f, 0.85f, 1f, 0.9f),
                        EndColor = new Color(0.5f, 0.85f, 1f, 0f),
                        SizeMin = 6f, SizeMax = 12f, Distance = 60f, Duration = 0.6f, Spread = 15f
                    }, spawnParent, pos, circleSprite);
                    break;

                case SideEffectType.BubblesLarge:
                case SideEffectType.GasEvolve:
                    UIParticleBurst.Play(new BurstSpec {
                        Pattern = BurstPattern.Bubble, Count = 14,
                        StartColor = new Color(0.85f, 1f, 0.95f, 0.85f),
                        EndColor = new Color(0.85f, 1f, 0.95f, 0f),
                        SizeMin = 10f, SizeMax = 22f, Distance = 100f, Duration = 1.4f, Spread = 30f
                    }, spawnParent, pos, circleSprite);
                    break;

                case SideEffectType.SmokeWhite:
                case SideEffectType.Steam:
                    UIParticleBurst.Play(new BurstSpec {
                        Pattern = BurstPattern.Rise, Count = 18,
                        StartColor = new Color(0.95f, 0.95f, 1f, 0.75f),
                        EndColor = new Color(0.9f, 0.9f, 0.95f, 0f),
                        SizeMin = 14f, SizeMax = 28f, Distance = 140f, Duration = 1.6f, Spread = 25f
                    }, spawnParent, pos, circleSprite);
                    break;

                case SideEffectType.Flame:
                    UIParticleBurst.Play(new BurstSpec {
                        Pattern = BurstPattern.Rise, Count = 12,
                        StartColor = new Color(1f, 0.6f, 0.2f, 0.95f),
                        EndColor = new Color(1f, 0.3f, 0.1f, 0f),
                        SizeMin = 12f, SizeMax = 24f, Distance = 60f, Duration = 0.9f, Spread = 20f
                    }, spawnParent, pos, circleSprite);
                    break;

                case SideEffectType.Sparkle:
                    UIParticleBurst.Play(new BurstSpec {
                        Pattern = BurstPattern.Burst, Count = 20,
                        StartColor = new Color(1f, 0.95f, 0.5f, 1f),
                        EndColor = new Color(1f, 0.7f, 0.3f, 0f),
                        SizeMin = 8f, SizeMax = 16f, Distance = 90f, Duration = 0.8f, Spread = 5f
                    }, spawnParent, pos, circleSprite);
                    break;

                case SideEffectType.ColorFlash:
                    UIParticleBurst.Play(new BurstSpec {
                        Pattern = BurstPattern.Burst, Count = 12,
                        StartColor = new Color(1f, 0.95f, 0.5f, 0.85f),
                        EndColor = new Color(1f, 0.95f, 0.5f, 0f),
                        SizeMin = 10f, SizeMax = 18f, Distance = 70f, Duration = 0.6f, Spread = 0f
                    }, spawnParent, pos, circleSprite);
                    break;

                case SideEffectType.PrecipitateForm:
                    UIParticleBurst.Play(new BurstSpec {
                        Pattern = BurstPattern.Fall, Count = 16,
                        StartColor = new Color(0.95f, 0.95f, 0.95f, 0.9f),
                        EndColor = new Color(0.9f, 0.9f, 0.95f, 0f),
                        SizeMin = 6f, SizeMax = 12f, Distance = 120f, Duration = 1.3f, Spread = 40f
                    }, spawnParent, pos, circleSprite);
                    break;

                case SideEffectType.Explosion:
                    UIParticleBurst.Play(new BurstSpec {
                        Pattern = BurstPattern.Burst, Count = 30,
                        StartColor = new Color(1f, 0.5f, 0.3f, 1f),
                        EndColor = new Color(1f, 0.3f, 0.1f, 0f),
                        SizeMin = 14f, SizeMax = 26f, Distance = 150f, Duration = 1.1f, Spread = 0f
                    }, spawnParent, pos, circleSprite);
                    break;
            }
        }
    }
}
