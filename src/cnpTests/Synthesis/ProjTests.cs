using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Synthesis
{
  [TestClass]
  public class ProjTests : TestBase
  {
    [TestMethod]
    public void proj_id()
    {
      string typeStr = "{u:in, v:in}";
      string atusStr = "{{u:1, v:1}}";
      assertFirstResultFor(typeStr, atusStr, proj(id, ("a", "u"), ("b", "v")), "proj_id");
    }

    [TestMethod]
    public void proj_append()
    {
      string typeStr = "{list1:in, list2:in, appended:out}";
      string atusStr = "{{list1:[1,2,3], list2:[4,5,6], appended:[1,2,3,4,5,6]}}";
      assertFirstResultFor(typeStr, atusStr, proj(foldr(cons, id),("b0", "list2"), ("as", "list1"), ("b", "appended")), "append");

    }

    public void proj_negative()
    {
      
    }
  }
}
