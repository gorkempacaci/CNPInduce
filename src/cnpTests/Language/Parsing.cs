using System;
using System.Linq;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CNP.Language;
using CNP.Parsing;
using CNP;
using System.Collections.Generic;

namespace Language
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
                new Lexem(TokenType.Comma, ",", 6),
                new Lexem(TokenType.Identifier, "b", 8),
                new Lexem(TokenType.Colon, ":", 9),
                new Lexem(TokenType.VariableName, "Boo", 10),
                new Lexem(TokenType.Comma, ",", 13),
                new Lexem(TokenType.Identifier, "c", 15),
                new Lexem(TokenType.Colon, ":", 16),
                new Lexem(TokenType.BracketOpen, "[", 17),
                new Lexem(TokenType.Identifier, "a", 18),
                new Lexem(TokenType.Comma, ",", 19),
                new Lexem(TokenType.VariableName, "Y", 21),
                new Lexem(TokenType.Comma, ",", 22),
                new Lexem(TokenType.BracketOpen, "[", 24),
                new Lexem(TokenType.ValueInt, "0", 25),
                new Lexem(TokenType.Comma, ",", 26),
                new Lexem(TokenType.VariableName, "Boo", 28),
                new Lexem(TokenType.BracketClose, "]", 31),
                new Lexem(TokenType.BracketClose, "]", 32),
                new Lexem(TokenType.MustacheClose, "}", 33)
            };
      Lexer lx = new Lexer();
      IEnumerable<Lexem> actualTokens = Lexer.Tokenize(input);
      CollectionAssert.AreEqual(expectedTokens, actualTokens.ToList());
    }

    [TestMethod]
    public void Valence()
    {
      NameVarBindings names = new();
      NameVarBindings names2 = new();
      string typeStr = "{a:in, c:in, b:out}";
      ValenceVar parsedType = Parser.ParseValence(typeStr, names);
      ValenceVar expectedType = new ValenceVar(new[] { names2.AddNameVar("a"), names2.AddNameVar("c") },
        new[] { names2.AddNameVar("b") });
      Assert.AreEqual(expectedType.Accept(PrettyStringer.Contextless), parsedType.Accept(PrettyStringer.Contextless));
    }

    [DataTestMethod]
    [DataRow("[1, 'b', [C, 'd'], [], C]", "[1, 'b', [F0, 'd'], [], F0]", "Complex list term")]
    [DataRow("[0|'u']", "[0|'u']", "Cons-ed list term")]
    [DataRow("[0,1,2|'u']", "[0, 1, 2|'u']", "Head-heavy list")]
    [DataRow("[0,1,2|X]", "[0, 1, 2|F0]", "Var-tailed list")]
    [DataRow("[ ]", "[]", "Empty list")]
    [DataRow("[1| [A| [A| [4|[]]]]]", "[1, F0, F0, 4]", "Nested list")]
    public void TermParsesCorrectly(string parsedStr, string expectedStr, string termDescription)
    {
      string listStr = parsedStr;
      ITerm t = Parser.ParseTerm(listStr, new(new()));
      string tStr = contextless(t);
      Assert.AreEqual(expectedStr, tStr, termDescription+" parses correctly.");
    }


    [TestMethod]
    public void AlphaTupleSet()
    {
      NameVarBindings nameBinds = new();
      string atuStr = "{{a:'aa', b:B}, {a:B, b:[B,'a']}, {a:'aa', b:0}}";
      AlphaRelation rel = Parser.ParseAlphaTupleSet(atuStr, nameBinds, new());
      Assert.AreEqual(3, rel.TuplesCount);
      PrettyStringer ps = new PrettyStringer(nameBinds);
      string[] colNames = nameBinds.GetNamesForVars(rel.Names);
      Assert.AreEqual("{a:'aa', b:F0}", ps.Visit(rel.Tuples[0], colNames)); 
      Assert.AreEqual("{a:F0, b:[F0, 'a']}", ps.Visit(rel.Tuples[1], colNames));
      Assert.AreEqual("{a:'aa', b:0}", ps.Visit(rel.Tuples[2], colNames));
    }



    //{a:'aa', b:['λ0', ['λ0'], 'λ1', 'b', 'λ0']}
    //{a:'aa', b:['λ0', ['λ0'], 'λ1', 'b', _2]}>. 


  }
}