using System;

namespace ChemistryGame.Chemistry
{
    /// <summary>Lượng chất (mL/g). Đơn vị tuỳ phase nhưng được normalize về scalar để engine xử lý đồng nhất.</summary>
    [Serializable]
    public struct Amount
    {
        public float Value;

        public Amount(float v) { Value = v; }

        public static Amount Zero => new Amount(0f);
        public bool IsZero => Value <= 0.001f;
        public bool IsPositive => Value > 0.001f;

        public static Amount operator +(Amount a, Amount b) => new Amount(a.Value + b.Value);
        public static Amount operator -(Amount a, Amount b) => new Amount(System.Math.Max(0f, a.Value - b.Value));
        public static Amount operator *(Amount a, float k) => new Amount(a.Value * k);

        public static bool operator >(Amount a, Amount b) => a.Value > b.Value;
        public static bool operator <(Amount a, Amount b) => a.Value < b.Value;
        public static bool operator >=(Amount a, Amount b) => a.Value >= b.Value;
        public static bool operator <=(Amount a, Amount b) => a.Value <= b.Value;

        public override string ToString() => $"{Value:F1}";
    }
}
