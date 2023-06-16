using System;
using CNP.Language;
using System.Text;

namespace CNP
{
  public enum VisitorOptions { Contextless }

  public interface ICNPVisitor
  {
    // TERMS

    public string Visit(ConstantTerm ct);
    public string Visit(NilTerm nt);
    public string Visit(Free f);
    public string Visit(TermList li);

    // PROGRAM TERMS

    public string Visit(Cons _);
    public string Visit(Id _);
    public string Visit(Const cnst);
    public string Visit(And a);
    public string Visit(FoldL fl);
    public string Visit(FoldR fr);
    public string Visit(Proj pj);
    public string Visit(ObservedProgram op);

    // META TERMS

    public string Visit(GroundRelation gt);
    public string Visit(AlphaRelation at);
    public string Visit(ITerm[][] tuples, string[] colNames);
    public string Visit(ITerm[] terms, string[] colNames);
    public string Visit(NameVar nv);
    public string Visit(ValenceVar vv);
    public string Visit(ProjectionMap pm);
    public string Visit(ProtoAndValence av);
    public string Visit(LibraryProgram libraryProgram);
  }
}

