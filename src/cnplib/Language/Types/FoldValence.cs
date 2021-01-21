namespace CNP.Language
{
  public class FoldValence : FunctionalValence
  {
    public readonly Valence RecursiveComponent;
    public readonly Valence BaseComponent;

    public FoldValence(Valence recDoms, Valence baseDoms, Valence doms) : base(doms)
    {
      RecursiveComponent = recDoms;
      BaseComponent = baseDoms;
    }

    public override bool IsGround()
    {
      return IsGround() && RecursiveComponent.IsGround() && BaseComponent.IsGround();
    }

    public override bool Equals(object obj)
    {
      if (!(obj is FoldValence other))
        return false;
      return base.Equals(other) &&
             RecursiveComponent.Equals(other.RecursiveComponent) &&
             BaseComponent.Equals(other.BaseComponent);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override string ToString()
    {
      return RecursiveComponent.ToString() + "->" + BaseComponent.ToString() + "->" + base.ToString();
    }
  }

}