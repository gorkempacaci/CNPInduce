using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

  /// <summary>
  /// Term representing free variables, constants (through variables), lists of these.
  /// </summary>
  public interface ITerm : IPrettyStringable
  {
    /// <summary>
    /// Returns true if Free occurs in this term.
    /// </summary>
    bool Contains(Free other);

    /// <summary>
    /// The term has no Free s.
    /// </summary>
    bool IsGround();

    ITerm Clone(CloningContext cc);

    public string ToString()
    {
      return Accept(new PrettyStringer(VisitorOptions.Contextless));
    }

    /// <summary>
    /// Return a copy of the term with the given free replaced with the given term.
    /// </summary>
    public ITerm GetFreeReplaced(Free searchedFree, ITerm newTerm);

    /// <summary>
    /// Returns false with suggestedSubstition=null if it does not unify.
    /// Returns false with suggestedSubstition nonempty if terms unify to a point with given substitutions, but needs to be called again (after these substitutions are applied) until it returns true without substitutions.
    /// Returns true with suggestedSubstitutions=null if terms unify without substitutions.
    /// </summary>
    public static bool UnifyOrSuggest(ITerm a, ITerm b, out (Free, ITerm)? suggestedSubstitution)
    {
      suggestedSubstitution = null;
      switch((a,b))
      {
        case (null, _):
        case (_, null):
          throw new Exception("Unification cannot unify null values.");
        case (Free fa, Free fb) when fa.Index == fb.Index:
          return true;
        case (Free fa, Free fb) when fa.Index != fb.Index:
          suggestedSubstitution = (fa, fb);
          break;
        case (Free fa, ITerm tb) when tb is not Free && !tb.Contains(fa):
          suggestedSubstitution = (fa, tb);
          break;
        case (ITerm ta, Free fb) when ta is not Free && !ta.Contains(fb):
          suggestedSubstitution = (fb, ta);
          break;
        case (TermList la, TermList lb):
          if(UnifyOrSuggest(la.Head, lb.Head, out var headSuggestion))
          {
            if (headSuggestion is not null)
              throw new Exception("Unification returned true but with substitution suggestions.");
            if (UnifyOrSuggest(la.Tail, lb.Tail, out var tailSuggestion))
            {
              if (tailSuggestion is not null)
                throw new Exception("Unification returned true but with substitution suggestions.");
              return true;
            }
            else
              suggestedSubstitution = tailSuggestion;
          }
          else
            suggestedSubstitution = headSuggestion;
          break;
        case (ConstantTerm ca, ConstantTerm cb):
          return ca.Equals(cb);
        case (NilTerm _, NilTerm _):
          return true;
      }
      return false;
    }



  }

}