using System;
using System.Text;
using CNP.Helper;
using System.Linq;
using CNP.Language;

namespace CNP
{
  public interface IPrettyStringable
  {
    string Pretty(PrettyStringer ps);
  }
  /// <summary>
  /// Produces a string representation (replacing ToString() in most cases) for terms, meta-terms, and program terms.
  /// </summary>
  public class PrettyStringer
  {
    public NameVarBindings names;
    bool isContextless;

    public enum Options { Contextless }

    public PrettyStringer(Options op)
    {
      if (op == Options.Contextless)
      {
        names = default;
        isContextless = true;
      }
    }

    public PrettyStringer(NameVarBindings nm)
    {
      names = nm;
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
      return "F" + f.Index;
    }

    public string PrettyString(TermList li)
    {
      return "[" + _listInnerPrettyString(ref li) + "]";
    }

    private string _listInnerPrettyString(ref TermList li)
    {
      if (li.Tail is TermList tailList)
      {
        return li.Head.Pretty(this) + ", " + _listInnerPrettyString(ref tailList);
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
      return "const(" + cnst.ArgumentName.Pretty(this) + ", " + cnst.Value.Pretty(this) + ")";
    }

    public string PrettyString(And a)
    {
      return "and(" + a.LHOperand.Pretty(this) + ", " + a.RHOperand.Pretty(this) + ")";
    }

    public string PrettyString(FoldL fl)
    {
      return "foldl(" + fl.Recursive.Pretty(this) + ", " + fl.Base.Pretty(this) + ")";
    }

    public string PrettyString(FoldR fr)
    {
      return "foldr(" + fr.Recursive.Pretty(this) + ", " + fr.Base.Pretty(this) + ")";
    }

    public string PrettyString(Proj pj)
    {

      return "proj(" + pj.Source.Pretty(this) + ", " + pj.Projection.Pretty(this) + ")";

    }

    public string PrettyString(ObservedProgram op)
    {
      return op.Valence.Pretty(this) + "#" + op.Observables.TuplesCount + "(Dr. " + op.RemainingSearchDepth + ")";
    }

    // META TERMS

    public string PrettyString(AlphaRelation at)
    {
      return PrettyString(at.Tuples, names.GetNamesForVars(at.Names));
    }

    public string PrettyString(ITerm[][] tuples, string[] colNames)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("{");
      for (int r = 0; r < tuples.Length; r++)
      {
        if (r != 0)
          sb.Append(", ");
        sb.Append(PrettyString(tuples[r], colNames));
      }
      sb.Append("}");
      return sb.ToString();
    }

    public string PrettyString(ITerm[] terms, string[] colNames)
    {
      StringBuilder sb = new();
      sb.Append("{");
      for (int c = 0; c < terms.Length; c++)
      {
        if (c != 0)
          sb.Append(", ");
        sb.Append(colNames[c]+":"+terms[c].Pretty(this));
      }
      sb.Append("}");
      return sb.ToString();
    }

    public string PrettyString(NameVar nv)
    {
      string n;
      if (isContextless || (n = names.GetNameForVar(nv)) == null)
        return "n" + nv.Index;
      else return n;
    }

    public string PrettyString(ValenceVar vv)
    {
      var ins = vv.Ins.Length == 0 ? "" : string.Join(", ", vv.Ins.Select(i => i.Pretty(this)));
      var outs = vv.Outs.Length == 0 ? "" : string.Join(", ", vv.Outs.Select(o => o.Pretty(this)));
      var inouts = ins == "" ? outs : (outs == "" ? ins : ins + ", " + outs);
      return "{" + inouts + "}";
    }

    public string PrettyString(ProjectionMap pm)
    {
      return "{" + string.Join(", ",
        pm.Map.Select(nn => nn.Key.Pretty(this) + "->" + nn.Value.Pretty(this))) + "}";
    }


    public string PrettyString(AndValence av)
    {
      return av.LHValence.Pretty(this) + "->" + av.RHValence.Pretty(this) + "->" + av.OperatorValence.Pretty(this);
    }

  }
}
