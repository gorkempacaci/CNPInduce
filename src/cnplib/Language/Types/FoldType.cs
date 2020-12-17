namespace CNP.Language
{
  public class FoldType : ComposedType
  {
    public readonly Valence RecursiveComponentDomains;
    public readonly Valence BaseComponentDomains;

    public FoldType(Valence recDoms, Valence baseDoms, Valence doms) : base(doms)
    {
      RecursiveComponentDomains = recDoms;
      BaseComponentDomains = baseDoms;
    }

    public override bool IsGround()
    {
      return Domains.IsGround() && RecursiveComponentDomains.IsGround() && BaseComponentDomains.IsGround();
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
      return Domains.GetHashCode();
    }

    public override string ToString()
    {
      return RecursiveComponentDomains.ToString() + "->" + BaseComponentDomains.ToString() + "->" + Domains.ToString();
    }
  }

}