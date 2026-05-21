using ChemistryGame.Chemistry;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChemistryGame.Gameplay
{
    /// <summary>
    /// Base draggable UI item (chai chất / dụng cụ).
    /// Drag = clone temporary "ghost" gắn vào cursor. Drop → check DroppableTube hit.
    /// </summary>
    public abstract class DraggableItem : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform rect;
        [SerializeField] protected float dragAlpha = 0.7f;

        protected Canvas rootCanvas;
        protected Vector2 originalPos;
        protected Transform originalParent;

        protected virtual void Awake()
        {
            if (rect == null) rect = transform as RectTransform;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            originalPos = rect.anchoredPosition;
            originalParent = rect.parent;
            // Bring to top
            if (rootCanvas != null)
                rect.SetParent(rootCanvas.transform, true);
            rect.SetAsLastSibling();
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
            transform.localScale = Vector3.one * 1.05f;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (rootCanvas == null) return;
            rect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            transform.localScale = Vector3.one;

            // Reset position. DroppableTube xử lý drop trong OnDrop.
            rect.SetParent(originalParent, true);
            rect.anchoredPosition = originalPos;
        }
    }
}
