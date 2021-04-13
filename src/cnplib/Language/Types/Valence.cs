using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  /// <summary>
  /// A structure that maps domain names to modes
  /// </summary>
  public class Valence : IReadOnlyDictionary<NameVar, Mode>, IEnumerable<KeyValuePair<NameVar,Mode>>
  {
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
      InsCount = dic.Count(kv => kv.Value == Mode.In);
      OutsCount = dic.Count(kv => kv.Value == Mode.Out);
      ModeNumber = InsCount * 23 + OutsCount * 17;
    }

    public int Count => dict.Count;
    public IEnumerable<NameVar> Keys => dict.Keys;
    /// <summary>
    /// A number obtained by number of input(I)/output(O) arguments. Always same for the same I and O, and different for any different I and O. Independent from names, therefore persists as the names become ground.
    /// </summary>
    public readonly int ModeNumber;
    /// <summary>
    /// Number of In domains
    /// </summary>
    public readonly int InsCount;
    /// <summary>
    /// Number of Out domains
    /// </summary>
    public readonly int OutsCount;
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

    public virtual bool IsGround()
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

    /// <summary>
    /// Can only compare ground valences
    /// </summary>
    protected bool EqualsNamesAndModes(Valence otherVal)
    {
      if (dict.Count != otherVal.Count)
        return false;
      foreach(var name in dict.Keys)
      {
        if (!name.IsGround())
          throw new ArgumentException("Valence comparison can only be made between ground valences.");
        if (!otherVal.TryGetValue(name, out Mode otherMode))
          return false;
        var myMode = dict[name];
        if (myMode != otherMode)
          return false;
      }
      return true;
    }

    /// <summary>
    /// This Valence has to be ground otherwise throws.
    /// </summary>
    public override bool Equals(object obj)
    {
      if (obj is Valence otherVal)
        return EqualsNamesAndModes(otherVal);
      else return false;
    }

    /// <summary>
    /// Checks, against a given list of names and modes, that this and the given list maps all names to same modes. Requires all names to be ground.
    /// </summary>
    public bool MapsAllNamesToSameMode(IEnumerable<KeyValuePair<NameVar,Mode>> groundNamesAndModes)
    {
      return groundNamesAndModes.All(g =>
      {
        if (!g.Key.IsGround())
          throw new ArgumentException("All names should be ground for this check.");
        return ContainsKey(g.Key) && this[g.Key] == g.Value;
      });
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
      IEnumerable<KeyValuePair<NameVar, Mode>> myGroundDomains, myFreeDomains;
      (myGroundDomains, myFreeDomains) = this.WhereAndNot(n => n.Key.IsGround());
      // if this valence is already ground, just return empty cloning map
      if (!myFreeDomains.Any())
        return Iterators.Singleton(new TermReferenceDictionary());
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
      var allBindings = (myFreeIns.Count(), myFreeOuts.Count()) switch
      {
        (0, _) => outBindings,
        (_, 0) => inBindings,
        (_, _) => inBindings.Cartesian(outBindings, (@is, os) => @is.Concat(os))
      };
      var allMaps = allBindings.Select(o => new TermReferenceDictionary(o));
      return allMaps;
    }

  }
}

