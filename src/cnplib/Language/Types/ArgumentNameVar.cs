using System;
using CNP.Helper;

namespace CNP.Language
{
    /// <summary>
    /// Meta-variable for name for an argument. Can be bound to a ground identifier or can be free.
    /// </summary>
    public class ArgumentNameVar : Term, IComparable
    {
        private string _name;
        private readonly int _argNameId = System.Threading.Interlocked.Increment(ref _argNameCounter);
        private static int _argNameCounter = 0;

        
        public string Name => _name ?? ("#"+_argNameId.ToString());

        private ArgumentNameVar()
        {
            _name = null;
        }
        
        public ArgumentNameVar(string name)
        {
            BindName(name);
        }

        public bool BindName(string name)
        {
            Validator.AssertArgumentName(name);
            if (IsGround())
                return false;
            else
            {
                _name = name;
                return true;
            }
        }

        public override bool IsGround() => _name != null;

        public override bool Contains(Free other)
        {
            return false;
        }

        /// <summary>
        /// Returns true if both names are ground and have the same name, or if they're the same object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is ArgumentNameVar other))
            {
                return false;
            } else if (this.IsGround() && other.IsGround())
            {
                return Name == other.Name;
            } else if (!this.IsGround() && !other.IsGround())
            {
                return object.ReferenceEquals(this, other);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return IsGround() ? Name.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return Name;
        }

        public override ArgumentNameVar Clone(TermReferenceDictionary plannedParenthood)
        {
            return plannedParenthood.AddOrGet(this, () => IsGround() ? new ArgumentNameVar(_name) : ArgumentNameVar.NewUnbound()) as ArgumentNameVar;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is ArgumentNameVar otherName))
                throw new Exception("ArgumentName.CompareTo: obj is not ArgumentName");
            if (this.IsGround() && otherName.IsGround())
                return String.Compare(_name, otherName._name, StringComparison.Ordinal);
            else return _argNameId.CompareTo(otherName._argNameId);
        }

        public static ArgumentNameVar NewUnbound()
        {
            return new ArgumentNameVar();
        }
    }
}