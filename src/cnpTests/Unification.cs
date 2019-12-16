using Microsoft.VisualStudio.TestTools.UnitTesting;
using CNP.Language;
using CNP.Helper;
using System.Diagnostics;
using System;



    [TestClass]
    public class Unification : TestBase
    {


        [TestMethod]
        public void ListsWithComplexTermsUnify()
        {
            Free X = new Free(), Y = new Free(), Z = new Free();
            ConstantTerm a = new ConstantTerm("a"),
                         u = new ConstantTerm("u"),
                         i = new ConstantTerm("i"),
                         o = new ConstantTerm("o");
            Term left = list(list(X, a), u, X, list(i, Y, X), Z);
            Term right = list(list(Z, Y), u, X, list(i, Y, X), o);
            var success = Term.UnifyInPlace(left, right);
            Assert.IsTrue(success);
            string unifier = "[['o', 'a'], 'u', 'o', ['i', 'a', 'o'], 'o']";
            Assert.AreEqual(unifier, left.ToString(), "Left term unified correctly.");
            Assert.AreEqual(unifier, right.ToString(), "Right term unified correctly.");
        }

        [TestMethod]
        public void ListUnifiesWithConsToFormUngroundTerm()
        {
            Term left = list(cnst(1), new Free(), cnst(3));
            Term right = cns(cnst(1), new Free());
            var success = Term.UnifyInPlace(left, right);
            Assert.AreEqual(true, success, "List unifies with cons to unground");
            string unifier = "[1, 'λ0', 3]";
            Assert.AreEqual(unifier, nietBruijnTerm(left).ToString());
            Assert.AreEqual(unifier, nietBruijnTerm(right).ToString());
        }

        [TestMethod]
        public void ListUnifiesWithConsToFormGroundTerm()
        {
            Free a = new Free();
            Term left = list(a, list(cnst("u"), a, cnst("y")), cnst(3));
            Term right = cns(cnst(1), new Free());
            var success = Term.UnifyInPlace(left, right);
            Assert.AreEqual(true, success, "List unifies with cons to ground");
            string unifier = "[1, ['u', 1, 'y'], 3]";
            Assert.AreEqual(unifier, nietBruijnTerm(left).ToString());
            Assert.AreEqual(unifier, nietBruijnTerm(right).ToString());
            Assert.AreEqual(true, left.IsGround(), "Left term is ground");
            Assert.AreEqual(true, right.IsGround(), "Right term is ground");
        }

        [TestMethod]
        // Making sure keeping track of Frees work while they're nested within lists.
        public void FreeIsSubstitutedWhileInsideList()
        {
            Free a = new Free();
            Term t = list(cnst(1), list(cnst("a"), list(cnst("i"), a, cnst("iii")), cnst("c")), cnst(3));
            a.SubstituteInContainers(cnst("ii"));
            Assert.AreEqual("[1, ['a', ['i', 'ii', 'iii'], 'c'], 3]", t.ToString(), "Free is unified without tuple.");
        }


        [TestMethod]
        // Making sure keeping track of Frees work while they're nested within lists.
        public void FreeIsSubstitutedInNestedListTail()
        {
            Free a = new Free();
            Term t = list(cnst(1), list(cnst("a"), list(cnst("i"), cnst("ii"), a), cnst("c")), cnst(3));
            a.SubstituteInContainers(cnst("iii"));
            Assert.AreEqual("[1, ['a', ['i', 'ii', 'iii'], 'c'], 3]", t.ToString(), "Free is unified without tuple.");
        }


    }

