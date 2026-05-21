using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace ChemistryGame.Core
{
    /// <summary>Extension giúp await DOTween Tween bằng UniTask + cancellation token.</summary>
    public static class TweenExt
    {
        public static async UniTask AwaitAsync(this Tween tween, CancellationToken ct = default)
        {
            if (tween == null) return;
            while (tween.IsActive() && !tween.IsComplete())
            {
                if (ct.IsCancellationRequested)
                {
                    tween.Kill();
                    return;
                }
                await UniTask.Yield();
            }
        }
    }
}
