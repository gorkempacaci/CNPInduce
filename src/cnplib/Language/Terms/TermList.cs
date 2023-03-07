using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public struct TermList : ITerm
  {
    public ITerm Head;
    public ITerm Tail;

    public TermList(ITerm h, ITerm t)
    {
      Head = h;
      Tail = t;
    }
    public static ITerm FromTerms(params ITerm[] terms)
    {
      return FromEnumerable(terms);
    }

    /// <summary>
    /// The list tail is by default nil (NilTerm).
    /// </summary>
    public static ITerm FromEnumerable(IEnumerable<ITerm> terms, ITerm tail = null)
    {
      return FromEnumerator(terms.GetEnumerator(), tail);
    }
    static ITerm FromEnumerator(IEnumerator<ITerm> it, ITerm tail)
    {
      if (it.MoveNext())
        return new TermList(it.Current, FromEnumerator(it, tail));
      else
        return tail ?? new NilTerm();
    }

    public bool IsGround()
    {
      return Head.IsGround() && Tail.IsGround();
    }

    public bool Contains(Free other)
    {
      return Head.Contains(other) || Tail.Contains(other);
    }

    public override bool Equals(object obj)
    {
      return (obj is TermList li) &&
          li.Head.Equals(Head) &&
          li.Tail.Equals(Tail);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Head, Tail);
    }


    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public ITerm Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public ITerm GetFreeReplaced(Free searchedFree, ITerm newTerm)
    {
      return new TermList(Head.GetFreeReplaced(searchedFree, newTerm), Tail.GetFreeReplaced(searchedFree, newTerm));
    }

    public override string ToString()
    {
      return "[" + Head.ToString() + (Tail is NilTerm ? "" : ", " + Tail.ToString()) + "]";
    }

    /// <summary>
    /// Returns the terms in an IEnumerable.
    /// </summary>
    /// <param name="includeTerminalNil">If set to true returns [] as an element. For example, returns [1,2,3,[]] for [1,2,3]. Still returns [1,2,3] for [1,2|3].</param>
    /// <returns>A list of all elements in the list. It returns [1,2,3] for both [1,2,3] and [1,2|3]. </returns>
    public IEnumerable<ITerm> ToEnumerable(bool includeTerminalNil = false)
    {
      yield return Head;
      if (Tail is NilTerm && includeTerminalNil)
      {
        yield return Tail;
      }
      else
      {
        if (Tail is not TermList li)
        {
          yield return Tail;
        }
        else
        {
          foreach (ITerm t in li.ToEnumerable(includeTerminalNil))
            yield return t;
        }
      }
    }
  }
}
