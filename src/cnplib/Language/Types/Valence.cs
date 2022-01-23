using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CNP.Display;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  /// <summary>
  /// A structure that maps domain names to modes
  /// </summary>
  public class Valence : IReadOnlyDictionary<NameVar, Mode>, IEnumerable<KeyValuePair<NameVar,Mode>>, IPrettyStringable
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
      dict = new Dictionary<NameVar, Mode>(dic, NameVar.StringComparer.Instance);
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

    public virtual string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }
    
    /// <summary>
    /// Can only compare ground valences
    /// </summary>
    protected bool EqualsNamesAndModes(Valence otherVal)
    {
      if (dict.Count != otherVal.Count)
        return false;
      foreach(var name in dict.Keys)
      {
        //if (name.IsGround())
        //  throw new ArgumentException("Valence comparison can only be made between ground valences.");
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

    public bool NameConstraintsHold()
    {
      return Names.All(n=>n.NameConstraintsHold());
    }

    /// <summary>
    /// Takes a ground(?) name mode map, and returns all the possible assignments for free domains in this
    /// one to those in the given one.
    /// ?: nothing in the code requires the given valence to be ground. If it's partially ground, and its ground domains match some of the free domains in this valence, the returning unifications are a bit more ground-ing. Otherwise, if none of the domains in the given valence is ground, the returned unification only produces an alpha-equivalent valence.
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
      var myFreeIns = myFreeDomains.Where(d => d.Value == Mode.In).Select(d => d.Key);
      var myFreeOuts = myFreeDomains.Where(d => d.Value == Mode.Out).Select(d => d.Key);
      var targetGroundIns = targetDomsToMatch.Where(d => d.Value == Mode.In).Select(d => d.Key);
      var targetGroundOuts = targetDomsToMatch.Where(d => d.Value == Mode.Out).Select(d => d.Key);
      if (myFreeIns.Count() != targetGroundIns.Count() || myFreeOuts.Count() != targetGroundOuts.Count())
        return Iterators.Empty<TermReferenceDictionary>();
      var targetInPerms = targetGroundIns.Permutations();
      var targetOutPerms = targetGroundOuts.Permutations();
      var inBindings = targetInPerms.Select(gs => myFreeIns.Zip(gs)).Where(b => b.All(pair => pair.Item1.NameConstraintsAllow(pair.Item2)));
      var outBindings = targetOutPerms.Select(gs => myFreeOuts.Zip(gs)).Where(b => b.All(pair => pair.Item1.NameConstraintsAllow(pair.Item2)));
      var allBindings = (myFreeIns.Count(), myFreeOuts.Count()) switch
      {
        (0, _) => outBindings.Select(b => b.Select(pair => new KeyValuePair<Term, Term>(pair.Item1, pair.Item2))),
        (_, 0) => inBindings.Select(b => b.Select(pair => new KeyValuePair<Term,Term>(pair.Item1,pair.Item2))),
        (_, _) => inBindings.Cartesian(outBindings, (ibs, obs) => ibs.Concat(obs).Select(b => new KeyValuePair<Term,Term>(b.Item1,b.Item2)))
      };
      var allMaps = allBindings.Select(o => new TermReferenceDictionary(o));
      return allMaps;
    }

    /// <summary>
    /// Throws if there are shared names among the two valences.
    /// </summary>
    public static Valence Combine(Valence val1, Valence val2)
    {
      return new Valence(val1.Concat(val2));
    }
  }
}

