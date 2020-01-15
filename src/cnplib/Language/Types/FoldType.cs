namespace CNP.Language
{
    public abstract class FoldType : ComposedType
    {
        public readonly NameModeMap RecursiveComponentDomains;
        public readonly NameModeMap BaseComponentDomains;
        public abstract string ANameInitial { get; }
        public abstract string ANameList { get; } 
        public abstract string ANameFolded { get; }
        public FoldType(NameModeMap recDoms, NameModeMap baseDoms, NameModeMap doms) : base(doms)
        {
            RecursiveComponentDomains = recDoms;
            BaseComponentDomains = baseDoms;
        }

        public override bool IsGround()
        {
            return base.IsGround() && RecursiveComponentDomains.IsGround() && BaseComponentDomains.IsGround();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FoldType other))
                return false;
            return Domains.Equals(other.Domains) &&
                   RecursiveComponentDomains.Equals(other.RecursiveComponentDomains) &&
                   BaseComponentDomains.Equals(other.BaseComponentDomains);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class FoldRType : FoldType
    {
        public override string ANameInitial { get; } = "b0";
        public override string ANameList { get; } = "as";
        public override string ANameFolded { get; } = "b";

        public FoldRType(NameModeMap recDoms, NameModeMap baseDoms, NameModeMap doms) : base(recDoms, baseDoms, doms)
        {
        }
    }
    
    public class FoldLType : FoldType
    {
        public override string ANameInitial { get; } = "a0";
        public override string ANameList { get; } = "bs";
        public override string ANameFolded { get; } = "a";
        
        public FoldLType(NameModeMap recDoms, NameModeMap baseDoms, NameModeMap doms) : base(recDoms, baseDoms, doms)
        {
        }
    }
}