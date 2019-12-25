using System;
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
            ProgramType s1 = new ProgramType(("a", ArgumentMode.In), ("b", ArgumentMode.Out));
            ProgramType s2 = Parser.ParseProgramType("{b:out, a:in}");
            Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be the same.");
        }

        [TestMethod]
        public void TypeWithDifferentArgNames_DifferentHashcodes()
        {
            ProgramType s1 = new ProgramType(("a2", ArgumentMode.In), ("b", ArgumentMode.Out));
            ProgramType s2 = Parser.ParseProgramType("{b:out, a:in}");
            Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be different");
        }

        [TestMethod]
        public void TypeWithDifferentArgModes_DifferentHashcodes()
        {
            ProgramType s1 = new ProgramType(("a", ArgumentMode.Out), ("b", ArgumentMode.Out));
            ProgramType s2 = Parser.ParseProgramType("{b:out, a:in}");
            Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be different");
        }

        public void Hashcode_CombineHashes_Associativity()
        {

        }

    }
}