using System;
using System.Collections.Generic;
using CNP.Parsing;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

    public abstract class Fold : Program
    {
        public Program Base { get; private set; }
        public Program Recursive { get; private set; }
        public override ISet<string> ArgumentNames => foldArgumentNames;
        protected static readonly HashSet<string> foldArgumentNames = new HashSet<string>(new string[] { "a0", "as", "b" });
        private int height;

        public Fold(Program recursiveCase, Program baseCase)
        {
            IsClosed = baseCase.IsClosed && recursiveCase.IsClosed;
            Base = baseCase;
            Recursive = recursiveCase;
            height = Math.Max(baseCase.Height, recursiveCase.Height) + 1;
        }

        public override int Height { get => height; }
        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Fold other))
                return false;
            return this.Recursive.Equals(other.Recursive) && this.Base.Equals(other.Base);
        }
        public override int GetHashCode()
        {
            return Recursive.GetHashCode() * 27 + Base.GetHashCode() * 31;
        }
    }


    
}
