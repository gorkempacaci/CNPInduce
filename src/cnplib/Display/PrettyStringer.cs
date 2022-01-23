using System;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using CNP.Language;

namespace CNP.Display
{
  /// <summary>
  /// Produces a string representation (replacing ToString() in most cases) for terms, meta-terms, and program terms.
  /// </summary>
  public class PrettyStringer
  {
    int freeCounter = 0;
    ReferenceDictionary<Free, int> freeIndices = new();
    int nameVarCounter = 0;
    ReferenceDictionary<NameVar, int> nameVarIndices = new();


    public PrettyStringer()
    {
    }

    // TERMS

    public string PrettyString(ConstantTerm ct)
    {
      if (ct.Value is string)
        return "'" + ct.Value + "'";
      else return (ct.Value).ToString();
    }

    public string PrettyString(NilTerm nt) => "[]";

    public string PrettyString(Free f)
    {
      int i = freeIndices.GetOrAdd(f, () => freeCounter++);
      return "_" + i;
    }

    public string PrettyString(TermList li)
    {
      return "[" + _listInnerPrettyString(li) + "]";
    }
    private string _listInnerPrettyString(TermList li)
    {
      if (li.Tail is TermList tailList)
      {
        return li.Head.Pretty(this) + ", " + _listInnerPrettyString(tailList);
      }
      else if (li.Tail is NilTerm)
      {
        return li.Head.Pretty(this);
      }
      else // tail is another term (unusual case)
      {
        return li.Head.Pretty(this) + "|" + li.Tail.Pretty(this);
      }
    }

    public string PrettyString(AlphaTuple at)
    {
      return "{" + string.Join(", ", at.Terms.Select(kv => kv.Key + ":" + kv.Value.ToString())) + "}";
    }

    // META TERMS

    public string PrettyString(NameVar nv)
    {
      if (nv.IsGround())
        return nv.Name;
      else
      {
        int i = nameVarIndices.GetOrAdd(nv, () => nameVarCounter++);
        return "_" + i;
      }
    }

    public string PrettyString(ProjectionMap pm)
    {
      return "{" + string.Join(", ", pm.Select(nn => nn.Key.Pretty(this) + ":" + nn.Value.Pretty(this))) + "}";
    }

    // PROGRAM TERMS

    public string PrettyString(Cons _)
    {
      return "cons";
    }

    public string PrettyString(Id _)
    {
      return "id";
    }

    public string PrettyString(Const cnst)
    {
      return "const(" + cnst.ArgumentName + ", " + cnst.Value.Pretty(this) + ")";
    }

    public string PrettyString(And a)
    {
      return "and(" + a.LHOperand.Pretty(this) + ", " + a.RHOperand.Pretty(this) + ")";
    }

    public string PrettyString(FoldL fl)
    {
      return "foldl(" + fl.Recursive.Pretty(this) + "," + fl.Base.Pretty(this) + ")";
    }

    public string PrettyString(FoldR fr)
    {
      return "foldr(" + fr.Recursive.Pretty(this) + "," + fr.Base.Pretty(this) + ")";
    }

    public string PrettyString(Proj pj)
    {

      return "proj(" + pj.Source.Pretty(this) + "," + pj.Projection.Pretty(this) + ")";

    }

    public string PrettyString(ObservedProgram op)
    {
      return op.Valence.Pretty(this) + "#" + op.Observables.Count() + "(Dr. " + op._remainingSearchDepth + ")";
    }

    // TYPE TERMS

    public string PrettyString(Valence v)
    {
      return "{" + string.Join(", ", v.Select(nv => nv.Key.Pretty(this) + ":" + nv.Value)) + "}";
    }

    public string PrettyString(AndOrValence aov)
    {
      return aov.LHDoms.Pretty(this) + "->" + aov.RHDoms.Pretty(this) + "->" + PrettyString(aov as Valence);
    }

    public string PrettyString(FoldValence fv)
    {
      return fv.RecursiveComponent.Pretty(this) + "->" + fv.BaseComponent.Pretty(this) + "->" + PrettyString(fv as Valence);
    }
  }
}
