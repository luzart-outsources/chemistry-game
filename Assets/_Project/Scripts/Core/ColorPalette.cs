using UnityEngine;

namespace ChemistryGame.Core
{
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "ChemistryGame/Color Palette", order = 99)]
    public class ColorPalette : ScriptableObject
    {
        [Header("Liquid theme")]
        public Color NeutralBlue = new Color(0.42f, 0.78f, 0.94f, 0.85f);
        public Color AcidRed     = new Color(0.95f, 0.4f,  0.4f,  0.85f);
        public Color BaseBlue    = new Color(0.45f, 0.55f, 0.9f,  0.85f);
        public Color PrecipWhite = new Color(0.95f, 0.95f, 0.95f, 0.95f);

        [Header("Specific")]
        public Color BloodRedKSCN = new Color(0.78f, 0.1f, 0.12f, 0.95f); // Fe(SCN)3
        public Color CuSO4Blue    = new Color(0.18f, 0.6f, 0.92f, 0.9f);
        public Color FeCl3Yellow  = new Color(0.92f, 0.78f, 0.2f, 0.9f);
        public Color FeOH2Green   = new Color(0.5f, 0.78f, 0.45f, 0.9f);
        public Color FeOH3Brown   = new Color(0.62f, 0.34f, 0.18f, 0.9f);
        public Color CuOH2Blue    = new Color(0.32f, 0.6f, 0.92f, 0.9f);
        public Color AgClWhite    = new Color(0.96f, 0.96f, 0.96f, 0.95f);
        public Color AgOHBlack    = new Color(0.2f, 0.18f, 0.16f, 0.92f);
        public Color Ag2CO3Yellow = new Color(0.93f, 0.85f, 0.42f, 0.92f);

        [Header("UI Accents")]
        public Color SuccessGreen = new Color(0.32f, 0.84f, 0.42f);
        public Color FailRed      = new Color(0.92f, 0.32f, 0.32f);
        public Color StarGold     = new Color(1f, 0.83f, 0.18f);
        public Color HintPurple   = new Color(0.74f, 0.5f, 0.95f);
    }
}
