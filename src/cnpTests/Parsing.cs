using System;
using System.Linq;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CNP.Language;
using CNP.Parsing;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class Parsing : TestBase
    {
        [TestMethod]
        public void Tokenizer()
        {
            string input = @"{a:goo, b:Boo, c:[a, Y, [0, Boo]]}";
            List<Lexem> expectedTokens = new List<Lexem>()
            {
                new Lexem(TokenType.MustacheOpen, "{", 0),
                new Lexem(TokenType.Identifier, "a", 1),
                new Lexem(TokenType.Colon, ":", 2),
                new Lexem(TokenType.Identifier, "goo", 3),
                new Lexem(TokenType.Comma, ",",6),
                new Lexem(TokenType.Identifier, "b",8),
                new Lexem(TokenType.Colon, ":",9),
                new Lexem(TokenType.VariableName, "Boo",10),
                new Lexem(TokenType.Comma, ",",13),
                new Lexem(TokenType.Identifier, "c",15),
                new Lexem(TokenType.Colon, ":",16),
                new Lexem(TokenType.BracketOpen, "[",17),
                new Lexem(TokenType.Identifier, "a",18),
                new Lexem(TokenType.Comma, ",",19),
                new Lexem(TokenType.VariableName, "Y",21),
                new Lexem(TokenType.Comma, ",",22),
                new Lexem(TokenType.BracketOpen, "[",24),
                new Lexem(TokenType.ValueInt, "0",25),
                new Lexem(TokenType.Comma, ",",26),
                new Lexem(TokenType.VariableName, "Boo",28),
                new Lexem(TokenType.BracketClose, "]",31),
                new Lexem(TokenType.BracketClose, "]",32),
                new Lexem(TokenType.MustacheClose, "}",33)
            };
            Lexer lx = new Lexer();
            IEnumerable<Lexem> actualTokens = Lexer.Tokenize(input);
            CollectionAssert.AreEqual(expectedTokens, actualTokens.ToList());
        }

        [TestMethod]
        public void ProgramSignature()
        {
            string sigStr = "{a:in, b:out, c:in}";
            Signature parsedSig = Parser.ParseProgramSignature(sigStr);
            Signature expectedSig = new Signature(("a", ArgumentMode.In),
                                                                ("b", ArgumentMode.Out),
                                                                ("c", ArgumentMode.In));
            Assert.AreEqual(expectedSig, parsedSig);
        }

        [TestMethod]
        public void NietBruijn_String()
        {
            Free A = new Free(), B = new Free();
            AlphaTuple atu = new AlphaTuple(("a", cnst("aa")), ("b", list(A, list(A), list(B, cnst("b"), A))));
            AlphaTuple atuNB = nietBruijn(atu);
            Assert.AreEqual("{a:'aa', b:['λ0', ['λ0'], ['λ1', 'b', 'λ0']]}", atuNB.ToString());
        }

        [TestMethod]
        public void TermList()
        {
            string listStr = "[1, 'b', [C, 'd'], [], C]";
            Term t = Parser.ParseTerm(listStr);
            t = nietBruijnTerm(t);
            string tStr = t.ToString();
            Assert.AreEqual("[1, 'b', ['λ0', 'd'], [], 'λ0']", tStr);
        }

        [TestMethod]
        public void TermCons()
        {
            string listStr = "[0|'u']";
            Term t = Parser.ParseTerm(listStr);
            string tStr = t.ToString();
            Assert.AreEqual("[0|'u']", tStr);
        }

        [TestMethod]
        public void TermCons_MultipleInTheHead()
        {
            string listStr = "[0,1,2|'u']";
            Term t = Parser.ParseTerm(listStr);
            string tStr = t.ToString();
            Assert.AreEqual("[0, 1, 2|'u']", tStr);
        }

        [TestMethod]
        public void TermCons_MultipleInTheHead_VarTail()
        {
            string listStr = "[0,1,2|X]";
            Term t = Parser.ParseTerm(listStr);
            string tStr = nietBruijnTerm(t).ToString();
            Assert.AreEqual("[0, 1, 2|'λ0']", tStr);
        }

        [TestMethod]
        public void AlphaTupleSet()
        {
            string atuStr = "{{a:'aa', b:B}, {a:B, b:[B,'a']}, {a:'aa', c:B, d:0}}";
            List<AlphaTuple> atu = Parser.ParseAlphaTupleSet(atuStr).ToList();
            var t0 = atu[0];
            var t1 = atu[1];
            var t2 = atu[2];
            Assert.AreEqual(3, atu.Count);
            Assert.AreEqual("{a:'aa', b:'λ0'}", nietBruijn(t0).ToString());
            Assert.AreEqual("{a:'λ0', b:['λ0', 'a']}", nietBruijn(t1).ToString());
            Assert.AreEqual("{a:'aa', c:'λ0', d:0}", nietBruijn(t2).ToString());
        }
        [TestMethod]
        public void AlphaTuple()
        {
            string atuStr = "{a:'aa', b:[A, [A], B, 'b', A]}";
            AlphaTuple atu = Parser.ParseAlphaTuple(atuStr);
            atu = nietBruijn(atu);
            Assert.AreEqual("{a:'aa', b:['λ0', ['λ0'], 'λ1', 'b', 'λ0']}", atu.ToString());
        }

        //{a:'aa', b:['λ0', ['λ0'], 'λ1', 'b', 'λ0']}
        //{a:'aa', b:['λ0', ['λ0'], 'λ1', 'b', _2]}>. 

        [TestMethod]
        public void EmptyListParsesAsNil()
        {
            string emptyListStr = "[ ]";
            Term emptyList = Parser.ParseTerm(emptyListStr);
            Assert.AreEqual(NilTerm.Instance, emptyList);
        }

        [TestMethod]
        public void ListConstruction_ParsesSameAs_List()
        {
            string listStrNested = "[1| [A| [A| [4|[]]]]]";
            string listStrList = "[1, B, B, 4]";
            Term listNested = Parser.ParseTerm(listStrNested);
            Term listList = Parser.ParseTerm(listStrList);
            Term listNestedNB = nietBruijnTerm(listNested);
            Term listListNB = nietBruijnTerm(listList);
            string strOfNested = listNestedNB.ToString();
            string strOfList = listListNB.ToString();
            Assert.AreEqual(strOfNested, strOfList);
        }
    }
}