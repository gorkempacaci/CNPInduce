using System;
using CNP;
using CNP.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Synthesis
{
    [TestClass]
    public class FoldR : TestBase
    {
        [TestMethod]
        public void Append()
        {
            string typeStr = "{b0:in, as:in, b:out}";
            string atusStr = "{{b0:[4,5,6], as:[1,2,3], b:[1,2,3,4,5,6]}}";
            assertSingleResultFor(typeStr, atusStr, foldr(cons, id), "append");
        }
        [TestMethod]
        public void AppendToUnit()
        {
            string typeStr = "{b0:in, as:in, b:out}";
            string atusStr = "{{b0:[4], as:[1,2,3], b:[1,2,3,4]}}";
            assertSingleResultFor(typeStr, atusStr, foldr(cons, id), "append");
        }
        [TestMethod]
        public void AppendToEmpty()
        {
            string typeStr = "{b0:in, as:in, b:out}";
            string atusStr = "{{b0:[], as:[1,2,3], b:[1,2,3]}}";
            assertSingleResultFor(typeStr, atusStr, foldr(cons, id), "append");
        }
    }

    [TestClass]
    public class FoldL : TestBase
    {
        [TestMethod]
        public void Reverse3()
        {
            string typeStr = "{a0:in, bs:in, a:out}";
            string atusStr = "{{a0:[], bs:[1,2,3], a:[3,2,1]}, {a0:[], bs:[], a:[]}}";
            assertSingleResultFor(typeStr, atusStr, foldl(cons, id), "reverse3");
        }
    }

    // [TestClass]
    // public class FoldRProjFoldR : TestBase
    // {
    //     [TestMethod]
    //     public void Flatten()
    //     {
    //         string typeStr = "{a0:in, as:in, b:out}";
    //         string atusStr = "{{a0:[], as:[[1,2,3], [4,5,6], [7,8,9]], b:[1,2,3,4,5,6,7,8,9]}}";
    //         assertSingleResultFor(typeStr, atusStr, foldr(foldr(cons, id), id));
    //     }
    //
    //     [TestMethod]
    //     public void Reverse2()
    //     {
    //         string typeStr = "{as:in, bs:out}";
    //         string atusStr = "{{as:[1,2,3], bs:[3,2,1]}}";
    //         assertSingleResultFor(typeStr, atusStr, proj(foldl(cons, id), {as,b->bs
    //         }), "reverse2");
    //     }
    // }

    [TestClass]
    public class FoldNegatives : TestBase
    {
        [TestMethod]
        public void AppendFail_1()
        {
            string typeStr = "{a0:in, as:in, b:out}";
            string atusStr = "{{a0:[4,5,6], as:[1,2,3], b:[1,2,3]}}";
            assertNoResultFor(typeStr, atusStr);
        }
    }


}
