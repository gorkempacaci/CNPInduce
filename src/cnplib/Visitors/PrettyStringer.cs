using System;
using System.Text;
using CNP.Helper;
using System.Linq;
using CNP.Language;

namespace CNP
{
  public interface IPrettyStringable
  {
    string Accept(ICNPVisitor ps);
  }
  /// <summary>
  /// Produces a string representation (replacing ToString() in most cases) for terms, meta-terms, and program terms.
  /// </summary>
  public class PrettyStringer : ICNPVisitor
  {
    public NameVarBindings names;
    bool isContextless;

    public static PrettyStringer Contextless = new PrettyStringer(VisitorOptions.Contextless);

    public PrettyStringer(VisitorOptions op)
    {
      if (op == VisitorOptions.Contextless)
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

    public string Visit(ConstantTerm ct)
    {
      if (ct.Value is string)
        return "'" + ct.Value + "'";
      else return (ct.Value).ToString();
    }

    public string Visit(NilTerm nt) => "[]";

    public string Visit(Free f)
    {
      return "F" + f.Index;
    }

    public string Visit(TermList li)
    {
      return "[" + _listInnerPrettyString(ref li) + "]";
    }

    private string _listInnerPrettyString(ref TermList li)
    {
      if (li.Tail is TermList tailList)
      {
        return li.Head.Accept(this) + ", " + _listInnerPrettyString(ref tailList);
      }
      else if (li.Tail is NilTerm)
      {
        return li.Head.Accept(this);
      }
      else // tail is another term (unusual case)
      {
        return li.Head.Accept(this) + "|" + li.Tail.Accept(this);
      }
    }


    

    // PROGRAM TERMS

    public string Visit(Cons _)
    {
      return "cons";
    }

    public string Visit(Id _)
    {
      return "id";
    }

    public string Visit(Const cnst)
    {
      return "const(" + cnst.ArgumentName.Accept(this) + ", " + cnst.Value.Accept(this) + ")";
    }

    public string Visit(And a)
    {
      return "and(" + a.LHOperand.Accept(this) + ", " + a.RHOperand.Accept(this) + ")";
    }

    public string Visit(FoldL fl)
    {
      return "foldl(" + fl.Recursive.Accept(this) + ")";
    }

    public string Visit(FoldR fr)
    {
      return "foldr(" + fr.Recursive.Accept(this) + ")";
    }

    public string Visit(Proj pj)
    {

      return "proj(" + pj.Source.Accept(this) + ", " + pj.Projection.Accept(this) + ")";

    }

    public string Visit(ObservedProgram op)
    {
      if (op.Observations.Length==1)
      {
        return op.Observations[0].Valence.Accept(this) + "#" + op.Observations[0].Examples.TuplesCount + "(RD=" + op.RemainingSearchDepth + $", RU={op.RemainingUnboundArguments})";
      }
      else
      {
        return $"({op.Observations.Length} obrvs, RD={op.RemainingSearchDepth}, RU={op.RemainingUnboundArguments})";
      }

    }

    // META TERMS

    public string Visit(AlphaRelation at)
    {
      return Visit(at.Tuples, names.GetNamesForVars(at.Names));
    }

    public string Visit(ITerm[][] tuples, string[] colNames)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("{");
      for (int r = 0; r < tuples.Length; r++)
      {
        if (r != 0)
          sb.Append(", ");
        sb.Append(Visit(tuples[r], colNames));
      }
      sb.Append("}");
      return sb.ToString();
    }

    public string Visit(ITerm[] terms, string[] colNames)
    {
      StringBuilder sb = new();
      sb.Append("{");
      for (int c = 0; c < terms.Length; c++)
      {
        if (c != 0)
          sb.Append(", ");
        sb.Append(colNames[c]+":"+terms[c].Accept(this));
      }
      sb.Append("}");
      return sb.ToString();
    }

    public string Visit(NameVar nv)
    {
      string n;
      if (isContextless || (n = names.GetNameForVar(nv)) == null)
        return "n" + nv.Index;
      else return n;
    }

    public string Visit(ValenceVar vv)
    {
      var ins = vv.Ins.Length == 0 ? "" : string.Join(", ", vv.Ins.Select(i => i.Accept(this)));
      var outs = vv.Outs.Length == 0 ? "" : string.Join(", ", vv.Outs.Select(o => o.Accept(this)));
      var inouts = ins == "" ? outs : (outs == "" ? ins : ins + ", " + outs);
      return "{" + inouts + "}";
    }

    public string Visit(ProjectionMap pm)
    {
      return "{" + string.Join(", ",
        pm.Map.Select(nn => nn.Key.Accept(this) + "->" + nn.Value.Accept(this))) + "}";
    }


    public string Visit(ProtoAndValence pav)
    {
      var opModes = "[" + string.Join(", ", pav.OpModes.Select(m => m.ToString())) + "]";
      var lhModes = "[" + string.Join(", ", pav.LHModes.Select(m => m is null ? "_" : m.ToString())) + "]";
      var rhModes = "[" + string.Join(", ", pav.RHModes.Select(m => m is null ? "_" : m.ToString())) + "]";
      return opModes + "(" + lhModes + ", " + rhModes + ")";
    }
  }
}
