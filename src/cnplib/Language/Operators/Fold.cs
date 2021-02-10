﻿using System;
using System.Collections.Generic;
using System.Reflection;
using CNP.Parsing;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using Helper;

namespace CNP.Language
{

  public abstract class Fold : Program
  {
    public Program Base { get; private set; }
    public Program Recursive { get; private set; }

    public Fold(Program recursiveCase, Program baseCase) : base(baseCase.IsClosed && recursiveCase.IsClosed)
    {
      Base = baseCase;
      Recursive = recursiveCase;
    }

    public override bool Equals(object obj)
    {
      if (obj is null || !(obj is Fold other))
        return false;
      return this.Recursive.Equals(other.Recursive) && this.Base.Equals(other.Base);
    }
    internal sealed override ObservedProgram FindFirstHole()
    {
      return Recursive.FindFirstHole() ?? Base.FindFirstHole();
    }
    public override int GetHashCode()
    {
      return Recursive.GetHashCode() * 27 + Base.GetHashCode() * 31;
    }

    public override int GetHeight()
    {
      return Math.Max(Recursive.GetHeight(), Base.GetHeight());
    }

    public sealed override void SetAllRootsTo(Program newRoot)
    {
      Root = newRoot;
      Recursive.SetAllRootsTo(newRoot);
      Base.SetAllRootsTo(newRoot);
    }

    protected static IEnumerable<Program> CreateAtFirstHole(Program rootProgram, TypeStore<FoldValence> valences, Func<Program, Program, Fold> foldFactoryMethod, Func<Term, Term, Term, List<AlphaTuple>, NameVarDictionary, List<AlphaTuple>, NameVarDictionary, bool> unfold)
    {
      ObservedProgram origObservation = rootProgram.FindFirstHole();
      if (origObservation.DTL == 0)
        return Iterators.Empty<Program>();
      IEnumerable<FoldValence> foldValences = valences.FindCompatibleTypes(origObservation.Valence);
      if (!foldValences.Any())
        return Iterators.Empty<Program>();

      var newRootPrograms = new List<Program>();
      foreach (var valFPQ in foldValences)
      {
        var combs = origObservation.Valence.PossibleGroundings(valFPQ);
        foreach(var uni in combs)
        {
          var cloneProgram = rootProgram.Clone(uni);
          var obs = cloneProgram.FindFirstHole();
          // decompose into p and q examples
          List<AlphaTuple> pExamples = new(), qExamples = new();
          NameVarDictionary pNames = new(valFPQ.RecursiveComponent.Keys); // initialize with existing names
          NameVarDictionary qNames = new(valFPQ.BaseComponent.Keys);
          foreach (AlphaTuple at in obs.Observables)
            if (false == unfold(at["b0"], at["as"], at["b"], pExamples, pNames, qExamples, qNames))
              return Iterators.Empty<Program>(); // if even one of the observations doesn't unfold, this is not a fold.
          var pp = new TermReferenceDictionary(); // a fresh cloning map from old root to new context
          var pEx = pExamples.Select(e => e.Clone(pp)); // clone examples
          var qEx = qExamples.Select(e => e.Clone(pp));
          var pVal = valFPQ.RecursiveComponent.Clone(pp); // clone valences (maybe arg names aren't ground)
          var qVal = valFPQ.BaseComponent.Clone(pp);
          var pObs = new ObservedProgram(pEx, pVal, obs.DTL - 1);
          var qObs = new ObservedProgram(qEx, qVal, obs.DTL - 1);
          var foldProgram = foldFactoryMethod(pObs, qObs);
          var p = cloneProgram.CloneAndReplaceObservation(obs, foldProgram, pp); // use same cloning scheme for cloning the root program
          newRootPrograms.Add(p);
        }
      }
      return newRootPrograms;
    }
  }



}