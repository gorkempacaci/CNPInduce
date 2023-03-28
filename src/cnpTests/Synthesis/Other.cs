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

    [TestMethod]
    public void Sum()
    {
      var typeStr = "{list:in, sum:out}";
      var atus = "{{list:[], sum:0}, {list:[1,2,3], sum:6}}";
      var prog = assertFirstResultFor(typeStr, atus, "proj(and(const(b0, 0), foldl(+)), {as->list, b->sum})");
    }


    [TestMethod]
    public void SumAll3()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atus = "{{b0:0, as:[], b:0}, {b0:0, as:[[1,2], [3,4]], b:10}}";
      assertFirstResultFor(typeStr, atus, "foldl(proj(foldl(+), {as->a, b0->b, b->ab}))", 4);
    }

    [TestMethod]
    public void SumAll2()
    {
      string typeStr = "{lists:in, sum:out}";
      string atus = "{{lists:[], sum:0}, {lists:[[1,2], [3,4]], sum:10}}";
      assertFirstResultFor(typeStr, atus, "foldl(proj(foldl(+), {as->a, b0->b, b->ab}))", 5);
    }
  }

}