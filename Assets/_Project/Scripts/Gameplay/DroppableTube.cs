using ChemistryGame.Chemistry;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChemistryGame.Gameplay
{
    public class DroppableTube : MonoBehaviour, IDropHandler
    {
        [SerializeField] private LiquidView liquidView;
        public LiquidView LiquidView => liquidView;

        public void ReceiveSubstance(SubstanceData s, float amount)
        {
            if (s == null || amount <= 0f) return;
            GameplayEvents.RaiseSubstanceDropped(s, amount);
        }

        public void OnDrop(PointerEventData eventData)
        {
            // Drop handling thực hiện bên DraggableBottle.OnEndDrag (raycast vẫn hợp lệ).
        }
    }
}
