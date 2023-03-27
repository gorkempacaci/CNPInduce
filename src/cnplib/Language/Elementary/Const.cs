using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  //TODO: if straight semantics are implemented, const(a, 5) should unify with proj(a, id), or any tuple that has {a:5}.
  public struct Const : IProgram
  {
    public readonly NameVar ArgumentName;
    public readonly ITerm Value;

    /// <summary>
    /// The valence that lead to this program.
    /// </summary>
    public string DebugValenceString { get; set; } = "";
    /// <summary>
    /// The observations that lead to this program.
    /// </summary>
    public string DebugObservationString { get; set; } = "";

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

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public ObservedProgram FindLeftmostHole() => null;

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
        if (!env.UnifyInPlace(tuple0, tuplen)) // unify [0, 1] and [1, 0]
          return Array.Empty<ProgramEnvironment>();
      }
      var name = obsOriginal.Observables.Names[0];
      if (tuple0[0].IsGround())
      { // found the constant through decomposition / initial example
        var constProg = new Const(name, tuple0[0]);
        (constProg as IProgram).SaveDebugInformationString(env, obsOriginal);
        var newEnv = env.Clone((obsOriginal, constProg));
        return new ProgramEnvironment[] { newEnv };
      }
      else
      { // suggest common constants
        ITerm[] commonConstants = new ITerm[] { new NilTerm(),
                                                new ConstantTerm(0),
                                                new ConstantTerm(1),
                                                new ConstantTerm(-1)
        };
        List<ProgramEnvironment> ccEnvironments = new();
        foreach(ITerm cc in commonConstants)
        {
          var newEnv = env.Clone();
          var newObs = newEnv.Root.FindHole();
          if (newEnv.UnifyInPlace(newObs.Observables.Tuples[0], new ITerm[] { cc }))
          {
            var constProg = new Const(newObs.Observables.Names[0], cc);
            (constProg as IProgram).SaveDebugInformationString(newEnv, newObs);
            var clonedEnv = newEnv.Clone((newObs, constProg));
            ccEnvironments.Add(clonedEnv);
          }
        }
        return ccEnvironments;
      }


    }

  }
}
