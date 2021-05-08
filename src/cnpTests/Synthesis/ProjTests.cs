using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Synthesis
{
  [TestClass]
  public class ProjTests : TestBase
  {
    [TestMethod]
    public void Proj_id()
    {
      string typeStr = "{u:in, v:in}";
      string atusStr = "{{u:1, v:1}}";
      assertFirstResultFor(typeStr, atusStr, proj(id, ("a", "u"), ("b", "v")), "proj_id");
    }

    [TestMethod]
    public void Proj_append()
    {
      string typeStr = "{list1:in, list2:in, appended:out}";
      string atusStr = "{{list1:[1,2,3], list2:[4,5,6], appended:[1,2,3,4,5,6]}}";
      assertFirstResultFor(typeStr, atusStr, proj(foldr(cons, id),("b0", "list2"), ("as", "list1"), ("b", "appended")), "append");

    }

    [TestMethod]
    public void Proj_reverse_by_foldl()
    {
      string typeStr = "{nillist:in, inlist:in, outlist:out}";
      string atusStr = "{{nillist:[], inlist:[3,2,1], outlist:[1,2,3]}}";
      var expectedProgram = proj(foldl(cons, id), ("b0", "nillist"), ("as", "inlist"), ("b", "outlist"));
      assertFirstResultFor(typeStr, atusStr, expectedProgram, "Proj_reverse_by_foldl");
    }


    public void Proj_and_const_reverse()
    {
      string typeStr = "{inlist:in, outlist:out}";
      string atusStr = "{{inlist:[3,2,1], outlist:[1,2,3]}}";

    }

  }
}
