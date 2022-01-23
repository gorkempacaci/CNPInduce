using System;
using System.Collections.Generic;
using CNP.Display;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public class Free : Term
  {
    /// <summary>
    /// The contexts this Free appears in. 
    /// </summary>
    private List<IFreeContext> contexts = new();


    public override bool IsGround() => false;



    public override Term Clone(TermReferenceDictionary plannedParenthood)
    {
      return plannedParenthood.GetOrAdd(this, () => new Free());
    }

    public override bool Contains(Free other) => false;

    public override string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    /// <summary>
    /// Adds a context the free appears in. For example, if Free X, and a Tuple {a:X, b:[X, [1, 2, X]]}, then X has three contexts, one tuple, and two lists (one through the head one through the tail)
    /// </summary>
    /// <param name="c"></param>
    public void AddAContext(IFreeContext c)
    {
      if (!this.contexts.Any(co => object.ReferenceEquals(co, c)))
        this.contexts.Add(c);
    }

    /// <summary>
    /// Replaces this Free in all the contexts it appears in, with the given term. After this this Free should be disposable.
    /// </summary>
    public void ReplaceInAllContexts(Term t)
    {
      foreach (IFreeContext c in contexts)
        c.ReplaceAllInstances(this, t);
    }
  }

}
