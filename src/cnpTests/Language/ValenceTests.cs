using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using CNP.Language;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Language
{
  [TestClass]
  public class ValenceTests : TestBase
  {

    [TestMethod]
    public void AndOrValence_I()
    {
      Valence opv = Parser.ParseValence("{a:in}");
      IEnumerable<AndOrValence> expectedVs = new List<AndOrValence>
      {
        Parser.ParseFunctionalValence<AndOrValence>("{a:in} -> {a:in} -> {a:in}"),
        Parser.ParseFunctionalValence<AndOrValence>("{a:in} -> {a:out} -> {a:in}"),
        Parser.ParseFunctionalValence<AndOrValence>("{a:out} -> {a:in} -> {a:in}"),
        Parser.ParseFunctionalValence<AndOrValence>("{a:out} -> {a:out} -> {a:in}")
      };
      
      IEnumerable<AndOrValence> actualVs = AndOrValence.Generate(opv);

      Assert.IsTrue(expectedVs.SequenceEqual(actualVs), "AndOrValence generation single I");
    }

    [TestMethod]
    public void AndOrValence_O()
    {
      Valence opv = Parser.ParseValence("{a:out}");
      IEnumerable<AndOrValence> expectedVs = new List<AndOrValence>
      {
        Parser.ParseFunctionalValence<AndOrValence>("{a:out} -> {a:in} -> {a:out}"),
        Parser.ParseFunctionalValence<AndOrValence>("{a:out} -> {a:out} -> {a:out}")
      };

      IEnumerable<AndOrValence> actualVs = AndOrValence.Generate(opv);

      Assert.IsTrue(expectedVs.SequenceEqual(actualVs), "AndOrValence generation single I");
    }

    [TestMethod]
    public void AndOrValence_IO()
    {
      Valence opv = Parser.ParseValence("{a:in, b:out}");
      var expectedVsStr = new string[]
      {
         "{a:in} -> {a:in, b:out} -> {a:in, b:out}" // a -> a,b ->
        ,"{a:in} -> {a:out, b:out} -> {a:in, b:out}" 
        ,"{a:out} -> {a:in, b:out} -> {a:in, b:out}"
        ,"{a:out} -> {a:out, b:out} -> {a:in, b:out}"
        ,"{a:in, b:out} -> {a:in} -> {a:in, b:out}"  // a,b -> a ->
        ,"{a:in, b:out} -> {a:out} -> {a:in, b:out}"
        ,"{a:out, b:out} -> {a:in} -> {a:in, b:out}"
        ,"{a:out, b:out} -> {a:out} -> {a:in, b:out}"
        ,"{b:out} -> {a:in, b:in} -> {a:in, b:out}" // b -> a,b ->
        ,"{b:out} -> {a:in, b:out} -> {a:in, b:out}"
        ,"{b:out} -> {a:out, b:in} -> {a:in, b:out}" // b -> a,b ->
        ,"{b:out} -> {a:out, b:out} -> {a:in, b:out}"
        ,"{a:in, b:out} -> {b:in} -> {a:in, b:out}" // a,b -> b ->
        ,"{a:in, b:out} -> {b:out} -> {a:in, b:out}"
        ,"{a:out, b:out} -> {b:in} -> {a:in, b:out}"
        ,"{a:out, b:out} -> {b:out} -> {a:in, b:out}"
        ,"{a:in, b:out} -> {a:in, b:in} -> {a:in, b:out}" // a,b -> a,b ->
        ,"{a:in, b:out} -> {a:in, b:out} -> {a:in, b:out}" 
        ,"{a:in, b:out} -> {a:out, b:in} -> {a:in, b:out}" 
        ,"{a:in, b:out} -> {a:out, b:out} -> {a:in, b:out}"
        ,"{a:out, b:out} -> {a:in, b:in} -> {a:in, b:out}"
        ,"{a:out, b:out} -> {a:in, b:out} -> {a:in, b:out}"
        ,"{a:out, b:out} -> {a:out, b:in} -> {a:in, b:out}"
        ,"{a:out, b:out} -> {a:out, b:out} -> {a:in, b:out}"
      };
      var expectedVs = expectedVsStr.Select(Parser.ParseFunctionalValence<AndOrValence>);

      IEnumerable<AndOrValence> actualVs = AndOrValence.Generate(opv);

      bool eq = expectedVs.SequenceEqualPos(actualVs, out int pos);
      Assert.IsTrue(eq, "AndOrValence generation IO unexpected at: " + pos);
    }
  }
}
