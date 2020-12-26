using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Synthesis
{
  [TestClass]
  public class FoldLTests : TestBase
  {
    [TestMethod]
    public void Reverse3()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[], as:[1,2,3], b:[3,2,1]}}";
      assertSingleResultFor(typeStr, atusStr, foldl(cons, id), "reverse3");
    }


  }
}
