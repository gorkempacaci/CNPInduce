using System;
using CNP.Language;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Language
{
    /// <summary>
    /// In valence lookups ProgramSignatures are used as a key so it's important they have consistent hashcodes.
    /// </summary>
    [TestClass]
    public class Signatures
    {
        [TestMethod]
        public void IdenticalSignatures_SameHashcode()
        {
            Signature s1 = new Signature(("a", ArgumentMode.In), ("b", ArgumentMode.Out));
            Signature s2 = Parser.ParseProgramSignature("{b:out, a:in}");
            Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be the same.");
        }

        [TestMethod]
        public void SigWithDifferentArgNames_DifferentHashcodes()
        {
            Signature s1 = new Signature(("a2", ArgumentMode.In), ("b", ArgumentMode.Out));
            Signature s2 = Parser.ParseProgramSignature("{b:out, a:in}");
            Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be different");
        }

        [TestMethod]
        public void SigWithDifferentArgModes_DifferentHashcodes()
        {
            Signature s1 = new Signature(("a", ArgumentMode.Out), ("b", ArgumentMode.Out));
            Signature s2 = Parser.ParseProgramSignature("{b:out, a:in}");
            Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be different");
        }

        public void Hashcode_CombineHashes_Associativity()
        {

        }

    }
}