using Cysharp.Threading.Tasks;
using Luzart;
using UnityEngine;

namespace ChemistryGame.Core
{
    /// <summary>Entry point cho game: load save → init audio → show MainMenu.</summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private bool autoShowMainMenu = true;

        private async void Start()
        {
            SaveSystem.Load();

            // Đợi 1 frame để UIManager init xong (Awake/Start order safety)
            await UniTask.Yield();

            if (AudioManager.Instance != null)
                AudioManager.Instance.ApplyVolumes();

            // DEBUG: vào thẳng 1 màn để test (set debugPlayLevelIndex ở GameManager Inspector).
            var gm = GameManager.Instance;
            if (gm != null && gm.DebugPlayLevelIndex > 0 && UIManager.Instance != null)
            {
                gm.SetCurrentLevel(gm.DebugPlayLevelIndex);
                if (gm.CurrentLevel == null)
                    Debug.LogWarning($"[GameBootstrap] DEBUG: màn {gm.DebugPlayLevelIndex} chưa có trong " +
                        "GameManager.levels — chạy ChemistryGame/Seed/All (hoặc kéo asset vào list) trước.");
                await UIManager.Instance.ShowAsync(UIId.CG_Gameplay);
                return;
            }

            if (autoShowMainMenu && UIManager.Instance != null)
            {
                await UIManager.Instance.ShowAsync(UIId.CG_MainMenu);
            }
        }
    }
}
