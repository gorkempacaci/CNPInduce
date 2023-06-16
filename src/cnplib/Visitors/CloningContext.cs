using System;
using System.Collections.Generic;
using System.Linq;
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
    //public void AssertDifferencesUsingOldIndices(NameVar[] @as, NameVar[] bs)
    //{
    //  for (int ai = 0; ai < @as.Length; ai++)
    //  {
    //    for (int bi = 0; bi < bs.Length; bi++)
    //    {
    //      assertPreCloned(@as[ai].Index, out NameVar newA, out bool _);
    //      assertPreCloned(bs[bi].Index, out NameVar newB, out bool _);
    //      NewNameBindings.AssertDifferent(newA, newB);
    //    }
    //  }
    //}

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

    private ITerm[][] cloneTuples(RelationBase rel)
    {
      #region  Assert domain size
#if DEBUG
      if (rel.Tuples.Length > 0)
        if (rel.ColumnsCount != rel.Tuples[0].Length)
          throw new ArgumentException("The AlphaRelation to be cloned does not have same number of columns as there are domains in its tuples.");
#endif
      #endregion
      var tuples = new ITerm[rel.TuplesCount][];
      for (int ti = 0; ti < rel.TuplesCount; ti++)
      {
        tuples[ti] = new ITerm[rel.ColumnsCount];
        for (int di = 0; di < rel.ColumnsCount; di++)
          tuples[ti][di] = rel.Tuples[ti][di].Clone(this);
      }
      return tuples;
    }

    public AlphaRelation Clone(AlphaRelation at)
    {
      var names = new NameVar[at.Names.Length];
      for (int i = 0; i < names.Length; i++)
        names[i] = at.Names[i].Clone(this);
      return new AlphaRelation(names, cloneTuples(at));
    }

    public GroundRelation Clone(GroundRelation rel)
    {
      var names = rel.Names.Clone() as string[];
      return new GroundRelation(names, cloneTuples(rel));
    }

    // ELEMENTARY

    public Cons Clone(in Cons cns)
    {
      var cns2 = new Cons();
      CopyDebugInformation(cns, cns2);
      return cns2;
    }
    public Const Clone(in Const cnst)
    {
      var cnst2 = new Const(cnst.ArgumentName.Clone(this), cnst.Value.Clone(this));
      CopyDebugInformation(cnst, cnst2);
      return cnst2;
    }
    public Id Clone(in Id i)
    {
      var i2 = new Id();
      CopyDebugInformation(i, i2);
      return i2;
    }

    // OPERATORS

    public And Clone(in And nd)
    {
      var and2 = new And(nd.LHOperand.Clone(this), nd.RHOperand.Clone(this));
      CopyDebugInformation(nd, and2);
      return and2;
    }
    public FoldL Clone(in FoldL fl)
    {
      var fold2 = new FoldL(fl.Recursive.Clone(this));
      CopyDebugInformation(fl, fold2);
      return fold2;
    }
    public FoldR Clone(in FoldR fr)
    {
      var fold2 = new FoldR(fr.Recursive.Clone(this));
      CopyDebugInformation(fr, fold2);
      return fold2;
    }
    public Proj Clone(in Proj p)
    {
      var proj2 = new Proj(p.Source.Clone(this), p.Projection.Clone(this));
      CopyDebugInformation(p, proj2);
      return proj2;
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
        var thiz = this; // OMG
        var obss = obs.Observations.Select(o => o.Clone(thiz)).ToArray();
        return new ObservedProgram(obss,obs.RemainingSearchDepth, obs.RemainingUnboundArguments, obs.Constraints);
      }
    }

    public Observation Clone(Observation o)
    {
      var examples = o.Examples.Clone(this);
      var val = o.Valence.Clone(this);
      return new Observation(examples, val);
    }

    // MISC

    private static void CopyDebugInformation(in IProgram oldProgram, IProgram newProgram)
    {
      newProgram.DebugObservationString = oldProgram.DebugObservationString;
      newProgram.DebugValenceString = oldProgram.DebugValenceString;
    }
  }

  // CLONE at root(origObservation, newProgram)
  // CLONE at root(termreferencedictionary including names for binding matching programs)
  // MUTATION unifyinplace replace free with term

  // CONSISTENCY: Cloner does the modification of arguments and passes in prepared data into new objects. 

}
