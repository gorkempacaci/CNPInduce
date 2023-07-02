using System;
using System.Collections.Generic;
using CNP.Language;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{
  public class FoldL : Fold
  {
    public FoldL(IProgram recursiveCase) : base(recursiveCase)
    {
    }

    public override int GetHashCode()
    {
      return 41;
    }

    public override bool Equals(object obj)
    {
      return obj is FoldL l && l.Recursive.Equals(this.Recursive);
    }

    public override string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public override IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public override string GetTreeQualifier()
    {
      return "foldl(" + Recursive.GetTreeQualifier() + ")";
    }

    protected override UnFold GetUnfolder() => UnFoldL;

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      return Fold.CreateAtFirstHole(env, factoryFoldL, UnFoldL);
    }

    static Fold.CreateFold factoryFoldL = (rec) => new FoldL(rec);

    public static bool UnFoldL(RelationBase foldRel, (short b0, short @as, short b) nameIndices, BaseEnvironment env, out ITerm[][] pTuples)
    {
      List<ITerm[]> pTuplesList = new();
      pTuples = null;
      for (int ri = 0; ri < foldRel.TuplesCount; ri++)
      {
        ITerm bVal = foldRel.Tuples[ri][nameIndices.b0];
        ITerm list = foldRel.Tuples[ri][nameIndices.@as];
        ITerm result = foldRel.Tuples[ri][nameIndices.b];
        if (list is NilTerm)
        {
          ITerm[] site = foldRel.Tuples[ri];
          ITerm[] unifier = new ITerm[] { null, null, null };
          unifier[nameIndices.b0] = result;
          unifier[nameIndices.b] = bVal;
          if (!env.UnifyInPlace(site, unifier))
          {
            pTuples = null;
            return false;
          }
        }
        else if (list is TermList)
        {
          while (list is TermList termList)
          {
            ITerm head = termList.Head;
            ITerm tail = termList.Tail;
            if (tail is not NilTerm)
            {
              Free carry = env.Frees.NewFree();
              pTuplesList.Add(new ITerm[] { head, bVal, carry }); // a, b, ab
              bVal = carry;
            }
            else // tail is nil
            {
              pTuplesList.Add(new ITerm[] { head, bVal, result }); // a, b, ab
            }
            list = tail;
          }
        }
        else // list is not [] or [X|L]
        {
          pTuples = null;
          return false;
        }
      }
      pTuples = pTuplesList.ToArray();
      return pTuples.Any();
    }
  }
}
