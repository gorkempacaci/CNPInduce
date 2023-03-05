using System;
using CNP.Language;
using System.Collections;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Language
{
  [TestClass]
  public class ValenceVarTests : TestBase
  {



    [TestMethod]
    public void Match_Var_2_Ins_1_Out_All_Ground()
    {
      var series = ElementaryValenceSeries.SeriesFromArrays(new[] { "a", "b", "ab", }, new Mode[][] { new[] { In, In, Out },
                                                                                            new[] { Out, Out, In },
                                                                                            new[] { In, In, In},
                                                                                            new[] { In, Out, In} });
      NameVarBindings nvars = new NameVarBindings();
      var v1 = nvars.AddNameVar("a");
      var v2 = nvars.AddNameVar("b");
      var v3 = nvars.AddNameVar("ab");

      ValenceVar vv = new ValenceVar(new[] { v1, v2 }, new[] { v3 });
      var expected = new [] { (ins:new[]{"a", "b"}, outs:new[]{"ab"}) };
      series.GroundingAlternatives(vv, nvars, out var alts);
      Assert.IsTrue(alts.Any(), "Some alternatives should be returned.");
      assert_ground_valence_equality(expected, alts.ToArray());
    }

    [TestMethod]
    public void Match_Var_4_Ins_1_Ground()
    {
      var series = ElementaryValenceSeries.SeriesFromArrays(new[] { "a", "b", "c", "d" }, new Mode[][] { new[] { In, In, In, In } });
      NameVarBindings nvars = new NameVarBindings();
      var v1 = nvars.AddNameVar(null);
      var v2 = nvars.AddNameVar("b");
      var v3 = nvars.AddNameVar(null);
      var v4 = nvars.AddNameVar(null);
      ValenceVar vv = new ValenceVar(new[] { v1, v2, v3, v4 }, Array.Empty<NameVar>());
      series.GroundingAlternatives(vv, nvars, out var alts);

      var expected = new (string[] ins, string[] outs)[] {
        (ins:new[]{"a", "b", "c", "d"}, outs:Array.Empty<string>()),
        (ins:new[]{"a", "b", "d", "c"}, outs:Array.Empty<string>()),
        (ins:new[]{"c", "b", "a", "d"}, outs:Array.Empty<string>()),
        (ins:new[]{"c", "b", "d", "a"}, outs:Array.Empty<string>()),
        (ins:new[]{"d", "b", "a", "c"}, outs:Array.Empty<string>()),
        (ins:new[]{"d", "b", "c", "a"}, outs:Array.Empty<string>())
      };
      assert_ground_valence_equality(expected, alts.ToArray());
      
    }


    private void assert_ground_valence_equality((string[] ins, string[] outs)[] expected, (string[] ins, string[] outs)[] actual)
    {
      if (expected.Length == actual.Length)
      {
        for (int i = 0; i < expected.Length; i++)
        {
          CollectionAssert.AreEqual(expected[i].ins, actual[i].ins, "Expected valence does not equal matched.");
          CollectionAssert.AreEqual(expected[i].outs, actual[i].outs, "Expected valence does not equal matched.");
        }
      }
      else Assert.Fail("Matched wrong number of valences.");
    }
  }
}

