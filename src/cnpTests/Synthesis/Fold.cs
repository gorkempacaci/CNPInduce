using System;
using CNP;
using CNP.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Synthesis
{
    [TestClass]
    public class Fold : TestBase
    {
        [TestMethod]
        public void Append()
        {
            string sigStr = "{a0:in, as:in, b:out}";
            string atusStr = "{{a0:[4,5,6], as:[1,2,3], b:[1,2,3,4,5,6]}}";
            assertSingleResultFor(sigStr, atusStr, foldr(cons, id), "append");
        }
        [TestMethod]
        public void AppendToUnit()
        {
            string sigStr = "{a0:in, as:in, b:out}";
            string atusStr = "{{a0:[4], as:[1,2,3], b:[1,2,3,4]}}";
            assertSingleResultFor(sigStr, atusStr, foldr(cons, id), "append");
        }
        [TestMethod]
        public void AppendToEmpty()
        {
            string sigStr = "{a0:in, as:in, b:out}";
            string atusStr = "{{a0:[], as:[1,2,3], b:[1,2,3]}}";
            assertSingleResultFor(sigStr, atusStr, foldr(cons, id), "append");
        }
    }

    //[TestClass]
    //public class FoldRProjFoldR : TestBase
    //{
    //    [TestMethod]
    //    public void Flatten()
    //    {
    //        string sigStr = "{a0:in, as:in, b:out}";
    //        string atusStr = "{{a0:[], as:[[1,2,3], [4,5,6], [7,8,9]], b:[1,2,3,4,5,6,7,8,9]}}";
    //        assertSingleResultFor(sigStr, atusStr, foldr(foldr(cons, id), id));
    //    }
    //}

    [TestClass]
    public class FoldNegatives : TestBase
    {
        [TestMethod]
        public void AppendFail_1()
        {
            string sigStr = "{a0:in, as:in, b:out}";
            string atusStr = "{{a0:[4,5,6], as:[1,2,3], b:[1,2,3]}}";
            assertNoResultFor(sigStr, atusStr);
        }
    }


}
