using System;
using CNP;
using CNP.Language;

namespace CNP.Language
{
  public abstract class ElementaryProgram : IProgram
  {
    public ElementaryProgram()
    {
    }

    public bool IsClosed { get => true; }
    public ObservedProgram FindLeftmostHole() => null;
    public int GetHeight() => 0;
    public int GetComplexityExponent() => 0;
    public string GetTreeQualifier() => "p";
    public void ReplaceFree(Free free, ITerm term) { }

    public string DebugValenceString { get; set; }
    public string DebugObservationString { get; set; }

    protected abstract bool RunElementary(BaseEnvironment env, GroundRelation args);

    public abstract string Accept(ICNPVisitor ps);
    public abstract IProgram Clone(CloningContext cc);
    public abstract string[] GetGroundNames(NameVarBindings nvb);
    

    public RunResult _Run(ExecutionEnvironment env, GroundRelation args)
    {
      if (RunElementary(env, args))
        return new RunResult.Success(env);
      else return new RunResult.Fail(env);
    }

  }
}

