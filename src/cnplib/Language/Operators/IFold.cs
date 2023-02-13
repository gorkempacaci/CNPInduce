﻿using System;
namespace CNP.Language

    bool IProgram.IsClosed => Recursive.IsClosed && Base.IsClosed;
    {
      var rec = Recursive.FindRootmostHole(calleesDistanceToRoot + 1);
      var bas = Base.FindRootmostHole(calleesDistanceToRoot + 1);
      if (rec.Item2 <= bas.Item2) return rec; else return bas;
    }
        return Array.Empty<ProgramEnvironment>();
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
        }