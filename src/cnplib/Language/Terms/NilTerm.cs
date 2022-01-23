using System;
using System.Collections.Generic;
using System.Text;
using CNP.Display;
using CNP.Helper;

namespace CNP.Language
{

  public class NilTerm : Term
  {
    private NilTerm()
    {

    }
    public override bool IsGround()
    {
      return true;
    }
    public override NilTerm Clone(TermReferenceDictionary plannedParenthood)
    {
      return this;
    }

    public override bool Contains(Free other)
    {
      return false;
    }

    public override string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    private static NilTerm _instance = new NilTerm();
    public static NilTerm Instance => _instance;
  }
}