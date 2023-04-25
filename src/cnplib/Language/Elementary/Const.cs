using System;
using System.Collections.Generic;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{
  //TODO: if straight semantics are implemented, const(a, 5) should unify with proj(a, id), or any tuple that has {a:5}.
  public class Const : IProgram
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
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment envOrig)
    {
      ObservedProgram obsReadonly = envOrig.Root.FindHole();
      for(int oi=0; oi< obsReadonly.Observations.Length; oi++)
      {
        var cloneEnv = envOrig.Clone(); // use a different penv for each try
        var obsprog = cloneEnv.Root.FindHole();
        var obs = obsprog.Observations[oi];
        if (obs.Examples.Names.Length != 1)
          continue;
        var tuple0 = obs.Examples.Tuples[0];
        var debugInfo = obs.GetDebugInformation(cloneEnv);
        for (int ri = 1; ri < obs.Examples.TuplesCount; ri++)
        {
          var tuplen = obs.Examples.Tuples[ri];
          if (!cloneEnv.UnifyInPlace(tuple0, tuplen)) // unify [0, 1] and [1, 0]
            return Array.Empty<ProgramEnvironment>();
        }
        var name = obs.Examples.Names[0];
        if (tuple0[0].IsGround())                                   
        { // found the constant through decomposition / initial example
          var constProg = new Const(name, tuple0[0]);
          (constProg as IProgram).SetDebugInformation(debugInfo);
          var newEnv = cloneEnv.Clone((obsprog, constProg));
          return new ProgramEnvironment[] { newEnv };
        }
        else
        { // suggest common constants
          ITerm[] commonConstants = new ITerm[] { new NilTerm(),
                                                  new ConstantTerm(0),
                                                  new ConstantTerm(1),
                                                  new ConstantTerm(-1) };
          List<ProgramEnvironment> ccEnvironments = new();
          foreach (ITerm cc in commonConstants)
          {
            var newEnv = envOrig.Clone();
            var newObsProg = newEnv.Root.FindHole();
            var newObs = newObsProg.Observations[oi];
            if (newObs.Examples.Tuples.All(tup => newEnv.UnifyInPlace(tup, new ITerm[] { cc })))
            {
              var constProg = new Const(newObs.Examples.Names[0], cc);
              (constProg as IProgram).SetDebugInformation(debugInfo);
              var clonedEnv = newEnv.Clone((newObsProg, constProg));
              ccEnvironments.Add(clonedEnv);
            }
          }
          return ccEnvironments;
        }
      }
      return Array.Empty<ProgramEnvironment>();
    }

  }
}
