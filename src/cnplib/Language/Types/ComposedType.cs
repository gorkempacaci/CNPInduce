namespace CNP.Language
{
  public abstract class ComposedType : ProgramType
  {
    protected ComposedType(Valence doms) : base(doms)
    {
    }
    // these should be implemented by the subclasses, the inherited implementation would be troublesome.
    public abstract override bool IsGround();
    public abstract override bool Equals(object obj);
    public abstract override int GetHashCode();
  }
}