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
      assertFirstResultFor(typeStr, atusStr, "id", "id");
    }

    [DataTestMethod]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:0, ab:[0|0]}}")]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:[], ab:[0]}}")]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:[1], ab:[0,1]}}")]
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:[1,2], ab:[0,1,2]}}")]
    public void ConsPositive(string typeStr, string atusStr)
    {
      assertFirstResultFor(typeStr, atusStr, "cons", "cons");
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
      assertFirstResultFor(typeStr, atusStr, "const("+argName+", "+constValueStr+")", "const");
    }

    [DataTestMethod]
    // cons
    [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:0, ab:[0,0]}}")]
    // const
    [DataRow("{a:in}", "{{a:[1|X]}, {a:L}, {a:[1,2,T|4]}}")] // not ground
    public void Negative(string typeStr, string atusStr)
    {
      assertNoResultFor(typeStr, atusStr, "neg cons");
      //NameVarDictionary namevars = new();
      //Valence namesModes = Parser.ParseNameModeMap(typeStr, namevars);
      //IEnumerable<AlphaTuple> atus = Parser.ParseAlphaTupleSet(atusStr, namevars);
      //assertNoResultFor(namesModes, atus);
      //SynthesisJob job = new SynthesisJob(atus, namesModes);
      //var programs = job.FindAllPrograms();
      //Assert.AreEqual(0, programs.Count());
    }
  }

}