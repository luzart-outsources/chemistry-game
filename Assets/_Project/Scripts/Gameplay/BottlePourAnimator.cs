using System;
using System.Collections;
using ChemistryGame.Chemistry;
using ChemistryGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame.Gameplay
{
    /// <summary>
    /// Animation pour chai inspired by WaterSort BottleController:
    ///   1. Move chai từ vị trí gốc đến cạnh tube (RotateOver).
    ///   2. Rotate chai quanh pivot left/right (RotateBottle).
    ///   3. Trong khi rotate: giảm fillAmount của chai, tăng fillAmount tube, vẽ
    ///      stream LineRenderer giữa miệng chai và mặt nước tube.
    ///   4. Rotate ngược về 0 (RotateBack), move chai về vị trí gốc (MoveBack).
    ///
    /// Khác BottleController gốc: chai chemistry chỉ chứa 1 substance + amount float
    /// (không phải 4-color stack). Engine.AddSubstance được gọi MỘT LẦN duy nhất ở
    /// điểm cao trào (giữa rotate) — đảm bảo data flow chính xác, animation chỉ
    /// thuần visual.
    /// </summary>
    public class BottlePourAnimator : MonoBehaviour
    {
        [Header("Curves (paste lại từ WaterSort BottleController hoặc tune)")]
        public AnimationCurve scaleRotationCurve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(60f, 0.5f), new Keyframe(110f, 1f));
        public AnimationCurve fillAmountCurve = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(40f, 0.9f), new Keyframe(70f, 0.6f),
            new Keyframe(95f, 0.3f), new Keyframe(110f, 0f));
        public AnimationCurve rotationSpeedMultiplier = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(50f, 0.6f), new Keyframe(110f, 1f));

        [Header("Pivot points (children)")]
        [SerializeField] private RectTransform leftRotatePoint;
        [SerializeField] private RectTransform rightRotatePoint;

        [Header("Visual")]
        [SerializeField] private LineRenderer streamLine;
        [SerializeField] private LayeredLiquidView liquidView;
        [SerializeField] private float pourMaxAngle = 95f;
        [SerializeField] private float rotateDurationBase = 0.6f;
        [SerializeField] private float moveDuration = 0.35f;

        private Vector2 _originAnchoredPos;
        private Quaternion _originRotation;
        private Transform _chosenPivot;
        private float _directionMultiplier = 1f;
        private bool _busy;
        private UnityEngine.UI.LayoutElement _layoutElem;
        private Transform _originalParent;       // BottlesRoot (parent gốc trước animate)
        private int _originalSiblingIndex;       // chỗ trong layout group (để insert lại đúng vị trí)

        public bool IsBusy => _busy;

        private void Awake()
        {
            if (liquidView == null) liquidView = GetComponent<LayeredLiquidView>();
            var rt = transform as RectTransform;
            if (rt != null) _originAnchoredPos = rt.anchoredPosition;
            _originRotation = transform.localRotation;
            if (streamLine != null) streamLine.enabled = false;
        }

        /// <summary>
        /// Public entry. Animate pouring `amount` từ chai vào target tube.
        /// onMidPour invoked ngay trước khi peak rotation — caller phải thực hiện
        /// engine.AddSubstance + cập nhật bottle internal state ở đây.
        /// </summary>
        public void PourTo(RectTransform targetTubeRect, LayeredLiquidView targetTubeView,
            float amount, Action onMidPour, Action onComplete)
        {
            if (_busy) return;
            if (targetTubeRect == null || targetTubeView == null) return;
            StartCoroutine(PourSequence(targetTubeRect, targetTubeView, amount, onMidPour, onComplete));
        }

        private IEnumerator PourSequence(RectTransform tubeRect, LayeredLiquidView tubeView,
            float amount, Action onMidPour, Action onComplete)
        {
            _busy = true;

            // Snapshot the origin FRESH (layout group may have moved us since Awake).
            var rtSnap = (RectTransform)transform;
            _originAnchoredPos = rtSnap.anchoredPosition;
            _originRotation = transform.localRotation;

            // Detach from VerticalLayoutGroup/HorizontalLayoutGroup so we can move freely.
            _layoutElem = GetComponent<UnityEngine.UI.LayoutElement>();
            if (_layoutElem == null) _layoutElem = gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            _layoutElem.ignoreLayout = true;

            // Reparent ra ngoài Viewport/Mask để không bị clip khi bay sang tube area.
            // Lên cao nhất trong Gameplay prefab (sibling của InventoryPanel + Workspace).
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            // Find ancestor Gameplay (root of current gameplay prefab) — first ancestor without Mask above.
            var newParent = FindSafeAncestor(_originalParent);
            if (newParent != null && newParent != _originalParent)
            {
                rtSnap.SetParent(newParent, worldPositionStays: true);
                rtSnap.SetAsLastSibling(); // render trên top
            }

            // 1. Choose pivot point based on relative X position.
            float myX = transform.position.x;
            float targetX = tubeRect.position.x;
            if (myX > targetX) { _chosenPivot = leftRotatePoint; _directionMultiplier = -1f; }
            else                { _chosenPivot = rightRotatePoint; _directionMultiplier = 1f; }

            // 2. Move chai sao cho pivot trùng cạnh tube.
            var rt = (RectTransform)transform;
            Vector2 startWorld = rt.position;
            // mục tiêu: pivot điểm = cạnh trên của tube (top-mid)
            Vector3 tubeTopWorld = tubeView.GetTopWorldPosition();
            // offset: pivot không phải transform.position. tính offset trong local space.
            Vector3 pivotWorld = _chosenPivot != null ? _chosenPivot.position : transform.position;
            Vector3 offsetWorldFromPivot = transform.position - pivotWorld;
            Vector3 destWorld = tubeTopWorld + offsetWorldFromPivot + new Vector3(0, 0.2f, 0); // 0.2 unit above

            float t = 0f;
            Vector3 fromWorld = rt.position;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                rt.position = Vector3.Lerp(fromWorld, destWorld, t / moveDuration);
                yield return null;
            }
            rt.position = destWorld;

            // 3. Mid-pour callback (data transfer happens here).
            onMidPour?.Invoke();

            // 4. Rotate. Track angle, drive shader fill + SARM.
            float duration = rotateDurationBase;
            float lastAngle = 0f;
            float angle = 0f;
            float elapsed = 0f;
            bool streamSpawned = false;
            float startFill01 = liquidView.CurrentFill01;
            float endFill01 = Mathf.Max(0f, startFill01 - amount / liquidView.MaxCapacity);

            while (elapsed < duration)
            {
                float lerpT = elapsed / duration;
                angle = Mathf.Lerp(0f, _directionMultiplier * pourMaxAngle, lerpT);
                if (_chosenPivot != null)
                    transform.RotateAround(_chosenPivot.position, Vector3.forward, lastAngle - angle);

                float absAngle = Mathf.Abs(angle);
                liquidView.SurfaceTilt = -_directionMultiplier * scaleRotationCurve.Evaluate(absAngle);

                // bottle fill decreases per curve
                float fillNow = Mathf.Lerp(startFill01, endFill01 * fillAmountCurve.Evaluate(absAngle), 1f);
                liquidView.SetFillImmediate(fillNow);

                // Stream visual once tilt past threshold
                if (!streamSpawned && absAngle >= 50f && streamLine != null)
                {
                    streamSpawned = true;
                    streamLine.enabled = true;
                    streamLine.startColor = GetStreamColor();
                    streamLine.endColor = GetStreamColor();
                }
                if (streamSpawned && streamLine != null && _chosenPivot != null)
                {
                    streamLine.SetPosition(0, _chosenPivot.position);
                    streamLine.SetPosition(1, _chosenPivot.position - Vector3.up * 1.4f);
                }

                // Tube fill handled by engine.AddSubstance → OnStateChanged → tubeView.UpdateFromContents.
                // Don't touch tubeView.TweenFill here (would kill pending state tween).

                lastAngle = angle;
                elapsed += Time.deltaTime * rotationSpeedMultiplier.Evaluate(absAngle);
                yield return null;
            }
            // Snap to final angle
            if (_chosenPivot != null)
                transform.RotateAround(_chosenPivot.position, Vector3.forward, lastAngle - _directionMultiplier * pourMaxAngle);
            liquidView.SurfaceTilt = -_directionMultiplier * scaleRotationCurve.Evaluate(pourMaxAngle);
            liquidView.SetFillImmediate(endFill01);

            if (streamLine != null) streamLine.enabled = false;

            // 5. Rotate back.
            elapsed = 0f;
            float endAngle = _directionMultiplier * pourMaxAngle;
            lastAngle = endAngle;
            while (elapsed < duration)
            {
                float lerpT = elapsed / duration;
                angle = Mathf.Lerp(endAngle, 0f, lerpT);
                if (_chosenPivot != null)
                    transform.RotateAround(_chosenPivot.position, Vector3.forward, lastAngle - angle);
                float absAngle = Mathf.Abs(angle);
                liquidView.SurfaceTilt = -_directionMultiplier * scaleRotationCurve.Evaluate(absAngle);
                lastAngle = angle;
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localRotation = _originRotation;
            liquidView.SurfaceTilt = 0f;

            // 6. Move back to origin.
            elapsed = 0f;
            Vector3 backFrom = rt.position;
            Vector3 backDest;
            if (rt.parent is RectTransform parentRt)
            {
                // convert anchoredPos -> world
                rt.anchoredPosition = _originAnchoredPos;
                backDest = rt.position;
                rt.position = backFrom;
            }
            else backDest = backFrom;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                rt.position = Vector3.Lerp(backFrom, backDest, elapsed / moveDuration);
                yield return null;
            }
            rt.position = backDest;
            rt.anchoredPosition = _originAnchoredPos;

            // Restore parent INSIDE viewport mask (so layout group can place us back).
            if (_originalParent != null && transform.parent != _originalParent)
            {
                transform.SetParent(_originalParent, worldPositionStays: false);
                transform.SetSiblingIndex(_originalSiblingIndex);
            }
            // Restore layout flow so VerticalLayoutGroup positions us back into the slot.
            if (_layoutElem != null) _layoutElem.ignoreLayout = false;

            _busy = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Tìm ancestor an toàn (không nằm trong Mask subtree). Stop khi tìm thấy
        /// Canvas hoặc khi đã ra khỏi Mask. Trả về parent đầu tiên KHÔNG bị Mask clip.
        /// </summary>
        private static Transform FindSafeAncestor(Transform startParent)
        {
            if (startParent == null) return null;
            var t = startParent;
            // Climb up until we exit any Mask.
            while (t != null)
            {
                // If t or its ancestors has Mask above, climb. Else this t is safe target.
                bool insideMask = false;
                var probe = t;
                while (probe != null)
                {
                    if (probe.GetComponent<UnityEngine.UI.Mask>() != null ||
                        probe.GetComponent<UnityEngine.UI.RectMask2D>() != null)
                    { insideMask = true; break; }
                    probe = probe.parent;
                }
                if (!insideMask) return t;
                // Else climb parent
                t = t.parent;
                if (t == null) break;
                // Skip into the parent: t became parent of previous t
                // Check again on next iteration
            }
            return startParent; // fallback
        }

        private Color GetStreamColor()
        {
            if (liquidView == null) return Color.white;
            // Approximate: use bottom-most layer color (most prominent).
            var mat = liquidView.GetComponent<Image>().material;
            if (mat == null) return Color.white;
            var c = mat.GetColor("_C1");
            c.a = 1f;
            return c;
        }
    }
}
