using System;
using System.Collections.Generic;
using System.Collections;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    
    public class ProgramType
    {
        public NameModeMap Domains;

        public ProgramType(NameModeMap domains)
        {
            Domains = domains;
        }
        
        public virtual bool IsGround()
        {
            return Domains.IsGround();
        }

        public override bool Equals(object obj)
        {
            if (obj is ProgramType other)
                return Domains.Equals(other.Domains);
            else return false;
        }

        public override int GetHashCode()
        {
            return Domains.GetHashCode();
        }
    }

}
