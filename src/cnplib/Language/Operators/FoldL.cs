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

    public static bool UnFoldL(RelationBase foldRel, (short b0, short @as, short b) nameIndices, FreeFactory freeFac, out ITerm[][] pTuples)
    {
      List<ITerm[]> pTuplesList = new();
      for (int ri = 0; ri < foldRel.TuplesCount; ri++)
      {
        ITerm seed = foldRel.Tuples[ri][nameIndices.b0];
        ITerm list = foldRel.Tuples[ri][nameIndices.@as];
        ITerm result = foldRel.Tuples[ri][nameIndices.b];
        while (list is TermList termList)
        {
          ITerm head = termList.Head;
          ITerm tail = termList.Tail;
          if (tail is NilTerm)
          {
            pTuplesList.Add(new ITerm[] { head, seed, result }); // a, b, ab
          }
          else
          {
            Free carry = freeFac.NewFree();
            pTuplesList.Add(new ITerm[] { head, seed, carry }); // a, b, ab
            seed = carry;
          }
          list = tail;
        }
        if (list is NilTerm)
        {
          ITerm[] site = foldRel.Tuples[ri];
          ITerm[] unifier = new ITerm[] { null, null, null };
          unifier[nameIndices.b0] = site[nameIndices.b];
        }
        else
        {
          pTuples = null;
          return false;
        }
      }
      if (pTuplesList.Any())
      {
        pTuples = pTuplesList.ToArray();
        return true;
      }
      else
      {
        pTuples = null;
        return false;
      }
    }
  }
}
