using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame.Chemistry
{
    /// <summary>
    /// Luật tinh khiết cho 1 màn. Sản phẩm chính phải tồn tại;
    /// tạp chất bị cấm; lượng dư cho phép trong tolerance.
    /// </summary>
    [Serializable]
    public class PurityRule
    {
        [Tooltip("Sản phẩm chính buộc phải có trong workspace cuối cùng.")]
        public SubstanceData TargetProduct;

        [Min(0f)]
        [Tooltip("Lượng tối thiểu của target product để tính là đạt.")]
        public float MinTargetAmount = 1f;

        [Tooltip("Các chất KHÔNG được phép tồn tại (tạp chất / kết tủa sai / dư reactant).")]
        public List<SubstanceData> ForbiddenSubstances = new List<SubstanceData>();

        [Min(0f)]
        [Tooltip("Lượng tolerance — chất cấm trên ngưỡng này coi như không tinh khiết.")]
        public float ForbiddenTolerance = 0.5f;

        [Tooltip("Khi true: bất kỳ kết tủa nào không nằm trong target sẽ bị tính tạp.")]
        public bool RejectAnyPrecipitate = false;
    }
}
