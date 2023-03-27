using Microsoft.VisualStudio.TestTools.UnitTesting;
using CNP;
using System.Diagnostics;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CNP.Parsing;
using CNP.Language;
using CNP.Search;
using CNP.Helper;

namespace Synthesis
{
  [TestClass]
  public class Other : TestBase
  {

    [TestMethod]
    public void Head()
    {
      var typeStr = "{list:in, h:out}";
      var atus = "{{list:[1,2,3], h:1}, {list:[2], h:2}}";
      var prog = assertFirstResultFor(typeStr, atus, "proj(cons, {ab->list, a->h})");
    }
  }

}