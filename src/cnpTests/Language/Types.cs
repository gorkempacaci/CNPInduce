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
      Valence s1 = new Valence((new("a"), Mode.In), (new("b"), Mode.Out));
      Valence s2 = Parser.ParseValence("{b:out, a:in}");
      Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be the same.");
    }

    // [TestMethod]
    // public void TypeWithDifferentArgNames_DifferentHashcodes()
    // {
    //     ProgramType s1 = new ProgramType(new NameModeMap(("a2", Mode.In), ("b", Mode.Out)));
    //     ProgramType s2 = Parser.ParseProgramType("{b:out, a:in}");
    //     Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode(), "Hashcodes should be different");
    // }

    [TestMethod]
    public void TypeWithDifferentArgModes_DifferentHashcodes()
    {
      Valence s1 = new Valence((new("a"), Mode.Out), (new("b"), Mode.Out));
      Valence s2 = Parser.ParseValence("{b:out, a:in}");
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
      TypeStore<Valence> ts = new TypeStore<Valence>(new[]
      {
                new Valence((new("a"), Mode.Out), (new("b"), Mode.In)),
                new Valence((new("a"), Mode.Out), (new("b"), Mode.Out))
            });
      var types = ts.FindCompatibleTypes(new Valence((new("a"), Mode.Out), (new("b"), Mode.Out)));
      Assert.IsTrue(types.Any(), "A result was returned.");
      bool inTheResults =
          types.Contains(new Valence((new("a"), Mode.Out), (new("b"), Mode.Out)));
      Assert.IsTrue(inTheResults, "A newly ground program type should be returned.");
      bool allAreGround = types.All(t => t.IsGround());
      Assert.IsTrue(allAreGround, "All types returned should be ground.");
    }

    [TestMethod]
    public void TypeStore_PartiallyGroundLookup_OnlyReturnsTypesWithPartiallyMatchingSignatures()
    {
      TypeStore<Valence> ts = new TypeStore<Valence>(new[]
      {
                new Valence((new("a"), Mode.Out), (new("b"), Mode.In))
            });
      var types = ts.FindCompatibleTypes(new Valence((NameVar.NewUnbound(), Mode.Out), (new NameVar("b"), Mode.In)));
      Assert.IsTrue(types.Any(), "A result was returned.");
      Assert.AreEqual(1, types.Count(), "Only one type is found.");
      Assert.AreEqual(Mode.Out, types.First()["a"]);
      Assert.AreEqual(Mode.In, types.First()["b"]);
    }

    [TestMethod]
    public void TypeStore_ModeLookup_OnlyReturnsTypesWithCorrectNumberOfInsAndOuts()
    {
      TypeStore<Valence> ts = new TypeStore<Valence>(new[]
      {
                new Valence((new("a"), Mode.Out), (new("b"), Mode.In)),
                new Valence((new("a"), Mode.Out), (new("b"), Mode.In), (new("c"), Mode.In))
            });
      var types = ts.FindCompatibleTypes(new Valence((NameVar.NewUnbound(), Mode.Out), (NameVar.NewUnbound(), Mode.In)));
      Assert.IsTrue(types.Any(), "A result was returned.");
      Assert.AreEqual(1, types.Count(), "Only one type is found.");
      Assert.AreEqual(Mode.Out, types.First()["a"]);
      Assert.AreEqual(Mode.In, types.First()["b"]);
    }

    [TestMethod]
    public void TypeStore_LookupWithMismatchingArgumentNames_ReturnsEmptyResult()
    {
      TypeStore<Valence> ts = new TypeStore<Valence>(new[]
      {
                new Valence((new("a"), Mode.Out), (new("b"), Mode.In)),
                new Valence((new("a"), Mode.Out), (new("b"), Mode.In), (new("c"), Mode.In))
            });
      var types = ts.FindCompatibleTypes(new Valence((NameVar.NewUnbound(), Mode.Out), (new NameVar("u"), Mode.In)));
      Assert.IsFalse(types.Any(), "No results were returned.");
      Assert.AreEqual(0, types.Count(), "Types count should be zero.");
    }

    [TestMethod]
    public void TypeStore_Lookup_ReturnsMultipleTypes()
    {
      TypeStore<Valence> ts = new TypeStore<Valence>(new[]
      {
                new Valence((new("a"), Mode.Out), (new("b"), Mode.In), (new("d"), Mode.In)),
                new Valence((new("a"), Mode.Out), (new("b"), Mode.In), (new("c"), Mode.In))
            });
      var types = ts.FindCompatibleTypes(new Valence((NameVar.NewUnbound(), Mode.Out), (new NameVar("b"), Mode.In), (NameVar.NewUnbound(), Mode.In)));
      Assert.IsTrue(types.Any(), "A result was returned.");
      Assert.AreEqual(2, types.Count(), "Two types are found.");
    }


  }
}