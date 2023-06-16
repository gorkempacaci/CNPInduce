using System;
using System.Collections.Generic;
using CNP.Helper;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Helpers
{
  [TestClass]
  public class Iterators
  {
    [DataTestMethod]
    [DataRow(new int[]{}, 0)]
    [DataRow(new []{1}, 1, new []{1})]
    [DataRow(new []{1,2}, 2, new []{1,2}, new []{2,1})]
    [DataRow(new []{1,2,3}, 6, new []{1,2,3}, new[]{1,3,2}, new[]{2,1,3}, new[]{2,3,1}, new[]{3,1,2}, new[]{3,2,1})]
    public void Permutations(IEnumerable<int> source, int nPerms, params int[][] correctPerm)
    {
      var perms = source.Permutations().ToArray();
      int len = perms.Length;
      Assert.AreEqual(nPerms, len, "Number of permutations is correct.");
      for(int i=0; i<correctPerm.Length; i++)
      {
        bool eq = perms[i].SequenceEqual(correctPerm[i]);
        Assert.IsTrue(eq, "Permutations are correct.");
      }
    }

    [DataTestMethod]
    [DataRow(new int[] {}, new int[] {}, true)]
    [DataRow(new int[] {1}, new int[] {1}, true)]
    [DataRow(new int[] {1}, new int[] {2}, false)]
    [DataRow(new int[] {1}, new int[] {1,2}, false)]
    [DataRow(new int[] {1,2}, new int[] {2,1}, true)]
    [DataRow(new int[] {1,2,3}, new int[] {2,3,1}, true)]
    public void EqualsAsSet(int[] aas, int[] bs, bool equal)
    {
      Assert.AreEqual(equal, aas.EqualsAsSet(bs));
    }
  }
}