using System;using System.Collections.Generic;using System.Reflection;using CNP.Parsing;using CNP.Helper;using System.Linq;namespace CNP.Language{  public interface IFold : IProgram  {    public IProgram Recursive { get; }    bool IProgram.IsClosed => Recursive.IsClosed;    int IProgram.GetHeight()    {      return Recursive.GetHeight() + 1;    }    delegate IFold CreateFold(IProgram recursive);    delegate bool UnFold(ProgramEnvironment env, AlphaRelation foldRel, (short b0, short @as, short b) nameIndices, FreeFactory freeFac, out ITerm[][] pRelation);    protected static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment oldEnv, FoldValenceSeries foldValences, CreateFold newFold, UnFold unfolder)    {      ObservedProgram origObservation = oldEnv.Root.FindHole();      if (origObservation.RemainingSearchDepth<2)        return Array.Empty<ProgramEnvironment>();
      var newPrograms = new List<ProgramEnvironment>();      for (int oi = 0; oi < origObservation.Observations.Length; oi++)
      {
        if (origObservation.Observations[oi].Examples.TuplesCount == 0)
          return Array.Empty<ProgramEnvironment>();
        foldValences.GroundingAlternatives(origObservation.Observations[oi].Valence, oldEnv.NameBindings, out var groundingAlternatives);
        foreach (var alt in groundingAlternatives)
        {
          var newEnv = oldEnv.Clone();
          var newObs = newEnv.Root.FindHole();
          var debugInfo = newObs.Observations[oi].GetDebugInformation(newEnv);
          if (newEnv.NameBindings.TryBindingAllNamesToGround(newObs.Observations[oi].Valence, (ins: alt.ins, outs: alt.outs)))
          {
            string[] groundNames = newEnv.NameBindings.GetNamesForVars(newObs.Observations[oi].Examples.Names);
            short b0 = (short)Array.IndexOf(groundNames, "b0");
            short @as = (short)Array.IndexOf(groundNames, "as");
            short b = (short)Array.IndexOf(groundNames, "b");

            var nameIndices = (b0: b0, @as: @as, b: b);
            if (unfolder(newEnv, newObs.Observations[oi].Examples, nameIndices, newEnv.Frees, out var pTuples))
            {
              // build p-observation
              NameVar[] pNames = new[] { newEnv.NameBindings.AddNameVar(foldValences.RecursiveCaseNames[0]), newEnv.NameBindings.AddNameVar(foldValences.RecursiveCaseNames[1]), newEnv.NameBindings.AddNameVar(foldValences.RecursiveCaseNames[2]) };
              AlphaRelation pRelation = new(pNames, pTuples);
              ValenceVar pVal = ValenceVar.FromModeIndices(pNames, alt.rec);
              Observation obs = new Observation(pRelation, pVal);
              ObservedProgram pObs = new ObservedProgram(new[] { obs }, newObs.RemainingSearchDepth - 1, newObs.RemainingUnboundArguments, ObservedProgram.Constraint.None);
              // build fold
              IFold fld = newFold(pObs);
              fld.SetDebugInformation((debugInfo.valenceString, debugInfo.observationString + $" with order (b0={b0}, as={@as}, b={b})"));
              var outEnv = newEnv.Clone((newObs, fld));
              newPrograms.Add(outEnv);
            }
          }
        }      }      return newPrograms;    }  }  }