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
  public class Elementary : TestBase
  {

    [DataTestMethod]
    [DataRow("{a:in, b:out}", "{{a:0, b:B}, {a:'hello', b:C}}")]
    [DataRow("{a:in, b:out}", "{{a:0, b:0}, {a:'hello', b:C}}")]
    [DataRow("{a:out, b:in}", "{{a:A, b:0}, {a:B, b:'yello'}}")]
    [DataRow("{a:in, b:in}", "{{a:0, b:0}, {a:'hello', b:'hello'}}")]
    [DataRow("{a:in, b:in}", "{{a:1, b:1}}")]
    [DataRow("{a:in, b:out}", "{{a:[], b:B}}")]
    [DataRow("{a:in, b:out}", "{{a:[3,2,1], b:[3,2,1]}}")]
    public void IdPositive(string typeStr, string atusStr)
    {
      assertFirstResultFor(typeStr, atusStr, "id");
    }

    [DataTestMethod]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:0, ab:[0|0]}}")]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:[], ab:[0]}}")]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:[1], ab:[0,1]}}")]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:[1,2], ab:[0,1,2]}}")]
    public void ConsPositive(string typeStr, string atusStr)
    {
      assertFirstResultFor(typeStr, atusStr, "cons");
    }

    [TestMethod]
    public void ConsValencesGrounding()
    {
      /*            new[]{  Mode.In,  Mode.In, Mode.In},
                    new[]{  Mode.In,  Mode.In, Mode.Out},
                    new[]{  Mode.In, Mode.Out, Mode.In},
                    new[]{  Mode.Out, Mode.In, Mode.In},
                    new[]{  Mode.Out, Mode.Out, Mode.In},
       */
      // observ.IsAllOutArgumentsGround() needs to be called after cons unifies to check if it actually grounded.
      assertFirstResultFor("{a:in,  b:in,  ab:in}",  "{{a:0, b:[1], ab:[0,1]}}", "cons");
      assertFirstResultFor("{a:in,  b:in,  ab:out}", "{{a:0, b:[1], ab:F0}}", "cons");
      assertFirstResultFor("{a:in,  b:out, ab:in}",  "{{a:0, b:F0, ab:[0,1]}}", "cons");
      assertFirstResultFor("{a:out, b:in,  ab:in}",  "{{a:F0, b:[1], ab:[0,1]}}", "cons");
      assertFirstResultFor("{a:out, b:out, ab:in}",  "{{a:F0, b:F1, ab:[0,1]}}", "cons");
    }

    [DataTestMethod]
    [DataRow("{a:in}", "{{a:0}}", "a", "0")]
    [DataRow("{a:out}", "{{a:'ello'}}", "a", "'ello'")]
    [DataRow("{a:in}", "{{a:[1,2,3]}}", "a", "[1, 2, 3]")]
    [DataRow("{a:in}", "{{a:[1|X]}, {a:[1,2|[3|F]]}, {a:L}, {a:[1,2,T|4]}}", "a", "[1, 2, 3|4]")]
    [DataRow("{a:out}", "{{a:[1|X]}, {a:[1,2|[3|F]]}, {a:L}, {a:[1,2,T|4]}}", "a", "[1, 2, 3|4]")]
    [DataRow("{a:in}", "{{a:[1|X]}, {a:[1,2|[3,F]]}, {a:L}, {a:[1,2,T,4]}}", "a", "[1, 2, 3, 4]")]
    //[DataRow("{a:in}", "{{a:[]}}")]
    public void ConstPositive(string typeStr, string atusStr, string argName, string constValueStr)
    {
      //ITerm grTerm = Parser.ParseTerm(constValueStr, new());
      //assertFirstResultFor(typeStr, atusStr, new Const(new NameVar(argName), grTerm), "const");
      assertFirstResultFor(typeStr, atusStr, "const("+argName+", "+constValueStr+")");
    }

    [TestMethod]
    public void ConstInvented()
    {
      var typeStr = "{b0:in}";
      var atus = "{{b0:Z}}";
      var prog = assertFirstResultFor(typeStr, atus, "const(b0, [])");
    }

    [TestMethod]
    public void ConstInventedInFold()
    {
      var typeStr = "{as:in, bs:out}";
      var atus = "{{as:['a'], bs:['a']}, {as:[1,2,3], bs:[3,2,1]}}";
      var prog = assertFirstResultFor(typeStr, atus, "proj(and(const(b0, []), foldl(cons)), {as->as, b->bs})");
    }

    [DataTestMethod]
    // // cons
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:0, ab:[2]}, {a:1, b:2, ab:[16]}}")]
    // const
    // [DataRow("{a:in}", "{{a:[1|X]}, {a:L}, {a:[1,2,3,4,T|6]}}")] // not ground
    public void Negative(string typeStr, string atusStr)
    {
      assertNoResultFor(typeStr, atusStr, 3);
    }

  }

}