using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  //TODO: if straight semantics are implemented, const(a, 5) should unify with proj(a, id), or any tuple that has {a:5}.
  public readonly struct Const : IProgram
  {
    public readonly NameVar ArgumentName;
    public readonly ITerm Value;

    public Const(NameVar argName, ITerm groundTerm)
    {
      if (!groundTerm.IsGround())
      {
        throw new Exception("The term for the Const can only be a ground term.");
      }

      ArgumentName = argName;
      Value = groundTerm;
    }

    public bool IsClosed => true;

    public override int GetHashCode()
    {
      return this.Value.GetHashCode();
    }
    public override bool Equals(object obj)
    {
      if (obj is null || !(obj is Const constProgram))
        return false;
      bool sameName = constProgram.ArgumentName.Equals(this.ArgumentName);
      bool sameValue = constProgram.Value.Equals(this.Value);
      return sameName && sameValue;
    }


    public void ReplaceFree(Free _, ITerm __) { }

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public ObservedProgram FindLeftmostHole() => null;

    public (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot = 0) => (null, int.MaxValue);

    public int GetHeight() => 0;

    public string GetTreeQualifier() => "p";

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      ObservedProgram obsOriginal = env.Root.FindHole();
      if (obsOriginal.Observables.Names.Length != 1)
        return Array.Empty<ProgramEnvironment>();
      if (obsOriginal.Observables.TuplesCount == 0)
        throw new ArgumentException("Const: Observation is empty.");
      var tuple0 = obsOriginal.Observables.Tuples[0];
      for (int ri = 1; ri < obsOriginal.Observables.TuplesCount; ri++)
      {
        var tuplen = obsOriginal.Observables.Tuples[ri];
        if (!AlphaRelation.UnifyInPlace(tuple0, env, tuplen)) // unify [0, 1] and [1, 0]
          return Array.Empty<ProgramEnvironment>();
      }
      if (!tuple0[0].IsGround())
      {
        return Array.Empty<ProgramEnvironment>();
      }
      var name = obsOriginal.Observables.Names[0];
      if (env.NameBindings.GetNameForVar(name)=="u")
      {
        ;
      }
      var constProg = new Const(name, tuple0[0]);
      var newEnv = env.Clone((obsOriginal, constProg));
      return new ProgramEnvironment[] { newEnv };
    }

  }
}
