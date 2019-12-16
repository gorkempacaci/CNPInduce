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

namespace Tests.Synthesis
{
    [TestClass]
    public class Elementary : TestBase
    {

        [DataTestMethod]
        [DataRow("{a:in, b:out}", "{{a:0, b:B}, {a:'hello', b:B}}")]
        [DataRow("{a:in, b:out}", "{{a:0, b:0}, {a:'hello', b:B}}")]
        [DataRow("{a:out, b:in}", "{{a:A, b:0}, {a:A, b:'yello'}}")]
        [DataRow("{a:in, b:in}", "{{a:0, b:0}, {a:'hello', b:'hello'}}")]
        [DataRow("{a:in, b:out}", "{{a:[], b:B}}")]
        public void IdPositive(string sigStr, string atusStr)
        {
            assertSingleResultFor(sigStr, atusStr, Id.IdProgram, "id");
        }

        [DataTestMethod]
        [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:0, ab:[0|0]}}")]
        [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:[], ab:[0]}}")]
        [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:[1], ab:[0,1]}}")]
        [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:[1,2], ab:[0,1,2]}}")]
        public void ConsPositive(string sigStr, string atusStr)
        {
            assertSingleResultFor(sigStr, atusStr, Cons.ConsProgram, "cons");
        }

        [DataTestMethod]
        [DataRow("{a:in}", "{{a:0}}", "a", "0")]
        [DataRow("{a:out}", "{{a:'ello'}}", "a", "'ello'")]
        [DataRow("{a:in}", "{{a:[1,2,3]}}", "a", "[1, 2, 3]")]
        [DataRow("{a:in}", "{{a:[1|X]}, {a:[1,2|[3|F]]}, {a:L}, {a:[1,2,T|4]}}", "a", "[1, 2, 3|4]")]
        [DataRow("{a:out}", "{{a:[1|X]}, {a:[1,2|[3|F]]}, {a:L}, {a:[1,2,T|4]}}", "a", "[1, 2, 3|4]")]
        [DataRow("{a:in}", "{{a:[1|X]}, {a:[1,2|[3,F]]}, {a:L}, {a:[1,2,T,4]}}", "a", "[1, 2, 3, 4]")]
        public void ConstPositive(string sigStr, string atusStr, string argName, string constValueStr)
        {
            Term grTerm = Parser.ParseTerm(constValueStr);
            assertSingleResultFor(sigStr, atusStr, new Const(argName, grTerm), "const");
        }

        [DataTestMethod]
        // id
        [DataRow("{a:out, b:out}", "{{a:0, b:0}}")]
        [DataRow("{a:in, b:out}", "{{a:0, b:1}}")]
        // cons
        [DataRow("{a:in, b:in, ab:out}", "{{a:0, b:0, ab:[0,0]}}")]
        // const
        [DataRow("{a:in}", "{{a:[1|X]}, {a:L}, {a:[1,2,T|4]}}")] // not ground
        public void Negative(string sigStr, string atusStr)
        {
            Signature sig = Parser.ParseProgramSignature(sigStr);
            IEnumerable<AlphaTuple> atus = Parser.ParseAlphaTupleSet(atusStr);
            ObservedProgram obs = new ObservedProgram(atus, sig);
            SynthesisJob job = new SynthesisJob(obs, 1);
            var programs = job.FindAllPrograms();
            Assert.AreEqual(0, programs.Count());
        }// code her eis a little delayed to print. code here is a little delayed to print. is it still delayed? No. 
         // does this really help? it seems so. 
    }

}