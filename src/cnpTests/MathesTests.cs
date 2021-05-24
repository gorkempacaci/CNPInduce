using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using CNP.Parsing;
using CNP.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Helpers
{
  [TestClass]
  public class MathesTests : TestBase
  {
    [TestMethod]
    public void Combinations_Test_0()
    {
      assert_combinations(Array.Empty<int>(), new int[][] {  });
    }

    [TestMethod]
    public void Combinations_Test_1()
    {
      assert_combinations(new[] {1}, new int[][]{ new[] {1} });
    }

    [TestMethod]
    public void Combinations_Test_2()
    {
      assert_combinations(new[] { 1, 2 }, new int[][] { new[] { 1 }, new[] { 2 }, new[] { 1, 2 } });
    }

    [TestMethod]
    public void Combinations_Test_3()
    {
      assert_combinations(new[] { 1, 2, 3 }, new int[][] {  new[] { 1 },
                                                            new[] { 2 },
                                                            new[] { 3 },
                                                            new[] { 2, 3 },
                                                            new[] { 1, 2 },
                                                            new[] { 1, 3 },
                                                            new[] { 1, 2, 3 } });
    }

    private void assert_combinations(int[] items, int[][] expectedCombinations)
    {
      var actualCombinations = Mathes.Combinations(items).Select(c => c.ToArray()).ToArray(); ;
      Assert.AreEqual(expectedCombinations.Length, actualCombinations.Length);
      for(int i=0; i<actualCombinations.Length; i++)
      { 
        Assert.IsTrue(Enumerable.SequenceEqual(expectedCombinations[i], actualCombinations[i]), "Combinations should be identical.");
      }
    }
  }
}
