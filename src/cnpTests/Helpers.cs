using System;
using System.Collections;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Helpers
{
    class ConstructorExample
    {
        public int A { get; }
        public  string B { get; }

        public ConstructorExample(int a)
        {
            A = a;
        }
        public ConstructorExample(int a, string b)
        {
            A = a;
            B = b;
        }
    }
    [TestClass]
    public class Helpers
    {

        [TestMethod]
        public void NewFromArray()
        {
            List<object[]> li = new List<object[]>
            {
                new object[] {5, "bes"},
                new object[] {7, "yedi"}
            };
            IEnumerable<ConstructorExample> ce = li.New<ConstructorExample>();
            var li2 = Enumerable.ToList(ce);
            Assert.AreEqual(li2[0].A, 5);
            Assert.AreEqual(li2[0].B, "bes");
            Assert.AreEqual(li2[1].A, 7);
            Assert.AreEqual(li2[1].B, "yedi");
        }

        [TestMethod]
        public void NewFromObject()
        {
            IEnumerable<int> ints = new int[] {1, 2};
            IEnumerable<ConstructorExample> objs = ints.New<ConstructorExample,int>();
            var o = objs.ToArray();
            Assert.AreEqual(o[0].A, 1);
            Assert.AreEqual(o[1].A, 2);
        }

        [TestMethod]
        public void NewFromTuple()
        {
            List<Tuple<int,string>> li = new List<Tuple<int,string>>
            {
                new Tuple<int, string>(5, "bes"),
                new Tuple<int, string>(7, "yedi")
            };
            IEnumerable<ConstructorExample> ce = li.New<ConstructorExample,int,string>();
            var li2 = Enumerable.ToList(ce);
            Assert.AreEqual(li2[0].A, 5);
            Assert.AreEqual(li2[0].B, "bes");
            Assert.AreEqual(li2[1].A, 7);
            Assert.AreEqual(li2[1].B, "yedi");
        }
        
        [TestMethod]
        public void NewFromValueTuple()
        {
            List<(int, string)> li = new List<(int, string)>
            {
                (5, "bes"),
                (7, "yedi")
            };
            IEnumerable<ConstructorExample> ce = li.New<ConstructorExample,int,string>();
            var li2 = Enumerable.ToList(ce);
            Assert.AreEqual(li2[0].A, 5);
            Assert.AreEqual(li2[0].B, "bes");
            Assert.AreEqual(li2[1].A, 7);
            Assert.AreEqual(li2[1].B, "yedi");
        }
    }
}