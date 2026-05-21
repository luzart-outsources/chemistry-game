using System.Collections.Generic;
using ChemistryGame.Chemistry;
using ChemistryGame.Core;
using Cysharp.Threading.Tasks;
using Luzart;
using TMPro;
using UnityEngine;

namespace ChemistryGame.Gameplay
{
    public enum GameplayPhase { None, Setup, Playing, Submitting, ResultWin, ResultFail, Paused }

    /// <summary>
    /// FSM của 1 màn. Setup tủ thuốc/dụng cụ, route input events vào ChemistryEngine,
    /// chấm điểm khi user Submit.
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [Header("Refs trong prefab Gameplay")]
        [SerializeField] private LiquidView mainTube;          // legacy fallback (unused)
        [SerializeField] private LayeredLiquidView layeredTube; // preferred (WaterSort-style shader)
        [SerializeField] private DroppableTube tube;
        [SerializeField] private RectTransform bottlesRoot;
        [SerializeField] private RectTransform toolsRoot;
        [SerializeField] private StatePanel statePanel;
        [SerializeField] private FXPlayer fxPlayer;           // legacy world-space FX (covered by overlay)
        [SerializeField] private ChemistryFXController uiFx;  // UI-friendly particle burst (visible on overlay)
        [SerializeField] private PourAmountPopupUI pourAmountPopup; // click bottle → popup chọn lượng

        [Header("Prefabs")]
        [SerializeField] private DraggableBottle bottlePrefab;
        [SerializeField] private ToolButton toolButtonPrefab;

        [Header("UI elements")]
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text reactionFeedText;

        public LevelConfig CurrentLevel { get; private set; }
        public GameplayPhase Phase { get; private set; }
        public int HintsUsed { get; private set; }
        public List<TrapDefinition> TrapsTriggered { get; } = new List<TrapDefinition>();

        private ChemistryEngine _engine;
        private readonly List<DraggableBottle> _bottles = new List<DraggableBottle>();
        private readonly List<ToolButton> _toolButtons = new List<ToolButton>();
        private Coroutine _burnerRoutine;

        [Header("Burner config")]
        [SerializeField] private float burnerEvaporationRate = 12f;     // Solvent units/sec
        [SerializeField] private float burnerCrystallizeRate = 6f;      // Aqueous→Crystal units/sec (chậm hơn)
        [SerializeField] private float burnerSteamInterval = 0.35f;     // particle spawn cadence

        public ChemistryEngine Engine => _engine;
        public bool IsBurnerActive => _burnerRoutine != null;

        private void OnEnable()
        {
            GameplayEvents.OnSubstanceDropped += HandleDrop;
            GameplayEvents.OnToolUsed += HandleTool;
            GameplayEvents.OnSubmitRequested += HandleSubmit;
            GameplayEvents.OnUndoRequested += HandleUndo;
            GameplayEvents.OnRestartRequested += HandleRestart;
            GameplayEvents.OnHintRequested += HandleHint;
        }

        private void OnDisable()
        {
            StopBurner();
            GameplayEvents.OnSubstanceDropped -= HandleDrop;
            GameplayEvents.OnToolUsed -= HandleTool;
            GameplayEvents.OnSubmitRequested -= HandleSubmit;
            GameplayEvents.OnUndoRequested -= HandleUndo;
            GameplayEvents.OnRestartRequested -= HandleRestart;
            GameplayEvents.OnHintRequested -= HandleHint;
        }

        public void LoadLevel(LevelConfig cfg)
        {
            CurrentLevel = cfg;
            Phase = GameplayPhase.Setup;
            HintsUsed = 0;
            TrapsTriggered.Clear();

            // Stop active burner coroutine before swap engine (avoid dangling ref to old engine).
            if (_burnerRoutine != null) { StopCoroutine(_burnerRoutine); _burnerRoutine = null; }

            _engine = new ChemistryEngine(cfg.AvailableReactions);
            _engine.ReactionOccurred += OnReactionOccurred;
            _engine.StateChanged += OnStateChanged;

            if (objectiveText != null) objectiveText.text = cfg.ObjectiveText;

            // Spawn bottles
            ClearChildren(bottlesRoot);
            _bottles.Clear();
            foreach (var b in cfg.Bottles)
            {
                if (b == null || b.Substance == null || bottlePrefab == null) continue;
                var go = Instantiate(bottlePrefab, bottlesRoot);
                go.Bind(b.Substance, b.InitialAmount, b.MaskLabel, b.MaskedLabel);
                // Hook click → mở popup chọn lượng. Capture closure: chai cụ thể.
                go.OnClickRequest = OpenPourPopupFor;
                _bottles.Add(go);
            }

            // Spawn tools
            ClearChildren(toolsRoot);
            _toolButtons.Clear();
            foreach (var t in cfg.Tools)
            {
                if (t == null || t.Tool == null || toolButtonPrefab == null) continue;
                var go = Instantiate(toolButtonPrefab, toolsRoot);
                go.Bind(t.Tool);
                _toolButtons.Add(go);
            }

            if (mainTube != null) mainTube.Reset();
            if (layeredTube != null) layeredTube.Reset();

            // Auto enter Playing
            Phase = GameplayPhase.Playing;
            if (statePanel != null) statePanel.Render(_engine.State);
            if (reactionFeedText != null) reactionFeedText.text = string.Empty;
        }

        private void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
        }

        // ===== Event handlers =====

        private void HandleDrop(SubstanceData s, float amount)
        {
            if (Phase != GameplayPhase.Playing) return;
            AudioManager.Instance?.PlaySfx("sfx_pour");

            // Find source bottle + animator.
            DraggableBottle source = null;
            foreach (var b in _bottles) if (b != null && b.Substance == s) { source = b; break; }

            if (source != null && layeredTube != null)
            {
                var animator = source.GetComponent<BottlePourAnimator>();
                var tubeRect = layeredTube.transform as RectTransform;
                if (animator != null && tubeRect != null && !animator.IsBusy)
                {
                    animator.PourTo(tubeRect, layeredTube, amount,
                        onMidPour: () => {
                            PlayFx(SideEffectType.BubblesSmall);
                            _engine.AddSubstance(s, amount);
                        },
                        onComplete: null);
                    return;
                }
            }

            // Fallback: no animator → instant
            PlayFx(SideEffectType.BubblesSmall);
            _engine.AddSubstance(s, amount);
        }

        private void PlayFx(SideEffectType type)
        {
            if (uiFx != null) uiFx.Play(type);
            else if (fxPlayer != null) fxPlayer.Play(type);
        }

        /// <summary>Click 1 chai → mở popup slider chọn lượng → confirm trigger pour event.</summary>
        private void OpenPourPopupFor(DraggableBottle bottle)
        {
            if (Phase != GameplayPhase.Playing) return;
            if (bottle == null || bottle.Substance == null) return;
            if (bottle.CurrentAmount <= 0f) return;
            if (pourAmountPopup == null)
            {
                // Fallback nếu popup không wired: dùng PourPerDrop default
                GameplayEvents.RaiseSubstanceDropped(bottle.Substance, bottle.PourPerDrop);
                bottle.Consume(bottle.PourPerDrop);
                return;
            }
            pourAmountPopup.Open(bottle, amount =>
            {
                if (amount <= 0f) return;
                GameplayEvents.RaiseSubstanceDropped(bottle.Substance, amount);
                bottle.Consume(amount);
            });
        }

        // ===== Burner coroutine =====

        private System.Collections.IEnumerator BurnerEvaporateRoutine()
        {
            float steamTimer = 0f;
            PlayFx(SideEffectType.Flame);
            PlayFx(SideEffectType.Steam);

            // Phase 1: Evaporate Solvent (water) until none left.
            while (_engine.HasEvaporableSolvent() && Phase == GameplayPhase.Playing)
            {
                _engine.EvaporateStep(burnerEvaporationRate * Time.deltaTime);
                steamTimer += Time.deltaTime;
                if (steamTimer >= burnerSteamInterval)
                {
                    steamTimer = 0f;
                    PlayFx(SideEffectType.Steam);
                }
                yield return null;
            }

            // Phase 2: Crystallize Aqueous → CrystalForm (slower rate).
            if (_engine.HasAqueousToCrystallize() && Phase == GameplayPhase.Playing)
            {
                PlayFx(SideEffectType.Sparkle); // visual transition cue
            }
            while (_engine.HasAqueousToCrystallize() && Phase == GameplayPhase.Playing)
            {
                _engine.CrystallizeStep(burnerCrystallizeRate * Time.deltaTime);
                steamTimer += Time.deltaTime;
                if (steamTimer >= burnerSteamInterval * 1.5f)
                {
                    steamTimer = 0f;
                    PlayFx(SideEffectType.Sparkle);
                }
                yield return null;
            }

            // Done: final sparkle marking complete crystallization.
            PlayFx(SideEffectType.Sparkle);
            _engine.ApplyHeat(false);
            _burnerRoutine = null;
        }

        public void StopBurner()
        {
            if (_burnerRoutine != null)
            {
                StopCoroutine(_burnerRoutine);
                _burnerRoutine = null;
                _engine?.ApplyHeat(false);
            }
        }

        private void HandleTool(ToolFunctionType type, SubstanceData ctx)
        {
            if (Phase != GameplayPhase.Playing) return;
            switch (type)
            {
                case ToolFunctionType.IndicatorPaper:
                    // Đọc pH hiện tại và toast
                    var ph = _engine.State.CalculatePH();
                    var msg = ph < 6.5f ? $"Quỳ → ĐỎ (pH {ph:F1})" : ph > 7.5f ? $"Quỳ → XANH (pH {ph:F1})" : $"Quỳ → TÍM (pH {ph:F1})";
                    if (reactionFeedText != null) reactionFeedText.text = msg;
                    UIManager.Instance?.ShowToastAsync(msg, ToastStyle.Info, 2.5f);
                    AudioManager.Instance?.PlaySfx("sfx_indicator");
                    break;

                case ToolFunctionType.Burner:
                    // Toggle continuous burner: click 1 = bắt đầu khò, click 2 = dừng.
                    if (_burnerRoutine != null)
                    {
                        StopBurner();
                    }
                    else
                    {
                        _engine.ApplyHeat(true);
                        _burnerRoutine = StartCoroutine(BurnerEvaporateRoutine());
                        AudioManager.Instance?.PlaySfx("sfx_burner");
                    }
                    break;

                case ToolFunctionType.FilterPaper:
                    _engine.Filter();
                    PlayFx(SideEffectType.PrecipitateForm);
                    AudioManager.Instance?.PlaySfx("sfx_filter");
                    UIManager.Instance?.ShowToastAsync("Đã lọc kết tủa", ToastStyle.Info, 2f);
                    break;

                case ToolFunctionType.DistilledWater:
                    // Add water - find substance with Id "H2O"
                    // Handled by AssociatedSubstance binding
                    if (ctx != null) _engine.AddSubstance(ctx, 30f);
                    break;

                case ToolFunctionType.GasCollector:
                    _engine.CollectGasInto();
                    PlayFx(SideEffectType.GasEvolve);
                    PlayFx(SideEffectType.BubblesLarge);
                    UIManager.Instance?.ShowToastAsync("Đang thu khí...", ToastStyle.Info, 1.5f);
                    break;

                case ToolFunctionType.ReagentDropper:
                    if (ctx != null) _engine.AddSubstance(ctx, 1f); // dropper: trace amount
                    AudioManager.Instance?.PlaySfx("sfx_drop");
                    break;

                case ToolFunctionType.GasBubbler:
                    if (ctx != null) _engine.BubbleGas(ctx, 20f);
                    PlayFx(SideEffectType.BubblesLarge);
                    AudioManager.Instance?.PlaySfx("sfx_bubble");
                    break;
            }
        }

        private async void HandleSubmit()
        {
            if (Phase != GameplayPhase.Playing) return;
            Phase = GameplayPhase.Submitting;

            var checker = new PurityChecker();
            var result = checker.Check(_engine.State, CurrentLevel.PurityRule);

            // Determine traps triggered
            TrapsTriggered.Clear();
            foreach (var trap in CurrentLevel.Traps)
            {
                if (trap?.TriggerProduct == null) continue;
                if (_engine.State.Contains(trap.TriggerProduct))
                    TrapsTriggered.Add(trap);
            }

            int stars = CalculateStars(result);

            if (stars >= 1 && result.TargetProductCreated)
            {
                Phase = GameplayPhase.ResultWin;
                SaveSystem.ReportLevelResult(CurrentLevel.LevelIndex, stars, HintsUsed > 0);
                PlayFx(SideEffectType.Sparkle);
                AudioManager.Instance?.PlaySfx("sfx_win");
                await UI.ResultPopupUI.ShowAsync(CurrentLevel, stars, result.Reason);
            }
            else
            {
                Phase = GameplayPhase.ResultFail;
                AudioManager.Instance?.PlaySfx("sfx_fail");
                var impurity = result.Impurities.Count > 0 ? result.Impurities[0] : null;
                var producedWrong = FindWrongProduct();
                await UI.WrongProductPopupUI.ShowAsync(CurrentLevel, result.Reason,
                    producedWrong, impurity);
            }
        }

        private SubstanceData FindWrongProduct()
        {
            // Tìm trap đã trigger có TriggerProduct
            foreach (var tr in TrapsTriggered)
                if (tr.TriggerProduct != null) return tr.TriggerProduct;
            return null;
        }

        private int CalculateStars(PurityResult result)
        {
            if (!result.TargetProductCreated) return 0;
            int stars = 1;
            if (result.IsPure) stars = 2;

            bool noBlockingTrap = true;
            foreach (var triggered in TrapsTriggered)
            {
                foreach (var blocking in CurrentLevel.ThreeStarBlockingTraps)
                    if (blocking != null && blocking.TrapId == triggered.TrapId) { noBlockingTrap = false; break; }
                if (!noBlockingTrap) break;
            }
            if (stars == 2 && noBlockingTrap && HintsUsed == 0) stars = 3;
            // Hint cap
            if (HintsUsed >= 3) stars = Mathf.Min(stars, 1);
            else if (HintsUsed >= 1) stars = Mathf.Min(stars, 2);
            return stars;
        }

        private void HandleUndo()
        {
            // Đơn giản: reset workspace (engine.Reset). Có thể implement snapshot stack sau.
            RestartLevel();
        }

        private void HandleRestart()
        {
            RestartLevel();
        }

        public void RestartLevel()
        {
            LoadLevel(CurrentLevel);
            AudioManager.Instance?.PlaySfx("sfx_back");
        }

        private async void HandleHint(int tier)
        {
            HintsUsed = Mathf.Max(HintsUsed, tier);
            if (CurrentLevel.Hints == null) return;
            var msg = CurrentLevel.Hints.GetHint(tier);
            await UIManager.Instance.ShowToastAsync(msg, ToastStyle.Info, 4f);
            AudioManager.Instance?.PlaySfx("sfx_hint");
        }

        // ===== Engine callbacks =====
        private void OnReactionOccurred(ReactionEvent evt)
        {
            if (evt.Rule == null) return;
            PlayFx(evt.Rule.PrimarySideEffect);
            if (reactionFeedText != null)
                reactionFeedText.text = "→ " + (string.IsNullOrEmpty(evt.Rule.ReactionEquation) ? evt.Rule.Description : evt.Rule.ReactionEquation);
        }

        private void OnStateChanged(StateChangeEvent evt)
        {
            if (mainTube != null) mainTube.UpdateFromContents(evt.State.Contents);
            if (layeredTube != null) layeredTube.UpdateFromContents(evt.State.Contents);
            if (statePanel != null) statePanel.Render(evt.State);
        }
    }
}
