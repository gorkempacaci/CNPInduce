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

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Combinations_K_2_from_length_1()
    {
      int[][] expected = new int[][]{};
      assert_combinations_k_from_length_n(2, 1, expected);
    }

    [TestMethod]
    public void Combinations_K_2_from_length_2()
    {
      int[][] expected = new int[][] { new[] { 1, 2 } };
      assert_combinations_k_from_length_n(2, 2, expected);
    }

    [TestMethod]
    public void Combinations_K_2_from_length_4()
    {
      int[][] expected = new int[][] { new[]{ 1, 2 }, new[]{ 1, 3 }, new[]{ 1, 4 }, new[]{ 2, 3 }, new[]{ 2, 4 }, new[]{ 3, 4 } };
      assert_combinations_k_from_length_n(2, 4, expected);
    }

    private void assert_combinations_k_from_length_n(int k, int l, int[][] expected)
    {
      var input = Enumerable.Range(1, l);
      var combs = Mathes.Combinations(input, k);
      var actual = combs.Select(c => c.ToArray()).ToArray();
      Assert.AreEqual(actual.Length, expected.Length);
      for (int i = 0; i < actual.Length; i++)
      {
        Assert.IsTrue(Enumerable.SequenceEqual(expected[i], actual[i]), "Actual combinations don't match the expected.");
      }
    }
  }
}
