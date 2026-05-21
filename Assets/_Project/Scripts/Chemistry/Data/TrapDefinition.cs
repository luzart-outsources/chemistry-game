using System;
using UnityEngine;

namespace ChemistryGame.Chemistry
{
    [Serializable]
    public class TrapDefinition
    {
        public string TrapId;
        public SubstanceData TriggerProduct;
        [TextArea] public string ExplanationVi;
    }
}
