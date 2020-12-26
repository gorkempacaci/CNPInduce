using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using Enumerable = System.Linq.Enumerable;

namespace CNP.Language
{
  /// <summary>
  /// A structure that maps domain names to modes
  /// </summary>
  public class Valence : IReadOnlyDictionary<NameVar, Mode>
  {
    readonly int modeNumber; // persists as the names become ground
    readonly IReadOnlyDictionary<NameVar, Mode> dict;


    public Valence(params (NameVar, Mode)[] tups)
        : this(tups.ToDictionary(x => x.Item1, x => x.Item2))
    {
    }

    public Valence(IEnumerable<KeyValuePair<NameVar, Mode>> args)
        : this(args.ToDictionary(x => x.Key, x => x.Value))
    {

    }

    public Valence(IDictionary<NameVar, Mode> dic)
    {
      dict = new Dictionary<NameVar, Mode>(dic);
      int ins = dic.Count(kv => kv.Value == Mode.In);
      int outs = dic.Count(kv => kv.Value == Mode.Out);
      modeNumber = ins * 23 + outs * 17;
    }

    public int Count => dict.Count;
    public IEnumerable<NameVar> Keys => dict.Keys;
    /// <summary>
    /// A number obtained by number of input(I)/output(O) arguments. Always same for the same I and O, and different for any different I and O. Independent from names.
    /// </summary>
    public int ModeNumber => modeNumber;
    public IEnumerable<NameVar> Names => dict.Keys;
    public IEnumerable<Mode> Values => dict.Values;
    public Mode this[string name] => dict[new NameVar(name)];
    public Mode this[NameVar name] => dict[name];


    public Valence Clone(TermReferenceDictionary plannedParenthood)
    {
      return new Valence(dict.ToDictionary(kvp => kvp.Key.Clone(plannedParenthood), kvp => kvp.Value));
    }

    public bool ContainsKey(NameVar key)
    {
      return dict.ContainsKey(key);
    }

    public bool IsGround()
    {
      return dict.Keys.All(k => k.IsGround());
    }

    public bool TryGetValue(NameVar name, out Mode mode)
    {
      return dict.TryGetValue(name, out mode);
    }

    public IEnumerator<KeyValuePair<NameVar, Mode>> GetEnumerator()
    {
      return dict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return dict.GetEnumerator();
    }

    public override int GetHashCode() => ModeNumber;

    public override bool Equals(object obj)
    {
      if (obj is Valence otherPs && dict.Keys.Count() == otherPs.Names.Count())
      {
        foreach (NameVar an in Names)
        {
          bool ifMe = this.TryGetValue(an, out Mode myMode);
          bool ifOther = otherPs.TryGetValue(an, out Mode otherMode);
          bool modes = myMode.Equals(otherMode);
          return ifMe && ifOther && modes;
        }

        return Names.All(k => this.TryGetValue(k, out Mode myMode) &&
                              otherPs.TryGetValue(k, out Mode othersMode) &&
                              myMode.Equals(othersMode));
      }

      return false;
    }

    public override string ToString()
    {
      return "{" + string.Join(", ", dict.Select(nv => nv.Key.ToString() + ":" + nv.Value.ToString())) + "}";
    }


    /// <summary>
    /// Takes a ground name mode map, and returns all the possible assignments for free domains in this
    /// one to those in the given one.
    /// </summary>
    /// <param name="targetDomains"></param>
    /// <returns></returns>
    public IEnumerable<TermReferenceDictionary> PossibleGroundings(Valence targetDomains)
    {
      IEnumerable<KeyValuePair<NameVar, Mode>> myGroundDomains =
          this.WhereAndNot(n => n.Key.IsGround(), out IEnumerable<KeyValuePair<NameVar, Mode>> myFreeDomains);
      var targetDomsToMatch = targetDomains.Except(myGroundDomains);
      if (targetDomains.Count() != targetDomsToMatch.Count() + myGroundDomains.Count())
        return Iterators.Empty<TermReferenceDictionary>();
      if (myFreeDomains.Count() != targetDomsToMatch.Count())
        return Iterators.Empty<TermReferenceDictionary>();
      var myFreeIns = myFreeDomains.Where(d => d.Value == Mode.In);
      var myFreeOuts = myFreeDomains.Where(d => d.Value == Mode.Out);
      var targetGroundIns = targetDomsToMatch.Where(d => d.Value == Mode.In);
      var targetGroundOuts = targetDomsToMatch.Where(d => d.Value == Mode.Out);
      if (myFreeIns.Count() != targetGroundIns.Count() || myFreeOuts.Count() != targetGroundOuts.Count())
        return Iterators.Empty<TermReferenceDictionary>();
      var targetInPerms = targetGroundIns.Permutations();
      var targetOutPerms = targetGroundOuts.Permutations();
      var inBindings = targetInPerms.Select(gs => myFreeIns.Zip(gs, (f, g) => new KeyValuePair<Term, Term>(f.Key, g.Key)));
      var outBindings = targetOutPerms.Select(gs => myFreeOuts.Zip(gs, (f, g) => new KeyValuePair<Term, Term>(f.Key, g.Key)));
      var allBindings = inBindings.Cartesian(outBindings, (@is, os) => @is.Concat(os));
      var allMaps = allBindings.Select(o => new TermReferenceDictionary(o));
      return allMaps;
    }

  }
}