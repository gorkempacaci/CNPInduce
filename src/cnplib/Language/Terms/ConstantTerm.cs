using System;
using CNP.Helper;

namespace CNP.Language
{
    /// <summary>
    /// Immutable
    /// </summary>
    public class ConstantTerm : Term
    {
        public readonly object Value;
        public ConstantTerm(object o)
        {
            if (!(o is string || o is int))
                throw new Exception("ConstantTerm: Constant should be string or int.");
            Value = o;
        }
        public ConstantTerm(string v) { Value = v; }
        public ConstantTerm(int i) { Value = i; }

        public override bool Equals(object obj)
        {
            if (!(obj is ConstantTerm c))
                return false;
            return this.Value.Equals(c.Value);
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override bool IsGround()
        {
            return true;
        }
        public override Term Clone(TermReferenceDictionary plannedParenthood)
        {
            if (Value is string)
            {
                return new ConstantTerm((string)Value);
            } else if (Value is int)
            {
                return new ConstantTerm((int)Value);
            } else
            {
                throw new Exception("Constant is not string or int. This shouldn't happen.");
            }
        }
        public override bool Contains(Free other)
        {
            return false;
        }
        public override string ToString()
        {
            return Value == null ? "null" : (Value is string valueStr ? "'" + valueStr + "'" : Value.ToString());
        }
    }
}
