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

            if (autoShowMainMenu && UIManager.Instance != null)
            {
                await UIManager.Instance.ShowAsync(UIId.CG_MainMenu);
            }
        }
    }
}
