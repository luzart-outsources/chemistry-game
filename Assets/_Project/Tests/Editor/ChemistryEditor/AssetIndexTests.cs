using ChemistryGame.Chemistry;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ChemistryGame.EditorTools.ContentEditor.Tests
{
    public class AssetIndexTests
    {
        private string _tempFolder;

        [SetUp]
        public void Setup()
        {
            var guid = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            _tempFolder = $"Assets/TempChemTest_{guid}";
            AssetDatabase.CreateFolder("Assets", $"TempChemTest_{guid}");
        }

        [TearDown]
        public void Teardown()
        {
            AssetDatabase.DeleteAsset(_tempFolder);
        }

        private SubstanceData CreateSubstance(string id)
        {
            var s = ScriptableObject.CreateInstance<SubstanceData>();
            s.Id = id;
            AssetDatabase.CreateAsset(s, $"{_tempFolder}/Sub_{id}.asset");
            return s;
        }

        [Test]
        public void GetAllSubstances_ReturnsCreatedSubstance()
        {
            var s = CreateSubstance("TestSubX");
            AssetDatabase.SaveAssets();

            var idx = new AssetIndex();
            idx.Rebuild();

            CollectionAssert.Contains(idx.GetAllSubstances(), s);
        }

        [Test]
        public void GetSubstanceById_ReturnsMatch()
        {
            var s = CreateSubstance("TestSubY");
            AssetDatabase.SaveAssets();

            var idx = new AssetIndex();
            idx.Rebuild();

            Assert.AreSame(s, idx.GetSubstanceById("TestSubY"));
        }

        [Test]
        public void GetReactionsUsingInput_ReturnsReactionsThatInputIt()
        {
            var fe    = CreateSubstance("TestFe");
            var hcl   = CreateSubstance("TestHCl");
            var fecl2 = CreateSubstance("TestFeCl2");

            var rule = ScriptableObject.CreateInstance<ReactionRule>();
            rule.Id = "TestRx";
            rule.Inputs.Add(new ReactionStoich { Substance = fe,  Ratio = 1 });
            rule.Inputs.Add(new ReactionStoich { Substance = hcl, Ratio = 2 });
            rule.Outputs.Add(new ReactionStoich { Substance = fecl2, Ratio = 1 });
            AssetDatabase.CreateAsset(rule, $"{_tempFolder}/Rx_Test.asset");
            AssetDatabase.SaveAssets();

            var idx = new AssetIndex();
            idx.Rebuild();

            CollectionAssert.Contains(idx.GetReactionsUsingInput(fe), rule);
            CollectionAssert.Contains(idx.GetReactionsUsingInput(hcl), rule);
            CollectionAssert.Contains(idx.GetReactionsProducing(fecl2), rule);
        }

        [Test]
        public void Invalidate_FiresOnIndexChanged()
        {
            var idx = new AssetIndex();
            bool fired = false;
            idx.OnIndexChanged += () => fired = true;
            idx.Invalidate();
            Assert.IsTrue(fired);
        }
    }
}
