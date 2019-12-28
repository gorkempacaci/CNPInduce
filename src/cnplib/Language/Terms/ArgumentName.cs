using System;
using CNP.Helper;

namespace CNP.Language
{
    /// <summary>
    /// A name for an argument. Can be bound to a ground identifier or can be free. 
    /// </summary>
    public class ArgumentName : IComparable
    {
        private string _name;
        private readonly int _argNameId = _argNameCounter++;
        private static int _argNameCounter = 0;

        
        public string Name => _name ?? ("#"+_argNameId.ToString());

        private ArgumentName()
        {
            _name = null;
        }
        
        public ArgumentName(string name)
        {
            BindName(name);
        }

        public void BindName(string name)
        {
            Validator.AssertArgumentName(name);
            _name = name;
        }

        public bool IsGround => _name != null;
        
        /// <summary>
        /// Returns true if both names are ground and have the same name, or if they're the same object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is ArgumentName other))
            {
                return false;
            } else if (this.IsGround && other.IsGround)
            {
                return Name == other.Name;
            } else if (!this.IsGround && !other.IsGround)
            {
                return object.ReferenceEquals(this, other);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return _argNameId * 31;
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is ArgumentName otherName))
                throw new Exception("ArgumentName.CompareTo: obj is not ArgumentName");
            if (this.IsGround && otherName.IsGround)
                return String.Compare(_name, otherName._name, StringComparison.Ordinal);
            else return _argNameId.CompareTo(otherName._argNameId);
        }

        public static ArgumentName NewUnbound()
        {
            return new ArgumentName();
        }
    }
}