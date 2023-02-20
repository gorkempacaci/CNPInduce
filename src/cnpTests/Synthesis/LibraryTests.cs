using System;
using CNP.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Synthesis
{
  [TestClass]
  public class LibraryTests : TestBase
  {

    [DataTestMethod]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:1, ab:1}}")]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:0, ab:0}}")]
    [DataRow("{a:in, b:in, ab:out}", "{{a:1, b:1, ab:2}}")]
    public void Plus(string typeStr, string atusStr)
    {
      assertFirstResultFor(typeStr, atusStr, "+");
    }

    [TestMethod]
    public void FoldL_Plus()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atus = "{{b0:0, as:[1,2,3], b:6}}";
      assertFirstResultFor(typeStr, atus, "foldl(+, id)");
    }

    [TestMethod]
    public void FoldL_Plus_2()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atus = "{{b0:6, as:[4,5,6], b:21}}";
      assertFirstResultFor(typeStr, atus, "foldl(+, id)");
    }

    [TestMethod]
    public void FoldL_Plus_3()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atus = "{{b0:6, as:[4,5,6], b:B}, {b0:B, as:[1,2,3], b:27}}";
      assertFirstResultFor(typeStr, atus, "foldl(+, id)");
    }


    [TestMethod]
    public void FoldL_Plus_Intermediate()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atus = "{{b0:0, as:[1,2,3], b:F0}," +
        "             {b0:F0, as:[4,5,6], b:F1}," +
        "             {b0:F1, as:[7,8,9], b:45}}";
      assertFirstResultFor(typeStr, atus, "foldl(+, id)");
    }


    [TestMethod]
    public void Proj_FoldL_Plus()
    {
      string typeStr = "{a:in, b:in, ab:out}";
      string atus = "{{a:0, b:[1,2,3], ab:6}}";
      assertFirstResultFor(typeStr, atus, "proj(foldl(+, id), {b0->a, as->b, b->ab})");
    }


    [TestMethod]
    public void FoldL_Proj_FoldL_Plus()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atus = "{{b0:0, as:[[1,2,3], [4,5,6], [7,8,9]], b:45}}";
      assertFirstResultFor(typeStr, atus, "foldl(proj(foldl(+, id), {as->a, b0->b, b->ab}), id)", 4);
    }

    //[TestMethod]
    //public void Proj_FoldL_Proj_FoldL_Plus()
    //{
    //  string typeStr = "{as:in, b:out}";
    //  string atus = "{{as:[[]], b:0}, {as:[[1,2,3], [4,5,6], [7,8,9]], b:45}}";
    //  assertFirstResultFor(typeStr, atus, "foldl(proj(foldl(+, id), {as->a, b0->b, b->ab}), id)", 5);
    //}
  }
}

