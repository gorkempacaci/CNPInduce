using System;

namespace CNP.Language
{
  /// <summary>
  /// Like alpharelation but names are ground, so simply strings.
  /// </summary>
  public class GroundRelation : RelationBase
  {
    public readonly string[] Names;

    public bool IsEmpty => Names.Length == 0 || Tuples.Length == 0;

    public GroundRelation(string[] names, ITerm[][] tups) : base(tups)
    {
      this.Names = names;
#if DEBUG
      if (Names.Length != ColumnsCount)
      {
        throw new ArgumentException("Tuples column count and the number of names don't match.");
      }
#endif
    }

    public override GroundRelation Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public override GroundRelation GetCroppedByIndices(short[] indices)
    {
      var (newNames, tups) = _getCroppedByIndices(this.Names, indices);
      return new GroundRelation(newNames, tups);
    }

    public override string[] GetGroundNames(NameVarBindings nvb)
    {
      return this.Names;
    }

    public static GroundRelation Empty = new GroundRelation(new string[] { }, new ITerm[][] { });
  }

}
