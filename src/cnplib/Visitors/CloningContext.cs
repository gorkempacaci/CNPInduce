﻿using System;
using System.Collections.Generic;

using CNP.Helper;
using CNP.Language;
using CNP.Search;

namespace CNP
{
  public struct CloningContext
  {
    public (ObservedProgram, IProgram)? ObservationReplacement = null;
    public (Free, ITerm)? FreeReplacement = null;

    NameVarBindings oldNameBindings;
    FreeFactory oldFreeFactory;
    public readonly NameVarBindings NewNameBindings;
    public readonly FreeFactory NewFreeFactory;


    /// <summary>
    /// transitionary mapping from old namevar indices to the new one and a boolean that stores if this namevar has been deep-cloned. 
    /// </summary>
    Dictionary<int, (NameVar NewVar, bool DeepCloned)> nameVarIndicesMapping = new();

    public CloningContext(NameVarBindings oldBindings, FreeFactory oldFreeFactory)
    {
      this.oldNameBindings = oldBindings;
      this.oldFreeFactory = oldFreeFactory;
      this.NewNameBindings = new();
      this.NewFreeFactory = FreeFactory.CopyFrom(oldFreeFactory);
    }

    /// <summary>
    /// Asserts that every a in @as is different to every b in bs given their old indices. Useful for making sure names on the lh side and rh side of and (unique names) remain unique as they become ground.
    /// </summary>
    public void AssertDifferencesUsingOldIndices(NameVar[] @as, NameVar[] bs)
    {
      for(int ai=0;ai<@as.Length;ai++)
      {
        for(int bi=0; bi<bs.Length; bi++)
        {
          assertPreCloned(@as[ai].Index, out NameVar newA, out bool _);
          assertPreCloned(bs[bi].Index, out NameVar newB, out bool _);
          NewNameBindings.AssertDifferent(newA, newB);
        }
      }
    }

    // TERMS

    public ConstantTerm Clone(ConstantTerm ct)
    {
      return new ConstantTerm(ct);
    }
    public ITerm Clone(Free f)
    {
      if (FreeReplacement.HasValue && f.Index == FreeReplacement.Value.Item1.Index)
      {
        return FreeReplacement.Value.Item2;
      }
      else
      {
        return new Free(f.Index);
      }
    }
    public NilTerm Clone(NilTerm nt)
    {
      return new NilTerm();
    }
    public TermList Clone(TermList tl)
    {
      var head = tl.Head.Clone(this);
      var tail = tl.Tail.Clone(this);
      return new TermList(head, tail);
    }

    // META-TERMS

    private void assertPreCloned(int oldIndex, out NameVar newVar, out bool isAlreadyDeepCloned)
    {
      if (nameVarIndicesMapping.TryGetValue(oldIndex, out var record))
      {
        newVar = record.NewVar;
        isAlreadyDeepCloned = record.DeepCloned;
      }
      else
      {
        newVar = NewNameBindings.AddNameVar(oldNameBindings.GetNameForVar(oldIndex));
        nameVarIndicesMapping.Add(oldIndex, (newVar, false));
        isAlreadyDeepCloned = false;
      }
    }

    public NameVar Clone(NameVar oldNameVar)
    {
      /* For NameVars, there is pre-cloning, which is only assigning it a new index and name, and 
       *               there is deep-cloning, which means its differences have been copied too. for deep-cloning, pre-cloning of all its dependencies is a requirement so first we do that.
       */
      assertPreCloned(oldNameVar.Index, out var newVar, out var isAlreadyDeepCloned);

      if (!isAlreadyDeepCloned)
      {
        NewNameBindings.Names[newVar.Index] = oldNameBindings.Names[oldNameVar.Index];
        // do the deep cloning
        for (int j = 0; j < oldNameBindings.NumNameVars; j++)
        {
          if (oldNameBindings.IsDifferent(oldNameVar.Index, j)) // if different to old j
          {
            assertPreCloned(j, out var newDepVar, out var _);
            NewNameBindings.AssertDifferent(newVar, newDepVar);
          }
        }
        // mark the deep cloning
        nameVarIndicesMapping[oldNameVar.Index] = (newVar, true);
      }

      return newVar;
    }
    public ValenceVar Clone(ValenceVar vv)
    {
      var ins = new NameVar[vv.Ins.Length];
      for (int i = 0; i < ins.Length; i++)
        ins[i] = vv.Ins[i].Clone(this);
      var outs = new NameVar[vv.Outs.Length];
      for (int i = 0; i < outs.Length; i++)
        outs[i] = vv.Outs[i].Clone(this);
      return new ValenceVar(ins, outs);
    }
    public ProjectionMap Clone(ProjectionMap pm)
    {
      KeyValuePair<NameVar, NameVar>[] newPairs = new KeyValuePair<NameVar, NameVar>[pm.Map.Length];
      for (int i = 0; i < newPairs.Length; i++)
        newPairs[i] = new KeyValuePair<NameVar, NameVar>(pm.Map[i].Key.Clone(this), pm.Map[i].Value.Clone(this));
      return new ProjectionMap(newPairs);
    }
    public AlphaRelation Clone(AlphaRelation at)
    {
      var names = new NameVar[at.Names.Length];
      for (int i = 0; i < names.Length; i++)
        names[i] = at.Names[i].Clone(this);
      #region  Assert domain size
#if DEBUG
      if (at.ColumnsCount != names.Length)
        throw new ArgumentException("The AlphaRelation to be cloned does not have same number of names as there are domains in its tuples.");
#endif
      #endregion
      var tuples = new ITerm[at.TuplesCount][];
      for (int ti = 0; ti < at.TuplesCount; ti++)
      {
        tuples[ti] = new ITerm[at.ColumnsCount];
        for (int di = 0; di < at.ColumnsCount; di++)
          tuples[ti][di] = at.Tuples[ti][di].Clone(this);
      }
      return new AlphaRelation(names, tuples);
    }

    // ELEMENTARY

    public Cons Clone(Cons _)
    {
      return new Cons();
    }
    public Const Clone(Const src)
    {
      return new Const(src.ArgumentName.Clone(this), src.Value.Clone(this));
    }
    public Id Clone(Id _)
    {
      return new Id();
    }

    // OPERATORS

    public And Clone(And nd)
    {
      return new And(nd.LHOperand.Clone(this), nd.RHOperand.Clone(this));
    }
    public FoldL Clone(FoldL fl)
    {
      return new FoldL(fl.Recursive.Clone(this), fl.Base.Clone(this));
    }
    public FoldR Clone(FoldR fr)
    {
      return new FoldR(fr.Recursive.Clone(this), fr.Base.Clone(this));
    }
    public Proj Clone(Proj p)
    {
      return new Proj(p.Source.Clone(this), p.Projection.Clone(this));
    }

    // OBSERVATION

    public IProgram Clone(ObservedProgram obs)
    {
      if (ObservationReplacement != null && object.ReferenceEquals(ObservationReplacement.Value.Item1, obs))
      {
        return ObservationReplacement.Value.Item2.Clone(this);
      }
      else
      {
        var obss = obs.Observables.Clone(this);
        var val = obs.Valence.Clone(this);
        var remaining = obs.RemainingSearchDepth;
        var constr = obs.Constraints;
        return new ObservedProgram(obss, val, remaining, constr);
      }
    }
  }

  // CLONE at root(origObservation, newProgram)
  // CLONE at root(termreferencedictionary including names for binding matching programs)
  // MUTATION unifyinplace replace free with term

  // CONSISTENCY: Cloner does the modification of arguments and passes in prepared data into new objects. 

}