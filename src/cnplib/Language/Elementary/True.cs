using System;
using System.Collections.Generic;

namespace CNP.Language
{

    public class True : ElementaryProgram
    {
        protected static readonly ISet<string> EmptyArgumentList = new HashSet<string>();
        public override ISet<string> ArgumentNames => EmptyArgumentList;
        public override string ToString()
        {
            return "true";
        }
    }
}
