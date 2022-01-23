using System;
using System.Collections.Generic;
using CNP.Display;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

  /// <summary>
  /// Term representing free variables, constants (through variables), lists of these.
  /// </summary>
  public abstract class Term : IPrettyStringable
  {
    /// <summary>
    /// Clones the whole term while preserving Free references correctly. For example, a list such as [1, 2, X, 4, X, Y] is cloned to [1, 2, A, 4, A, B].
    /// </summary>
    /// <param name="plannedParenthood">Accumulated mapping of Free instances and the new Free references they are replaced with.</param>
    /// <returns></returns>
    public abstract Term Clone(TermReferenceDictionary plannedParenthood);

    /// <summary>
    /// Returns true if Free occurs in this term.
    /// </summary>
    public abstract bool Contains(Free other);

    /// <summary>
    /// The term has no Free s.
    /// </summary>
    public abstract bool IsGround();

    /// <summary>
    /// Returns a string representation of the object using the pretty printer given. (Accept method of visitor pattern)
    /// </summary>
    public abstract string Pretty(PrettyStringer ps);

    public override string ToString()
    {
      return Pretty(new PrettyStringer());
    }

    /// <summary>
    /// Returns false if does not unify. Returns true if this term unifies with the given term, which may have
    /// resulted in modified state in the terms.
    /// </summary>
    public static bool UnifyInPlace(Term a, Term b)
    {

      if (a == null || b == null)
      {
        throw new Exception("Cannot unify null.");
      }
      else if (a is Free aFree)
      {
        if (b.Contains(aFree))
        {
          return false;
        }
        else if (object.ReferenceEquals(a, b))
        {
          return true;
        }
        else
        {
          aFree.ReplaceInAllContexts(b);
          return true;
        }
      }
      else if (a is ConstantTerm aConst)
      {
        if (b is Free bFree)
        {
          bFree.ReplaceInAllContexts(a);
          return true;
        }
        else if (b is ConstantTerm bConst)
        {
          bool isEq = aConst.Equals(bConst);
          return isEq;
        }
        else
        {
          return false;
        }
      }
      else if (a is TermList aList)
      {
        if (b is Free bFree)
        {
          if (a.Contains(bFree))
          {
            return false;
          }
          else
          {
            bFree.ReplaceInAllContexts(a);
            return true;
          }
        }
        else if (b is ConstantTerm)
        {
          return false;
        }
        else if (b is TermList bList)
        {
          bool headUnifies = UnifyInPlace(aList.Head, bList.Head);
          if (headUnifies)
          {
            bool tailUnifies = UnifyInPlace(aList.Tail, bList.Tail);
            return tailUnifies;
          }
          else return false;
        }
        else if (b is NilTerm)
        {
          return false;
        }
      }
      else if (a is NilTerm)
      {
        if (b is NilTerm)
          return true;
        else if (b is Free bFree)
        {
          bFree.ReplaceInAllContexts(a);
          return true;
        }
        else
          return false;
      }
      throw new Exception("Unification does not recognize one of the terms:" + a.ToString() + ", " + b.ToString());
    }



  }

}