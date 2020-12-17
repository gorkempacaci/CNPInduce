using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Language;
using CNP.Helper.EagerLinq;

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
            var types = parts.Select(ParseNameModeMap).ToArray();
            return (TOperatorType)Activator.CreateInstance(typeof(TOperatorType), (object[])types);
        }

        public static Valence ParseNameModeMap(string nameModeMap)
        {
            var lexems = Lexer.Tokenize(nameModeMap);
            var it = lexems.GetEnumerator();
            if (!it.MoveNext())
                throw new ParserException("Expecting program type.", it.Current);
            return ReadNameModeMap(it);
        }
        public static ProgramType ParseProgramType(string typeString)
        {
            return new ProgramType(ParseNameModeMap(typeString));
        }
        public static IEnumerable<AlphaTuple> ParseAlphaTupleSet(string alphaSetString)
        {
            var lexems = Lexer.Tokenize(alphaSetString);
            var it = lexems.GetEnumerator();
            if (!it.MoveNext())
                throw new ParserException("Expecting set of alpha tuples.", it.Current);
            if (ReadAlphaTupleSet(it, out IEnumerable<AlphaTuple> tuples))
            {
                return tuples;
            }
            else
            {
                return null;
            }
        }
        public static AlphaTuple ParseAlphaTuple(string str)
        {
            var lexems = Lexer.Tokenize(str);
            var it = lexems.GetEnumerator();
            Move(it);
            if (ReadAlphaTuple(it, new NamedVariables(), out AlphaTuple tup))
                return tup;
            else return null;
        }
        public static Term ParseTerm(string termString)
        {
            var lexems = Lexer.Tokenize(termString);
            var it = lexems.GetEnumerator();
            if (!it.MoveNext())
                throw new ParserException("Nothing to parse in term.", it.Current);
            Term term = ReadTerm(it, new NamedVariables());
            return term;
        }



        static Valence ReadNameModeMap(IEnumerator<Lexem> it)
        {
            List<KeyValuePair<string, ArgumentMode>> nameModePairs = new List<KeyValuePair<string, ArgumentMode>>();
            GetType(it, TokenType.MustacheOpen);
            Move(it);
            while (ReadNameMode(it, out KeyValuePair<string, ArgumentMode> nameMode))
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
        static bool ReadAlphaTupleSet(IEnumerator<Lexem> it, out IEnumerable<AlphaTuple> tuples)
        {
            List<AlphaTuple> _tuples = new List<AlphaTuple>();
            GetType(it, TokenType.MustacheOpen);
            Move(it);
            while (ReadAlphaTuple(it, new NamedVariables(), out AlphaTuple at))
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
        static bool ReadAlphaTuple(IEnumerator<Lexem> it, NamedVariables frees, out AlphaTuple at)
        {
            List<KeyValuePair<ArgumentNameVar, Term>> namedTerms = new List<KeyValuePair<ArgumentNameVar, Term>>();
            GetType(it, TokenType.MustacheOpen);
            Move(it);
            while (ReadNameTerm(it, frees, out KeyValuePair<ArgumentNameVar, Term> nameTerm))
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
        static bool ReadNameMode(IEnumerator<Lexem> it, out KeyValuePair<string, ArgumentMode> nameMode)
        {
            string name = GetContent(it, TokenType.Identifier, "A name:mode pair should start with an identifier.");
            Move(it);
            string colon = GetContent(it, TokenType.Colon, "A name:mode pair is missing a colon(:)");
            Move(it);
            ArgumentMode mode = GetMode(it);
            nameMode = new KeyValuePair<string, ArgumentMode>(name, mode);
            return true;
        }
        static bool ReadNameTerm(IEnumerator<Lexem> it, NamedVariables frees, out KeyValuePair<ArgumentNameVar, Term> nameTerm)
        {
            string name = GetContent(it, TokenType.Identifier, "A name:term pair should start with an identifier.");
            Move(it);
            string colon = GetContent(it, TokenType.Colon, "A name:term pair is missing a colon(:)");
            Move(it);
            Term term = ReadTerm(it, frees);
            nameTerm = new KeyValuePair<ArgumentNameVar, Term>(new ArgumentNameVar(name), term);
            return true;
        }
        static Term ReadTerm(IEnumerator<Lexem> it, NamedVariables frees)
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
        static Term ReadTermList(IEnumerator<Lexem> it, NamedVariables frees)
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
        public static ArgumentMode GetMode(IEnumerator<Lexem> it)
        {
            string modeString = GetContent(it, TokenType.Identifier, "A name:mode pair is missing a mode.");
            modeString = modeString.Trim();
            if (modeString == "in")
                return ArgumentMode.In;
            else if (modeString == "out")
                return ArgumentMode.Out;
            else throw new ParserException("Unrecognized argument mode:" + modeString, it.Current);
        }
    }

    public class ParserException : Exception
    {
        public ParserException(string message, Lexem current) : base(message + "(" + current?.ToString() + ")") { }
    }
}
