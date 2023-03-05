using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CNP.Language;
using System.Linq;
using System.Text;

namespace Language
{
  [TestClass]
  public class ValenceSeriesTests
  {
    private const Mode IN = Mode.In;
    private const Mode OUT = Mode.Out;

    [DataTestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public void AndOpBaseModes(int maxArity) // where maxArity <= 3
    {
      var expectedModeLists = new Mode[][][] {
        new Mode[][]{
          new Mode[]{ Mode.In },
          new Mode[]{ Mode.Out } },
        new Mode[][] {
          new Mode[]{ Mode.In, Mode.In },
          new Mode[]{ Mode.In, Mode.Out },
          new Mode[]{ Mode.Out, Mode.In },
          new Mode[]{ Mode.Out, Mode.Out } },
        new Mode[][]{
          new Mode[]{ Mode.In, Mode.In, Mode.In },
          new Mode[]{ Mode.In, Mode.In, Mode.Out },
          new Mode[]{ Mode.In, Mode.Out, Mode.In },
          new Mode[]{ Mode.In, Mode.Out, Mode.Out },
          new Mode[]{ Mode.Out, Mode.In, Mode.In },
          new Mode[]{ Mode.Out, Mode.In, Mode.Out },
          new Mode[]{ Mode.Out, Mode.Out, Mode.In },
          new Mode[]{ Mode.Out, Mode.Out, Mode.Out } }
      };
      //int expectedLength = Enumerable.Range(1, maxArity).Select(ar => (int)Math.Pow(2, ar)).Sum();
      //expectedModeLists = expectedModeLists.Take(expectedLength).ToArray();
      var actualModeLists = AndValenceSeries.AndOpModeLists(maxArity).ToArray();
      for (int arity = 1; arity <= maxArity; arity++)
      {
        Assert.AreEqual(expectedModeLists[arity-1].Length, actualModeLists[arity-1].Length);
        for (int i = 0; i < expectedModeLists[arity-1].Length; i++)
        {
          CollectionAssert.AreEqual(expectedModeLists[arity-1][i], actualModeLists[arity-1][i], $"Mode lists at index {i} should be equal.");
        }
      }
    }

    [TestMethod(displayName: "And/1:IN")]
    public void And_OnlyIN()
    {
      var modes = new Mode[] { Mode.In };
      var protosActual = AndValenceSeries.GenerateForSingleOpModeList(modes).ToArray();
      var protosExpected = new ProtoAndValence[]
      {
        new ProtoAndValence(modes, new Mode?[]{Mode.In}, new Mode?[]{Mode.In}, Array.Empty<short>(), Array.Empty<short>()),
        new ProtoAndValence(modes, new Mode?[]{Mode.In}, new Mode?[]{Mode.Out}, Array.Empty<short>(), Array.Empty<short>()),
        new ProtoAndValence(modes, new Mode?[]{Mode.Out}, new Mode?[]{Mode.In}, Array.Empty<short>(), Array.Empty<short>()),
        new ProtoAndValence(modes, new Mode?[]{Mode.Out}, new Mode?[]{Mode.Out}, Array.Empty<short>(), Array.Empty<short>())
      };
      assertProtoValsEqual(protosExpected, protosActual);
    }

    [TestMethod(displayName:"And/1:OUT")]
    public void And_OnlyOUT()
    {
      var modes = new Mode[] { Mode.Out };
      var protosActual = AndValenceSeries.GenerateForSingleOpModeList(modes).ToArray();
      var protosExpected = new ProtoAndValence[]
      {
        new ProtoAndValence(modes, new Mode?[]{Mode.Out}, new Mode?[]{Mode.In}, Array.Empty<short>(), Array.Empty<short>()),
        new ProtoAndValence(modes, new Mode?[]{Mode.Out}, new Mode?[]{Mode.Out}, Array.Empty<short>(), Array.Empty<short>())
      };
      assertProtoValsEqual(protosExpected, protosActual);
    }


    [DataTestMethod]
    [DataRow(48, IN, IN, DisplayName ="And/2:IN,IN")]
    [DataRow(24, IN, OUT, DisplayName = "And/2:IN,OUT")]
    [DataRow(24, OUT, IN, DisplayName = "And/2:OUT,IN")]
    [DataRow(12, OUT, OUT, DisplayName = "And/2:OUT,OUT")]
    public void And_Arity2(int expectedNumberOfProtoValences, params Mode[] modes)
    {
      var protosActual = AndValenceSeries.GenerateForSingleOpModeList(modes).ToArray();
      string protosString = allProtosToString(protosActual);
      Assert.AreEqual(expectedNumberOfProtoValences, protosActual.Length);
    }


    private static string allProtosToString(ProtoAndValence[] protos)
    {
      StringBuilder sb = new StringBuilder();
      foreach (ProtoAndValence e in protos)
      {
        string lhModesJoined = string.Join(", ", e.LHModes.Select(m => m is null ? "null" : m.ToString()));
        string rhModesJoined = string.Join(", ", e.RHModes.Select(m => m is null ? "null" : m.ToString()));
        string onlyLHJoined = string.Join(", ", e.OnlyLHIndices.Select(m => m.ToString()));
        string onlyRHJoined = string.Join(", ", e.OnlyRHIndices.Select(m => m.ToString()));
        sb.AppendLine($"new ProtoAndValence(modes, new Mode?[]{{{lhModesJoined}}}, new Mode?[]{{{rhModesJoined}}}, new short[]{{{onlyLHJoined}}}, new short[]{{{onlyRHJoined}}}),");
      }
      string s = sb.ToString();
      return s;
    }

    private static void assertProtoValsEqual(ProtoAndValence[] expecteds, ProtoAndValence[] actuals)
    {
      Assert.AreEqual(expecteds.Length, actuals.Length);
      for (int i = 0; i < expecteds.Length; i++)
      {
        CollectionAssert.AreEqual(expecteds[i].OpModes, actuals[i].OpModes);
        CollectionAssert.AreEqual(expecteds[i].LHModes, actuals[i].LHModes);
        CollectionAssert.AreEqual(expecteds[i].RHModes, actuals[i].RHModes);
        CollectionAssert.AreEqual(expecteds[i].OnlyLHIndices, actuals[i].OnlyLHIndices);
        CollectionAssert.AreEqual(expecteds[i].OnlyRHIndices, actuals[i].OnlyRHIndices);
      }
    }
  }
}

