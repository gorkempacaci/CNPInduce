using System;
using CNP.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Search
{
  [TestClass]
  public class DifferenceTableTests
  {
    private DifferenceTable table;

    [TestInitialize]
    public void Setup()
    {
      table = new DifferenceTable(5);
      table.AssertDifferent(2, 3);
    }

    [TestMethod]
    public void AssertionHoldsTrue()
    {
      Assert.IsTrue(table.IsDifferent(2, 3));
      Assert.IsTrue(table.IsDifferent(3, 2));
    }

    [TestMethod]
    public void ContourCasesFalse()
    {
      Assert.IsFalse(table.IsDifferent(2, 1));
      Assert.IsFalse(table.IsDifferent(1, 2));
      Assert.IsFalse(table.IsDifferent(3, 1));
      Assert.IsFalse(table.IsDifferent(1, 3));
      Assert.IsFalse(table.IsDifferent(4, 1));
      Assert.IsFalse(table.IsDifferent(1, 1));
      Assert.IsFalse(table.IsDifferent(4, 2));
      Assert.IsFalse(table.IsDifferent(2, 4));
      Assert.IsFalse(table.IsDifferent(4, 3));
      Assert.IsFalse(table.IsDifferent(3, 4));
    }

    [TestMethod]
    public void IdentityDifferenceReturnsFalse()
    {
      Assert.IsFalse(table.IsDifferent(0, 0));
      Assert.IsFalse(table.IsDifferent(2, 2));
      Assert.IsFalse(table.IsDifferent(4, 4));
    }
  }
}

