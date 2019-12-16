using System;
using CNP;
using CNP.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Synthesis
{
    [TestClass]
    public class FoldR : TestBase
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

    [TestClass]
    public class FoldL : TestBase
    {
        [TestMethod]
        public void Reverse3()
        {
            try
            {
                string sigStr = "{a0:in, as:in, b:out}";
                string atusStr = "{{a0:[], as:[1,2,3], b:[3,2,1]}}";
                assertSingleResultFor(sigStr, atusStr, foldl(cons, id), "reverse3");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
