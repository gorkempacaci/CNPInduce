using System;
using System.Linq;
using System.Collections.Generic;

namespace CNP.Language
{

  public record RunResult(bool IsSuccess, ExecutionEnvironment newEnvironment)
  {
    public record Success(ExecutionEnvironment newEnvironment) : RunResult(true, newEnvironment) { }
    public record Fail : RunResult
    {
      public Fail(ExecutionEnvironment oldEnvironment) : base(false, oldEnvironment)
      {
        oldEnvironment.Dirty = true;
      }
    }
  }

  public class ExecutionEnvironment : BaseEnvironment
  {
    /// <summary>
    /// Stack of arguments in the goal
    /// </summary>
    public Stack<GroundRelation> ArgumentStack { get; init; }

    public ExecutionEnvironment(IProgram program, NameVarBindings nvb, FreeFactory frees) : base(program, nvb, frees)
    {
      if (!program.IsClosed)
        throw new ArgumentException("An ExecutionEnvironment cannot have an open program (a program with observations in it).");
      this.ArgumentStack = new Stack<GroundRelation>();
    }

    public ExecutionEnvironment(IProgram program, NameVarBindings nvb, FreeFactory frees, Stack<GroundRelation> args) : this(program, nvb, frees)
    {
      this.ArgumentStack = args;
    }

    public override void ReplaceFree(Free free, ITerm term)
    {
      foreach(GroundRelation rel in ArgumentStack)
      {
        rel.ReplaceFreeInPlace(free, term);
      }
    }

    public RunResult Run(GroundRelation rel)
    {
      return Root.Run(this, rel); 
    }

    public ExecutionEnvironment Clone<TNames>()
    {
      if (Dirty)
        throw new InvalidOperationException("ProgramEnvironment is dirty.");
      CloningContext cc = new CloningContext(this.NameBindings, this.Frees);
      var newRoot = Root.Clone(cc);
      var oldArgsArr = this.ArgumentStack.ToArray();
      oldArgsArr.Reverse();
      var newArgs = new Stack<GroundRelation>(oldArgsArr);
      var newEnv = new ExecutionEnvironment(newRoot, cc.NewNameBindings, cc.NewFreeFactory, newArgs);
      return newEnv;
    }

    public static bool NegativeExamplesFailAsTheyShould(ProgramEnvironment synEnv, GroundRelation negExamples)
    {
      ExecutionEnvironment exenv = synEnv.ToExecutionEnvironment();
      foreach (ITerm[] tuple in negExamples.Tuples)
      {
        GroundRelation relPerTuple = new GroundRelation(negExamples.Names, new[] { tuple });
        if (exenv.Run(relPerTuple) is RunResult.Success)
        {
          return false;
        }
      }
      return true;
    }
  }
}

