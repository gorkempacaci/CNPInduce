using System;
using System.Collections.Generic;
using System.Collections;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using CNP.Display;

namespace CNP.Language
{
  public class AlphaTuple : IEnumerable<KeyValuePair<NameVar, Term>>, IFreeContext, IPrettyStringable
  {
    private int hashCode = -1;
    private readonly Dictionary<NameVar, Term> _terms;
    public IReadOnlyDictionary<NameVar, Term> Terms => _terms;

    public IEnumerable<NameVar> DomainNames => _terms.Keys;

    public AlphaTuple(params (NameVar, Term)[] terms)
        : this(terms.ToDictionary(t => t.Item1, t => t.Item2))
    {

    }

    /// <summary>
    /// Builds a new alphatuple from a dictionary. Creates a new data structure internally. Does not do a deep copy, so the same name and terms are used.
    /// </summary>
    /// <param name="terms"></param>
    public AlphaTuple(IEnumerable<KeyValuePair<NameVar, Term>> terms)
    {
      _terms = new Dictionary<NameVar, Term>(NameVar.StringComparer.Instance);
      foreach (KeyValuePair<NameVar, Term> nv in terms)
      {
        _terms.Add(nv.Key, nv.Value);
        if (nv.Value is Free freeValue)
        {
          freeValue.AddAContext(this);
        }
      }
      // combine hashcodes for domains in-order to obtain a hashcode for the alphatuple.
      foreach(var kv in _terms)
      {
        if (hashCode == -1)
          hashCode = kv.Key.GetHashCode();
        else hashCode = HashCode.Combine(hashCode, kv.Key.GetHashCode());
      }
    }

    public AlphaTuple Clone(TermReferenceDictionary plannedParenthood)
    {
      return new AlphaTuple(_terms.Select(e => new KeyValuePair<NameVar, Term>(e.Key.Clone(plannedParenthood), e.Value.Clone(plannedParenthood))));
    }

    /// <summary>
    /// Returns a cropped copy of this AlphaTuple where only domain-term pairs for the given domains exist.
    /// For example: {a:1, b:2, c:3}.Crop([a,b]) returns {a:1, b:2}.
    /// </summary>
    /// <param name="someDomains"></param>
    /// <returns></returns>
    public AlphaTuple Crop(IEnumerable<NameVar> someDomains)
    {
      var domAndTerms = someDomains.Select(d => new KeyValuePair<NameVar, Term>(d, _terms[d]));
      var atup = new AlphaTuple(domAndTerms);
      return atup;
    }

    public IEnumerator<KeyValuePair<NameVar, Term>> GetEnumerator()
    {
      return Terms.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return Terms.GetEnumerator();
    }

    public void ReplaceAllInstances(Free oldTerm, Term newTerm)
    {
      var keys = new List<NameVar>(_terms.Keys);
      var values = new List<Term>(_terms.Values);
      bool foundFree = false;
      for (int i = 0; i < values.Count; i++)
      {
        if (object.ReferenceEquals(values[i], oldTerm))
        {
          _terms[keys[i]] = newTerm;
          if (newTerm is Free newTermFree)
          {
            newTermFree.AddAContext(this);
          }
          foundFree = true;
        }
      }
      if (!foundFree)
      {
        throw new Exception("Free was not found in AlphaTuple to be replaced.");
      }
    }

    //TODO: is there a way to avoid new NameVar here?
    public Term this[string name] => Terms[new NameVar(name)];

    public Term this[NameVar name] => Terms[name];

    public override bool Equals(object that)
    {
      if (ReferenceEqualityComparer.Instance.Equals(this, that))
        return true;
      if (that is not AlphaTuple thatTuple)
        return false;
      if (!DomainNames.SequenceEqual(thatTuple.DomainNames))
        return false;
      if (!Terms.SequenceEqual(thatTuple.Terms))
        return false;
      return true;
    }

    public override int GetHashCode()
    {
      return hashCode;
    }

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public override string ToString()
    {
      return "(Contextless) " + Pretty(new());
    }

  }

}
