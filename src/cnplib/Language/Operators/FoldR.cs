using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Threading.Tasks;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{
  public class FoldR : Fold
  {
    public FoldR(IProgram recursiveCase) : base(recursiveCase)
    {
      
    }

    public override int GetHashCode()
    {
      return 43;
    }

    public override bool Equals(object obj)
    {
      return obj is FoldR r && r.Recursive.Equals(this.Recursive);
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
      return "foldr(" + Recursive.GetTreeQualifier() + ")";
    }

    protected override Fold.UnFold GetUnfolder() => UnFoldR;


    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      return Fold.CreateAtFirstHole(env, factoryFoldR, UnFoldR);
    }

    static Fold.CreateFold factoryFoldR = (rec) => new FoldR(rec);


    public static bool UnFoldR(RelationBase foldRel, (short b0, short @as, short b) nameIndices, FreeFactory freeFac, out ITerm[][] pTuples)
    {
      List<ITerm[]> pTuplesList = new();
      for (int ri = 0; ri < foldRel.TuplesCount; ri++)
      {
        List<ITerm[]> pTuplesPerFoldTuple = new List<ITerm[]>();
        ITerm seed = foldRel.Tuples[ri][nameIndices.b0];
        ITerm list = foldRel.Tuples[ri][nameIndices.@as];
        ITerm result = foldRel.Tuples[ri][nameIndices.b];
        while (list is TermList termList)
        {
          ITerm head = termList.Head;
          ITerm tail = termList.Tail;
          if (tail is NilTerm)
          {
            pTuplesPerFoldTuple.Add(new ITerm[] { head, seed, result });
          }
          else
          {
            var acc = freeFac.NewFree();
            pTuplesPerFoldTuple.Add(new ITerm[] { head, acc, result });
            result = acc;
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
        pTuplesPerFoldTuple.Reverse();
        pTuplesList.AddRange(pTuplesPerFoldTuple);
      }
      if (pTuplesList.Any())
      {
        pTuples = pTuplesList.ToArray();
        return true;
      } else
      {
        pTuples = null;
        return false;
      }
    }
  }
}