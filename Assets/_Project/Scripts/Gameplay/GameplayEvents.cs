using System;
using ChemistryGame.Chemistry;

namespace ChemistryGame.Gameplay
{
    /// <summary>Static event bus — decouple Drag/Drop ↔ Engine ↔ View ↔ HUD.</summary>
    public static class GameplayEvents
    {
        public static event Action<SubstanceData, float> OnSubstanceDropped; // drop chai vào tube
        public static event Action<ToolFunctionType, SubstanceData> OnToolUsed;
        public static event Action OnSubmitRequested;
        public static event Action OnUndoRequested;
        public static event Action OnRestartRequested;
        public static event Action<int> OnHintRequested; // tier 1/2/3

        public static void RaiseSubstanceDropped(SubstanceData s, float amt) => OnSubstanceDropped?.Invoke(s, amt);
        public static void RaiseToolUsed(ToolFunctionType t, SubstanceData ctx) => OnToolUsed?.Invoke(t, ctx);
        public static void RaiseSubmit() => OnSubmitRequested?.Invoke();
        public static void RaiseUndo() => OnUndoRequested?.Invoke();
        public static void RaiseRestart() => OnRestartRequested?.Invoke();
        public static void RaiseHint(int tier) => OnHintRequested?.Invoke(tier);

        public static void ClearAll()
        {
            OnSubstanceDropped = null;
            OnToolUsed = null;
            OnSubmitRequested = null;
            OnUndoRequested = null;
            OnRestartRequested = null;
            OnHintRequested = null;
        }
    }
}
