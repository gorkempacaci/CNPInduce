using System;
using System.Linq;
using System.Text;
using CNP.Language;

namespace CNP
{
  public class DebugPrinter : ICNPVisitor
  {
    NameVarBindings names;

    public DebugPrinter(NameVarBindings nvb)
    {
      names = nvb;
    }

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

    string debugInfo(IProgram program)
    {
      return $"\"valence\": \"{program.DebugValenceString}\", \"observation\": \"{program.DebugObservationString}\"";
    }

    string withDebugInfo(string programName, IProgram program)
    {
      return $"{{ \"name\": \"{programName}\", {debugInfo(program)} }}";
    }

    string withDebugInfo(string operatorName, IProgram program, params IProgram[] components)
    {
      var componentsInnerStr = string.Join(", ", components.Select(c => c.Accept(this)));
      return $"{{ \"name\": \"{operatorName}\", {debugInfo(program)}, \"components\": [{componentsInnerStr}] }}";
    }

    public string Visit(LibraryProgram lib) => withDebugInfo(lib.Name, lib);

    public string Visit(Cons cns) => withDebugInfo("cons", cns);
    public string Visit(Id id) => withDebugInfo("id", id);
    public string Visit(Const cnst)
    {
      var str = "const(" + cnst.ArgumentName.Accept(this) + ", " + cnst.Value.Accept(this) + ")";
      return withDebugInfo(str, cnst);
    }

    public string Visit(And a) => withDebugInfo("and", a, a.LHOperand, a.RHOperand);
    public string Visit(FoldL fl) => withDebugInfo("foldl", fl, fl.Recursive);
    public string Visit(FoldR fr) => withDebugInfo("foldr", fr, fr.Recursive);

    public string Visit(Proj pj)
    {
      var mapStr = pj.Projection.Accept(this);
      var name = $"proj({mapStr})";
      return withDebugInfo(name, pj, pj.Source);
    }

    public string Visit(ObservedProgram op)
    {
      return $"{{\"name\":\"observation\", \"first valence\":\\\"{op.Observations[0].Valence.Accept(this)}\\\", \"observables\":\"#{op.Observations.Length}\", RemainingSearchDepth:{op.RemainingSearchDepth}}}";
    }

    // META TERMS

    public string Visit(AlphaRelation at)
    {
      return Visit(at.Tuples, names.GetNamesForVars(at.Names));
    }

    public string Visit(GroundRelation gt)
    {
      return Visit(gt.Tuples, gt.Names);
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
        sb.Append(colNames[c] + ":" +  terms[c].Accept(this));
      }
      sb.Append("}");
      return sb.ToString();
    }

    public string Visit(NameVar nv)
    {
      string n;
      if ((n = names.GetNameForVar(nv)) == null)
        return "n" + nv.Index;
      else return n;
    }

    public string Visit(ValenceVar vv)
    {
      var ins = vv.Ins.Length == 0 ? "" : string.Join(", ", vv.Ins.Select(i => i.Accept(this) + ":in"));
      var outs = vv.Outs.Length == 0 ? "" : string.Join(", ", vv.Outs.Select(o => o.Accept(this) + ":out"));
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
      var rhModes = "[" + string.Join(", ", pav.RHModesArr[0].Select(m => m is null ? "_" : m.ToString())) + "],...";
      return opModes + "(" + lhModes + ", " + rhModes + ")";
    }
  }
}

