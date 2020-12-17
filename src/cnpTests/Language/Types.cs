using System;
using System.Collections.Generic;
using System.Linq;
using CNP.Language;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Language
{
    /// <summary>
    /// In valence lookups ProgramTypes are used as a key so it's important they have consistent hashcodes.
    /// </summary>
    [TestClass]
    public class Types
    {
        [TestMethod]
        public void IdenticalTypes_SameHashcode()
        {
            ProgramType s1 = new ProgramType(new Valence(("a", ArgumentMode.In), ("b", ArgumentMode.Out)));
            ProgramType s2 = Parser.ParseProgramType("{b:out, a:in}");
            Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be the same.");
        }

        // [TestMethod]
        // public void TypeWithDifferentArgNames_DifferentHashcodes()
        // {
        //     ProgramType s1 = new ProgramType(new NameModeMap(("a2", ArgumentMode.In), ("b", ArgumentMode.Out)));
        //     ProgramType s2 = Parser.ParseProgramType("{b:out, a:in}");
        //     Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be different");
        // }

        [TestMethod]
        public void TypeWithDifferentArgModes_DifferentHashcodes()
        {
            ProgramType s1 = new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.Out)));
            ProgramType s2 = Parser.ParseProgramType("{b:out, a:in}");
            int h1 = s1.GetHashCode();
            int h2 = s2.GetHashCode();
            Assert.AreNotEqual(h1, h2, "Hashcodes should be different");
        }

        public void Hashcode_CombineHashes_Associativity()
        {

        }

        [TestMethod]
        public void TypeStore_GroundLookup_ReturnsGroundTypes()
        {
            TypeStore<ProgramType> ts = new TypeStore<ProgramType>(new[]
            {
                new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.In))),
                new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.Out)))
            });
            var types = ts.FindCompatibleTypes(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.Out)));
            Assert.IsTrue(types.Any(), "A result was returned.");
            bool inTheResults =
                types.Contains(new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.Out))));
            Assert.IsTrue(inTheResults, "A newly ground program type should be returned.");
            bool allAreGround = types.All(t => t.IsGround());
            Assert.IsTrue(allAreGround, "All types returned should be ground.");
        }

        [TestMethod]
        public void TypeStore_PartiallyGroundLookup_OnlyReturnsTypesWithPartiallyMatchingSignatures()
        {
            TypeStore<ProgramType> ts = new TypeStore<ProgramType>(new[]
            {
                new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.In))),
                new ProgramType(new Valence(("u", ArgumentMode.Out), ("v", ArgumentMode.In)))
            });
            var types = ts.FindCompatibleTypes(new Valence((ArgumentNameVar.NewUnbound(), ArgumentMode.Out), (new ArgumentNameVar("b"), ArgumentMode.In)));
            Assert.IsTrue(types.Any(), "A result was returned.");
            Assert.AreEqual(1, types.Count(), "Only one type is found.");
            Assert.AreEqual(ArgumentMode.Out, types.First().Domains["a"]);
            Assert.AreEqual(ArgumentMode.In, types.First().Domains["b"]);
        }

        [TestMethod]
        public void TypeStore_ArgumentModeLookup_OnlyReturnsTypesWithCorrectNumberOfInsAndOuts()
        {
            TypeStore<ProgramType> ts = new TypeStore<ProgramType>(new[]
            {
                new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.In))),
                new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.In), ("c", ArgumentMode.In)))
            });
            var types = ts.FindCompatibleTypes(new Valence((ArgumentNameVar.NewUnbound(), ArgumentMode.Out), (ArgumentNameVar.NewUnbound(), ArgumentMode.In)));
            Assert.IsTrue(types.Any(), "A result was returned.");
            Assert.AreEqual(1, types.Count(), "Only one type is found.");
            Assert.AreEqual(ArgumentMode.Out, types.First().Domains["a"]);
            Assert.AreEqual(ArgumentMode.In, types.First().Domains["b"]);
        }

        [TestMethod]
        public void TypeStore_LookupWithMismatchingArgumentNames_ReturnsEmptyResult()
        {
            TypeStore<ProgramType> ts = new TypeStore<ProgramType>(new[]
            {
                new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.In))),
                new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.In), ("c", ArgumentMode.In)))
            });
            var types = ts.FindCompatibleTypes(new Valence((ArgumentNameVar.NewUnbound(), ArgumentMode.Out), (new ArgumentNameVar("u"), ArgumentMode.In)));
            Assert.IsFalse(types.Any(), "No results were returned.");
            Assert.AreEqual(0, types.Count(), "Types count should be zero.");
        }

        [TestMethod]
        public void TypeStore_Lookup_ReturnsMultipleTypes()
        {
            TypeStore<ProgramType> ts = new TypeStore<ProgramType>(new[]
            {
                new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.In), ("d", ArgumentMode.In))),
                new ProgramType(new Valence(("a", ArgumentMode.Out), ("b", ArgumentMode.In), ("c", ArgumentMode.In)))
            });
            var types = ts.FindCompatibleTypes(new Valence((ArgumentNameVar.NewUnbound(), ArgumentMode.Out), (new ArgumentNameVar("b"), ArgumentMode.In), (ArgumentNameVar.NewUnbound(), ArgumentMode.In)));
            Assert.IsTrue(types.Any(), "A result was returned.");
            Assert.AreEqual(2, types.Count(), "Two types are found.");
        }


    }
}