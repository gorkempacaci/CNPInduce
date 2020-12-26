using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Language;
using CNP.Helper.EagerLinq;
using CNP.Parsing;
using Helper;

namespace CNP.Parsing
{
  public class Parser
  {
    /// <summary>
    /// Operator types should follow the structure Operand1Type -> ... -> OperandNType -> ExpressionType
    /// For example: {a:in, b:in, ab:out} -> {a:in, b:out} -> {a0:in, bs:in, a:out}
    /// </summary>
    public static TOperatorType ParseOperatorType<TOperatorType>(string operatorTypeString)
    {
      string[] parts = operatorTypeString.Split(new[] { "->" }, StringSplitOptions.None);
      if (parts.Length < 2)
      {
        throw new Exception("Operator type must have at least 2 components.");
      }
      NameVarDictionary namevars = new(); // name variable scope is the operator type
      var types = parts.Select(s => ParseNameModeMap(s, namevars)).ToArray();
      return (TOperatorType)Activator.CreateInstance(typeof(TOperatorType), (object[])types);
    }

    public static Valence ParseNameModeMap(string nameModeMap, NameVarDictionary namevars)
    {
      var lexems = Lexer.Tokenize(nameModeMap);
      var it = lexems.GetEnumerator();
      if (!it.MoveNext())
        throw new ParserException("Expecting program type.", it.Current);
      return ReadNameModeMap(it, namevars);
    }
    public static ProgramType ParseProgramType(string typeString)
    {
      return new ProgramType(ParseNameModeMap(typeString, new()));
    }
    public static IEnumerable<AlphaTuple> ParseAlphaTupleSet(string alphaSetString, NameVarDictionary namevars)
    {
      var lexems = Lexer.Tokenize(alphaSetString);
      var it = lexems.GetEnumerator();
      if (!it.MoveNext())
        throw new ParserException("Expecting set of alpha tuples.", it.Current);
      // scope of name variables is the whole set of alphatuples. all tuples should have the same namevar references.
      if (ReadAlphaTupleSet(it, namevars, out IEnumerable<AlphaTuple> tuples))
      {
        return tuples;
      }
      else
      {
        return null;
      }
    }
    public static AlphaTuple ParseAlphaTuple(string str, NameVarDictionary namevars)
    {
      var lexems = Lexer.Tokenize(str);
      var it = lexems.GetEnumerator();
      Move(it);
      // scope of namevars can be shared among alphatuples but scope of term-level frees is the alpha tuple, so we create one free lookup for each tuple.
      if (ReadAlphaTuple(it, namevars, out AlphaTuple tup))
        return tup;
      else return null;
    }

    public static Term ParseTerm(string termString, FreeDictionary frees)
    {
      var lexems = Lexer.Tokenize(termString);
      var it = lexems.GetEnumerator();
      if (!it.MoveNext())
        throw new ParserException("Nothing to parse in term.", it.Current);
      Term term = ReadTerm(it, frees);
      return term;
    }



    static Valence ReadNameModeMap(IEnumerator<Lexem> it, NameVarDictionary namevars)
    {
      List<KeyValuePair<NameVar, Mode>> nameModePairs = new();
      GetType(it, TokenType.MustacheOpen);
      Move(it);
      while (ReadNameMode(it, namevars, out KeyValuePair<NameVar, Mode> nameMode))
      {
        nameModePairs.Add(nameMode);
        Move(it);
        var type = GetType(it, TokenType.Comma, TokenType.MustacheClose);
        if (type == TokenType.MustacheClose)
        {
          return new Valence(nameModePairs);
        }
        else
        {
          Move(it);
        }
      }
      throw new ParserException("Invalid program type.", it.Current);
    }
    /// <summary>
    /// Variable scope is the tuple scope (Variables do not reach between alpha-tuples)
    /// </summary>
    static bool ReadAlphaTupleSet(IEnumerator<Lexem> it, NameVarDictionary namevars, out IEnumerable<AlphaTuple> tuples)
    {
      List<AlphaTuple> _tuples = new();
      GetType(it, TokenType.MustacheOpen);
      Move(it);
      // context is shared among tuples but the scope of term-level variables is the alpha tuple.
      while (ReadAlphaTuple(it, namevars, out AlphaTuple at))
      {
        _tuples.Add(at);
        Move(it);
        TokenType type = GetType(it, TokenType.Comma, TokenType.MustacheClose);
        if (type == TokenType.MustacheClose)
        {
          tuples = _tuples;
          return true;
        }
        else // comma
        {
          Move(it);
        }
      }
      tuples = null;
      return false;
    }
    static bool ReadAlphaTuple(IEnumerator<Lexem> it, NameVarDictionary namevars, out AlphaTuple at)
    {
      List<KeyValuePair<NameVar, Term>> namedTerms = new List<KeyValuePair<NameVar, Term>>();
      GetType(it, TokenType.MustacheOpen);
      Move(it);
      FreeDictionary frees = new(); 
      while (ReadNameTerm(it, namevars, frees, out KeyValuePair<NameVar, Term> nameTerm))
      {
        namedTerms.Add(nameTerm);
        Move(it);
        TokenType type = GetType(it, TokenType.Comma, TokenType.MustacheClose);
        if (it.Current.Type == TokenType.MustacheClose)
        {
          at = new AlphaTuple(namedTerms);
          return true;
        }
        else // comma
        {
          Move(it);
        }
      }
      at = null;
      return false;
    }
    static bool ReadNameMode(IEnumerator<Lexem> it, NameVarDictionary namevars, out KeyValuePair<NameVar, Mode> nameMode)
    {
      string name = GetContent(it, TokenType.Identifier, "A name:mode pair should start with an identifier.");
      NameVar nameAsVar = namevars.GetOrAdd(name);
      Move(it);
      string colon = GetContent(it, TokenType.Colon, "A name:mode pair is missing a colon(:)");
      Move(it);
      Mode mode = GetMode(it);
      nameMode = new KeyValuePair<NameVar, Mode>(nameAsVar, mode);
      return true;
    }

    static bool ReadNameTerm(IEnumerator<Lexem> it, NameVarDictionary namevars, FreeDictionary frees, out KeyValuePair<NameVar, Term> nameTerm)
    {
      string name = GetContent(it, TokenType.Identifier, "A name:term pair should start with an identifier.");
      NameVar nameAsVar = namevars.GetOrAdd(name);
      Move(it);
      string colon = GetContent(it, TokenType.Colon, "A name:term pair is missing a colon(:)");
      Move(it);
      Term term = ReadTerm(it, frees);
      nameTerm = new KeyValuePair<NameVar, Term>(nameAsVar, term);
      return true;
    }
    static Term ReadTerm(IEnumerator<Lexem> it, FreeDictionary frees)
    {
      if (it.Current.Type == TokenType.ValueInt)
      {
        return new ConstantTerm(int.Parse(it.Current.Content));
      }
      if (it.Current.Type == TokenType.ValueString)
      {
        return new ConstantTerm(it.Current.Content.Trim('\''));
      }
      if (it.Current.Type == TokenType.VariableName)
      {
        return frees.GetOrAdd(it.Current.Content);
      }
      if (it.Current.Type == TokenType.BracketOpen)
      {
        return ReadTermList(it, frees);
      }
      throw new ParserException("Term can be: int, 'string', Var, or a list of [terms]. Position:", it.Current);
    }
    static Term ReadTermList(IEnumerator<Lexem> it, FreeDictionary frees)
    {
      List<Term> elements = new List<Term>();
      GetType(it, TokenType.BracketOpen, "Term list should start with [");
      while (it.MoveNext())
      {
        if (it.Current.Type == TokenType.BracketClose)
        {
          return TermList.FromEnumerable(elements);
        }
        else if (it.Current.Type == TokenType.Comma)
        {

        }
        else if (it.Current.Type == TokenType.Pipe)
        {
          if (elements.Count() == 0)
            throw new ParserException("List construction '|' needs at least one element in the head.", it.Current);
          if (!it.MoveNext())
            throw new ParserException("List construction '|' needs a tail.", it.Current);
          Term tail = ReadTerm(it, frees);
          Move(it);
          GetType(it, TokenType.BracketClose, "List construction '|' should be closed with a bracket(])");
          return TermList.FromEnumerable(elements, tail);
        }
        else
        {
          Term t = ReadTerm(it, frees);
          elements.Add(t);
        }
      }
      throw new ParserException("Term list is missing contents.", it.Current);
    }


    static string GetContent(IEnumerator<Lexem> it, TokenType type, string assertMessage)
    {
      GetType(it, new[] { type }, assertMessage);
      return it.Current.Content;
    }
    static TokenType GetType(IEnumerator<Lexem> it, params TokenType[] allowedTypes)
    {
      return GetType(it, allowedTypes, null);
    }
    static TokenType GetType(IEnumerator<Lexem> it, TokenType type, string assertMessage)
    {
      return GetType(it, new[] { type }, assertMessage);
    }
    static TokenType GetType(IEnumerator<Lexem> it, TokenType[] allowedTypes, string assertMessage)
    {
      if (!allowedTypes.Contains(it.Current.Type))
      {
        string exceptionMessage = assertMessage ??
            "Expecting (" + string.Join(",", allowedTypes) + ")";
        throw new ParserException(exceptionMessage, it.Current);
      }
      return it.Current.Type;
    }
    static void Move(IEnumerator<Lexem> it)
    {
      if (!it.MoveNext())
      {
        throw new ParserException("Expecting content", it.Current);
      }
    }
    public static Mode GetMode(IEnumerator<Lexem> it)
    {
      string modeString = GetContent(it, TokenType.Identifier, "A name:mode pair is missing a mode.");
      modeString = modeString.Trim();
      if (modeString == "in")
        return Mode.In;
      else if (modeString == "out")
        return Mode.Out;
      else throw new ParserException("Unrecognized argument mode:" + modeString, it.Current);
    }
  }

  public class ParserException : Exception
  {
    public ParserException(string message, Lexem current) : base(message + "(" + current?.ToString() + ")") { }
  }
}
