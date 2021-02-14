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
  }
}
