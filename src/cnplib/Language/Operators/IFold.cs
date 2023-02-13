using System;using System.Collections.Generic;using System.Reflection;using CNP.Parsing;using CNP.Helper;using System.Linq;
namespace CNP.Language{  public interface IFold : IProgram  {    public IProgram Base { get; }    public IProgram Recursive { get; }

    bool IProgram.IsClosed => Recursive.IsClosed && Base.IsClosed;    ObservedProgram IProgram.FindLeftmostHole()    {      return Recursive.FindLeftmostHole() ?? Base.FindLeftmostHole();    }    (ObservedProgram, int) IProgram.FindRootmostHole(int calleesDistanceToRoot)
    {
      var rec = Recursive.FindRootmostHole(calleesDistanceToRoot + 1);
      var bas = Base.FindRootmostHole(calleesDistanceToRoot + 1);
      if (rec.Item2 <= bas.Item2) return rec; else return bas;
    }    int IProgram.GetHeight()    {      return Math.Max(Recursive.GetHeight(), Base.GetHeight());    }    delegate IFold CreateFold(IProgram recursive, IProgram bas);    delegate bool UnFold(AlphaRelation foldRel, (short b0, short @as, short b) nameIndices, FreeFactory freeFac, out ITerm[][] pRelation, out ITerm[][] qRelation);    protected static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment oldEnv, GroundValence.FoldValenceSeries foldValences, CreateFold newFold, UnFold unfolder)    {      ObservedProgram origObservation = oldEnv.Root.FindHole();      if (origObservation.RemainingSearchDepth<2)
        return Array.Empty<ProgramEnvironment>();      foldValences.GroundingAlternatives(origObservation.Valence, oldEnv.NameBindings, out var groundingAlternatives);      if (!groundingAlternatives.Any())        return Array.Empty<ProgramEnvironment>();      var newPrograms = new List<ProgramEnvironment>();      foreach (var alt in groundingAlternatives)      {        var newEnv = oldEnv.Clone();        var newObs= newEnv.Root.FindHole();
        if (newEnv.NameBindings.TryBindingAllNamesToGround(newObs.Valence, (ins:alt.ins, outs:alt.outs)))
        {
          string[] groundNames = newEnv.NameBindings.GetNamesForVars(newObs.Observables.Names);
          short b0 = (short)Array.IndexOf(groundNames, "b0");
          short @as = (short)Array.IndexOf(groundNames, "as");
          short b = (short)Array.IndexOf(groundNames, "b");
          var nameIndices = (b0: b0, @as: @as, b: b);
          if (unfolder(newObs.Observables, nameIndices, newEnv.Frees, out var pTuples, out var qTuples))
          {
            // build p-observation
            NameVar[] pNames = new[] { newEnv.NameBindings.AddNameVar(foldValences.RecursiveCaseNames[0]), newEnv.NameBindings.AddNameVar(foldValences.RecursiveCaseNames[1]), newEnv.NameBindings.AddNameVar(foldValences.RecursiveCaseNames[2]) };
            AlphaRelation pRelation = new(pNames, pTuples);
            var recInsNames = alt.rec.Ins.Select(i => pNames[i]).ToArray();
            var recOutsNames = alt.rec.Outs.Select(i => pNames[i]).ToArray();
            ValenceVar pVal = new ValenceVar(recInsNames, recOutsNames);
            ObservedProgram pObs = new ObservedProgram(pRelation, pVal, newObs.RemainingSearchDepth - 1, ObservedProgram.Constraint.None);
            // build q-observation
            NameVar[] qNames = new[] { newEnv.NameBindings.AddNameVar(foldValences.BaseCaseNames[0]), newEnv.NameBindings.AddNameVar(foldValences.BaseCaseNames[1]) };
            AlphaRelation qRelation = new(qNames, qTuples);
            var basInsNames = alt.bas.Ins.Select(i => qRelation.Names[i]).ToArray();
            var basOutsNames = alt.bas.Outs.Select(i => qRelation.Names[i]).ToArray();
            ValenceVar qVal = new ValenceVar(basInsNames, basOutsNames);
            ObservedProgram qObs = new ObservedProgram(qRelation, qVal, newObs.RemainingSearchDepth - 1, ObservedProgram.Constraint.None);
            // build fold
            IFold fld = newFold(pObs, qObs);
            var outEnv = newEnv.Clone((newObs, fld));
            newPrograms.Add(outEnv);
          }
        }      }      return newPrograms;    }  }  }