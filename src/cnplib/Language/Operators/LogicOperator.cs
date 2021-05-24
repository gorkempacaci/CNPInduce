using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    // public abstract class LogicOperator : Program
    // {
    //     private int height;
    //     public override ISet<string> ArgumentNames => calculatedArgumentNames;
    //     private readonly ISet<string> calculatedArgumentNames;
    //     public readonly IEnumerable<Program> Operands;
    //     protected LogicOperator(IEnumerable<Program> ps)
    //     {
    //         height = ps.Max(c => c.Height) + 1;
    //         IsClosed = ps.All(p => p.IsClosed);
    //         Operands = ps;
    //         calculatedArgumentNames = new HashSet<string>(Operands.SelectMany(o => o.ArgumentNames));
    //     }
    //     internal override ObservedProgram FindFirstHole()
    //     {
    //         foreach (Program p in Operands)
    //         {
    //             ObservedProgram hole = p.FindFirstHole();
    //             if (hole != null)
    //             {
    //                 return hole;
    //             }
    //         }
    //         return null;
    //     }
    //     public override int Height { get => height; }
    // }
    //
    // public class And : LogicOperator
    // {
    //     public And(IEnumerable<Program> ps) : base(ps) { }
    //
    //     public override Program CloneAndReplace(ObservedProgram oldComponent, Program newComponent, FreeDictionary plannedParenthood)
    //     {
    //         return new And(Operands.Select(p => p.CloneAndReplace(oldComponent, newComponent, plannedParenthood)));
    //     }
    // }
    //
    // public class Or : LogicOperator
    // {
    //     public Or(IEnumerable<Program> ps) : base(ps) { }
    //     public override Program CloneAndReplace(ObservedProgram oldComponent, Program newComponent, FreeDictionary plannedParenthood)
    //     {
    //         return new Or(Operands.Select(p => p.CloneAndReplace(oldComponent, newComponent, plannedParenthood)));
    //     }
    // }
}
