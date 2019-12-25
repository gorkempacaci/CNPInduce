using System;
using System.Collections.Generic;
using System.Collections;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    public enum ArgumentMode { In, Out }

    public class ProgramType : IEnumerable<KeyValuePair<string, ArgumentMode>>
    {
        readonly int hashcode;
        readonly IReadOnlyDictionary<string, ArgumentMode> dict;

        public ProgramType(params (string, ArgumentMode)[] tups)
            : this(tups.ToDictionary(x => x.Item1, x => x.Item2))
        {}

        public ProgramType(IEnumerable<KeyValuePair<string, ArgumentMode>> args)
            : this(args.ToDictionary(x=>x.Key, x=>x.Value))
        {}

        private ProgramType(IDictionary<string, ArgumentMode> dic)
        {
            dict = new Dictionary<string, ArgumentMode>(dic);
            hashcode = HashCode.OfDictionary(dict);
        }

        public override bool Equals(object obj)
        {
            if (obj is ProgramType otherPs && dict.Keys.Count() == otherPs.Keys.Count())
            {
                return Keys.All(k => this.TryGetValue(k, out ArgumentMode myMode) &&
                                     otherPs.TryGetValue(k, out ArgumentMode othersMode) &&
                                     myMode.Equals(othersMode));
            }
            return false;
        }

        public IEnumerable<string> ArgNames => dict.Keys;

        public ArgumentMode this [string name]
        {
            get => dict[name];
        }

        public IEnumerable<string> Keys => dict.Keys;
        public IEnumerable<ArgumentMode> Values => dict.Values;

        public bool TryGetValue(string name, out ArgumentMode mode)
        {
            return dict.TryGetValue(name, out mode);
        }

        public IEnumerator<KeyValuePair<string, ArgumentMode>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        public override int GetHashCode() => hashcode;
    }

    public abstract class OperatorType
    {
        public readonly ProgramType ExpressionType;

        public OperatorType(ProgramType exprType)
        {
            ExpressionType = exprType;
        }

        public override int GetHashCode()
        {
            return ExpressionType.GetHashCode();
        }
    }
    
    public class FoldType : OperatorType
    {
        public readonly ProgramType RecursiveOperandType;
        public readonly ProgramType BaseOperandType;
        public FoldType(ProgramType recType, ProgramType baseType, ProgramType exprType) : base(exprType)
        {
            RecursiveOperandType = recType;
            BaseOperandType = baseType;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FoldType other))
                return false;
            return ExpressionType.Equals(other.ExpressionType) &&
                   RecursiveOperandType.Equals(other.RecursiveOperandType) &&
                   BaseOperandType.Equals(other.BaseOperandType);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class FoldRType : FoldType
    {
        public FoldRType(ProgramType recType, ProgramType baseType, ProgramType exprType) : base(recType, baseType, exprType)
        {
        }
    }
    
    public class FoldLType : FoldType
    {
        public FoldLType(ProgramType recType, ProgramType baseType, ProgramType exprType) : base(recType, baseType, exprType)
        {
        }
    }
}
