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
  public class Valence : IEnumerable<KeyValuePair<ArgumentNameVar, ArgumentMode>>
  {
    readonly int modeNumber; // persists as the names become ground
    readonly IReadOnlyDictionary<ArgumentNameVar, ArgumentMode> dict;

    public IEnumerable<ArgumentNameVar> Names => dict.Keys;

    /// <summary>
    /// A number obtained by number of input(I)/output(O) arguments. Always same for the same I and O, and different for any different I and O. Independent from names.
    /// </summary>
    public int ModeNumber => modeNumber;

    public Valence(params (string, ArgumentMode)[] tups)
        : this(tups.ToDictionary(x => new ArgumentNameVar(x.Item1), x => x.Item2))
    {
    }

    public Valence(params (ArgumentNameVar, ArgumentMode)[] tups)
        : this(tups.ToDictionary(x => x.Item1, x => x.Item2))
    {
    }

    public Valence(IEnumerable<KeyValuePair<string, ArgumentMode>> args)
        : this(args.ToDictionary(x => new ArgumentNameVar(x.Key), x => x.Value))
    {

    }

    public Valence(IEnumerable<KeyValuePair<ArgumentNameVar, ArgumentMode>> args)
        : this(args.ToDictionary(x => x.Key, x => x.Value))
    {

    }

    public Valence(IDictionary<ArgumentNameVar, ArgumentMode> dic)
    {
      dict = new Dictionary<ArgumentNameVar, ArgumentMode>(dic);
      int ins = dic.Count(kv => kv.Value == ArgumentMode.In);
      int outs = dic.Count(kv => kv.Value == ArgumentMode.Out);
      modeNumber = ins * 23 + outs * 17;
    }

    public Valence Clone(TermReferenceDictionary plannedParenthood)
    {
      return new Valence(dict.ToDictionary(kvp => kvp.Key.Clone(plannedParenthood) as ArgumentNameVar, kvp => kvp.Value));
    }

    public bool IsGround()
    {
      return dict.Keys.All(k => k.IsGround());
    }




    public bool TryGetValue(ArgumentNameVar name, out ArgumentMode mode)
    {
      return dict.TryGetValue(name, out mode);
    }

    /// <summary>
    /// Takes a ground name mode map, and returns all the possible assignments for free domains in this
    /// one to those in the given one.
    /// </summary>
    /// <param name="targetDomains"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public IEnumerable<TermReferenceDictionary> PossibleGroundings(Valence targetDomains)
    {
      IEnumerable<KeyValuePair<ArgumentNameVar, ArgumentMode>> myGroundDomains =
          this.WhereAndNot(n => n.Key.IsGround(), out IEnumerable<KeyValuePair<ArgumentNameVar, ArgumentMode>> myFreeDomains);
      var targetDomsToMatch = targetDomains.Except(myGroundDomains);
      if (targetDomains.Count() != targetDomsToMatch.Count() + myGroundDomains.Count())
        return Iterators.Empty<TermReferenceDictionary>();
      if (myFreeDomains.Count() != targetDomsToMatch.Count())
        return Iterators.Empty<TermReferenceDictionary>();
      var myFreeIns = myFreeDomains.Where(d => d.Value == ArgumentMode.In);
      var myFreeOuts = myFreeDomains.Where(d => d.Value == ArgumentMode.Out);
      var targetGroundIns = targetDomsToMatch.Where(d => d.Value == ArgumentMode.In);
      var targetGroundOuts = targetDomsToMatch.Where(d => d.Value == ArgumentMode.Out);
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

    public ArgumentMode this[string name] => dict[new ArgumentNameVar(name)];
    public ArgumentMode this[ArgumentNameVar name] => dict[name];

    public IEnumerator<KeyValuePair<ArgumentNameVar, ArgumentMode>> GetEnumerator()
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
        foreach (ArgumentNameVar an in Names)
        {
          bool ifMe = this.TryGetValue(an, out ArgumentMode myMode);
          bool ifOther = otherPs.TryGetValue(an, out ArgumentMode otherMode);
          bool modes = myMode.Equals(otherMode);
          return ifMe && ifOther && modes;
        }

        return Names.All(k => this.TryGetValue(k, out ArgumentMode myMode) &&
                              otherPs.TryGetValue(k, out ArgumentMode othersMode) &&
                              myMode.Equals(othersMode));
      }

      return false;
    }

    public override string ToString()
    {
      return "{" + string.Join(", ", dict.Select(nv => nv.Key.ToString() + ":" + nv.Value.ToString())) + "}";
    }
  }
}