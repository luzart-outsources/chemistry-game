using System.Collections.Generic;
using System.Linq;
using ChemistryGame.Chemistry;
using NUnit.Framework;
using UnityEngine;

namespace ChemistryGame.EditorTools.ContentEditor.Tests
{
    public class ValidatorTests
    {
        private static SubstanceData NewSub(string id)
        {
            var s = ScriptableObject.CreateInstance<SubstanceData>();
            s.Id = id;
            return s;
        }

        [Test]
        public void Substance_EmptyId_RaisesError()
        {
            var s = NewSub("");
            var issues = Validator.ValidateSubstance(s, new FakeIndex());
            Assert.IsTrue(issues.Any(i => i.Code == "SUB_ID_EMPTY"));
        }

        [Test]
        public void Substance_DuplicateId_RaisesError()
        {
            var a = NewSub("Dup");
            var b = NewSub("Dup");
            var idx = new FakeIndex();
            idx.Substances.Add(a);
            idx.Substances.Add(b);
            var issues = Validator.ValidateSubstance(a, idx);
            Assert.IsTrue(issues.Any(i => i.Code == "SUB_ID_DUPLICATE"));
        }

        [Test]
        public void Substance_Orphan_RaisesWarning()
        {
            var s = NewSub("Lonely");
            var idx = new FakeIndex();
            idx.Substances.Add(s);
            var issues = Validator.ValidateSubstance(s, idx);
            Assert.IsTrue(issues.Any(i => i.Code == "SUB_ORPHAN" && i.Severity == Severity.Warning));
        }

        [Test]
        public void Reaction_NullInput_RaisesError()
        {
            var r = ScriptableObject.CreateInstance<ReactionRule>();
            r.Inputs.Add(new ReactionStoich { Substance = null, Ratio = 1 });
            r.Outputs.Add(new ReactionStoich { Substance = NewSub("X"), Ratio = 1 });
            var issues = Validator.ValidateReaction(r, new FakeIndex());
            Assert.IsTrue(issues.Any(i => i.Code == "RX_INPUT_NULL"));
        }

        [Test]
        public void Reaction_EmptyInputs_RaisesError()
        {
            var r = ScriptableObject.CreateInstance<ReactionRule>();
            r.Outputs.Add(new ReactionStoich { Substance = NewSub("X"), Ratio = 1 });
            var issues = Validator.ValidateReaction(r, new FakeIndex());
            Assert.IsTrue(issues.Any(i => i.Code == "RX_INPUTS_EMPTY"));
        }

        [Test]
        public void Level_TargetNull_RaisesError()
        {
            var l = ScriptableObject.CreateInstance<LevelConfig>();
            l.LevelIndex = 99;
            var issues = Validator.ValidateLevel(l, new FakeIndex { Levels = { l } });
            Assert.IsTrue(issues.Any(i => i.Code == "LV_TARGET_NULL"));
        }

        [Test]
        public void Level_TargetInForbidden_RaisesError()
        {
            var target = NewSub("Target");
            var l = ScriptableObject.CreateInstance<LevelConfig>();
            l.LevelIndex = 99;
            l.PurityRule = new PurityRule { TargetProduct = target, ForbiddenSubstances = new List<SubstanceData> { target } };
            var issues = Validator.ValidateLevel(l, new FakeIndex { Levels = { l } });
            Assert.IsTrue(issues.Any(i => i.Code == "LV_TARGET_IN_FORBIDDEN"));
        }

        [Test]
        public void Level_DuplicateIndex_RaisesError()
        {
            var a = ScriptableObject.CreateInstance<LevelConfig>(); a.LevelIndex = 1;
            var b = ScriptableObject.CreateInstance<LevelConfig>(); b.LevelIndex = 1;
            a.PurityRule = new PurityRule { TargetProduct = NewSub("X") };
            b.PurityRule = new PurityRule { TargetProduct = NewSub("Y") };
            var idx = new FakeIndex();
            idx.Levels.Add(a); idx.Levels.Add(b);
            var issues = Validator.ValidateLevel(a, idx);
            Assert.IsTrue(issues.Any(i => i.Code == "LV_INDEX_DUPLICATE"));
        }
    }

    internal class FakeIndex : IAssetIndexReadOnly
    {
        public List<SubstanceData> Substances { get; } = new List<SubstanceData>();
        public List<ReactionRule>  Reactions  { get; } = new List<ReactionRule>();
        public List<LevelConfig>   Levels     { get; } = new List<LevelConfig>();

        public IReadOnlyList<SubstanceData> GetAllSubstances() => Substances;
        public IReadOnlyList<ReactionRule>  GetAllReactions()  => Reactions;
        public IReadOnlyList<LevelConfig>   GetAllLevels()     => Levels;
        public SubstanceData GetSubstanceById(string id) => Substances.Find(s => s.Id == id);
        public IReadOnlyList<ReactionRule>  GetReactionsUsingInput(SubstanceData s)  => System.Array.Empty<ReactionRule>();
        public IReadOnlyList<ReactionRule>  GetReactionsProducing(SubstanceData s)   => System.Array.Empty<ReactionRule>();
        public IReadOnlyList<LevelConfig>   GetLevelsUsingSubstance(SubstanceData s) => System.Array.Empty<LevelConfig>();
        public IReadOnlyList<LevelConfig>   GetLevelsUsingReaction(ReactionRule r)   => System.Array.Empty<LevelConfig>();
    }
}
