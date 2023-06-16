using System;
using CNP.Language;
using CNP.Parsing;

namespace Tests.Execution
{
  public class ExecutionTestBase : TestBase
  {
    public ExecutionEnvironment BuildExecution(IProgram program, string argsStr, out GroundRelation args)
    {
      NameVarBindings nvb = new NameVarBindings();
      FreeFactory frees = new FreeFactory();
      args = Parser.ParseGroundRelation(argsStr, frees);
      ExecutionEnvironment env = new ExecutionEnvironment(program, nvb, frees);
      return env;
    }
  }
}

