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

    /*
     
    foldr(cons)([3,4], [1,2], R).

    // procedurally:

    cons(2, [3,4], X1),
    cons(1, X1, R)

     
     */
    public static bool UnFoldR(RelationBase foldRel, (short b0, short @as, short b) nameIndices, BaseEnvironment env, out ITerm[][] pTuples)
    {
      List<ITerm[]> pTuplesList = new();
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
        } else if (list is TermList termList)
        {
          List<ITerm> terms = new(termList.ToEnumerable());
          terms.Reverse(); // in the order foldr executes
          for(int i=0; i<terms.Count; i++)
          {
            if (i<terms.Count-1) // not last executed
            {
              var acc = env.Frees.NewFree();
              pTuplesList.Add(new ITerm[] { terms[i], bVal, acc });
              bVal = acc;
            } else // last executed
            {
              pTuplesList.Add(new ITerm[] { terms[i], bVal, result });
            }
          }
        } else // list is not [] or [X|L]
        {
          pTuples = null;
          return false;
        }      }
      pTuples = pTuplesList.ToArray();
      return pTuples.Any();
    }
  }
}