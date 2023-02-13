using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Language;
using System.Linq;

namespace CNP.Parsing
{
  public class Parser
  {


    public static ValenceVar ParseValence(string nameModeMap, NameVarBindings names)
    {
      var lexems = Lexer.Tokenize(nameModeMap);
      var it = lexems.GetEnumerator();
      if (!it.MoveNext())
        throw new ParserException("Expecting program type.", it.Current);
      return ReadNameModeMap(it, names);
    }

    public static AlphaRelation ParseAlphaTupleSet(string alphaSetString, NameVarBindings namevars, FreeFactory frees)
    {
      var lexems = Lexer.Tokenize(alphaSetString);
      var it = lexems.GetEnumerator();
      if (!it.MoveNext())
        throw new ParserException("Expecting set of alpha tuples.", it.Current);
      // scope of name variables is the whole set of alphatuples. all tuples should have the same namevar references.
      FreeDictionary freeDict = new(frees);
      if (ReadAlphaTupleSet(it, namevars, freeDict, out AlphaRelation tuples))
      {
        return tuples;
      }
      else
      {
        return default;
      }
    }
    public static List<KeyValuePair<NameVar, ITerm>> ParseAlphaTuple(string str, NameVarBindings namevars, FreeDictionary freeDict)
    {
      var lexems = Lexer.Tokenize(str);
      var it = lexems.GetEnumerator();
      Move(it);
      // scope of namevars can be shared among alphatuples but scope of term-level frees is the alpha tuple, so we create one free lookup for each tuple.
      if (ReadAlphaTuple(it, namevars, freeDict, out var tup))
        return tup;
      else return null;
    }

    public static ITerm ParseTerm(string termString, FreeDictionary frees)
    {
      var lexems = Lexer.Tokenize(termString);
      var it = lexems.GetEnumerator();
      if (!it.MoveNext())
        throw new ParserException("Nothing to parse in term.", it.Current);
      ITerm term = ReadTerm(it, frees);
      return term;
    }



    static ValenceVar ReadNameModeMap(IEnumerator<Lexem> it, NameVarBindings namevars)
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
          return ValenceVar.FromDict(new(nameModePairs));
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
    static bool ReadAlphaTupleSet(IEnumerator<Lexem> it, NameVarBindings namevars,  FreeDictionary freeDict, out AlphaRelation rel)
    {
      List<ITerm[]> _tuples = new();
      GetType(it, TokenType.MustacheOpen);
      Move(it);
      NameVar[] nameVarsControl = null;
      // context is shared among tuples but the scope of term-level variables is the alpha tuple.
      while (ReadAlphaTuple(it, namevars, freeDict, out var at))
      {
        var names = at.Select(t => t.Key).ToArray();
        if (nameVarsControl == null)
          nameVarsControl = names;
        else
        {
          if (!nameVarsControl.SequenceEqual(names))
            throw new ArgumentException("Parsed names are not equal between rows:" + namevars + " and " + nameVarsControl);
        }
        var terms = at.Select(t => t.Value).ToArray();
        _tuples.Add(terms);
        Move(it);
        TokenType type = GetType(it, TokenType.Comma, TokenType.MustacheClose);
        if (type == TokenType.MustacheClose)
        {
          rel = new AlphaRelation(nameVarsControl, _tuples.ToArray());
          return true;
        }
        else // comma
        {
          Move(it);
        }
      }
      rel = default;
      return false;
    }
    static bool ReadAlphaTuple(IEnumerator<Lexem> it, NameVarBindings namevars, FreeDictionary freeDict, out List<KeyValuePair<NameVar,ITerm>> at)
    {
      List<KeyValuePair<NameVar, ITerm>> namedTerms = new List<KeyValuePair<NameVar, ITerm>>();
      GetType(it, TokenType.MustacheOpen);
      Move(it);
      while (ReadNameTerm(it, namevars, freeDict, out KeyValuePair<NameVar, ITerm> nameTerm))
      {
        namedTerms.Add(nameTerm);
        Move(it);
        TokenType type = GetType(it, TokenType.Comma, TokenType.MustacheClose);
        if (it.Current.Type == TokenType.MustacheClose)
        {
          at = namedTerms;
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
    static bool ReadNameMode(IEnumerator<Lexem> it, NameVarBindings bindings, out KeyValuePair<NameVar, Mode> nameMode)
    {
      string name = GetContent(it, TokenType.Identifier, "A name:mode pair should start with an identifier.");
      NameVar nameAsVar = GetOrAddNameVar(bindings, name);
      Move(it);
      string colon = GetContent(it, TokenType.Colon, "A name:mode pair is missing a colon(:)");
      Move(it);
      Mode mode = GetMode(it);
      nameMode = new KeyValuePair<NameVar, Mode>(nameAsVar, mode);
      return true;
    }

    static bool ReadNameTerm(IEnumerator<Lexem> it, NameVarBindings namevars, FreeDictionary freeDict, out KeyValuePair<NameVar, ITerm> nameTerm)
    {
      string name = GetContent(it, TokenType.Identifier, "A name:term pair should start with an identifier.");
      NameVar nameAsVar = GetOrAddNameVar(namevars, name);
      Move(it);
      string colon = GetContent(it, TokenType.Colon, "A name:term pair is missing a colon(:)");
      Move(it);
      ITerm term = ReadTerm(it, freeDict);
      nameTerm = new KeyValuePair<NameVar, ITerm>(nameAsVar, term);
      return true;
    }
    static ITerm ReadTerm(IEnumerator<Lexem> it, FreeDictionary freeDict)
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
        return freeDict.GetOrAdd(it.Current.Content);
      }
      if (it.Current.Type == TokenType.BracketOpen)
      {
        return ReadTermList(it, freeDict);
      }
      throw new ParserException("Term can be: int, 'string', Var, or a list of [terms]. Position:", it.Current);
    }
    static ITerm ReadTermList(IEnumerator<Lexem> it, FreeDictionary freeDict)
    {
      List<ITerm> elements = new List<ITerm>();
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
          ITerm tail = ReadTerm(it, freeDict);
          Move(it);
          GetType(it, TokenType.BracketClose, "List construction '|' should be closed with a bracket(])");
          return TermList.FromEnumerable(elements, tail);
        }
        else
        {
          ITerm t = ReadTerm(it, freeDict);
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

    private static NameVar GetOrAddNameVar(NameVarBindings bindings, string name)
    {
      int i = Array.IndexOf(bindings.Names, name);
      if (i >= 0)
        return new NameVar(i);
      else return bindings.AddNameVar(name);
    }
  }

  public class ParserException : Exception
  {
    public ParserException(string message, Lexem current) : base(message + "(" + current?.ToString() + ")") { }
  }
}
