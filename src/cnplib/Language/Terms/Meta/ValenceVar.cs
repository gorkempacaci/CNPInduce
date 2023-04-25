using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{
  /// <summary>
  /// A structure that maps argument names to modes.
  /// </summary>
  public readonly struct ValenceVar
  {
    public readonly NameVar[] Ins;
    public readonly NameVar[] Outs;
    /// <summary>
    /// A number obtained by number of input(I)/output(O) arguments. Always same for the same I and O, and different for any different I and O. 
    /// </summary>
    public readonly int ModeNumber;


    public ValenceVar()
    {
      throw new InvalidOperationException("Cannot create ValenceVar without parameters.");
    }
    /// <summary>
    /// Assigns given arrays as is without copying
    /// </summary>
    public ValenceVar(NameVar[] ins, NameVar[] outs)
    {
      this.Ins = ins;
      this.Outs = outs;
      this.ModeNumber = GroundValence.CalculateValenceModeNumber(ins.Length, outs.Length);
    }

    /// <summary>
    /// Assumes ins and outs appear in the order they appear on names
    /// </summary>
    public Mode[] GetModesOrderedByNames(NameVar[] names)
    {
      int ii = 0, oi = 0;
      Mode[] modes = new Mode[names.Length];
      for(int i=0; i<names.Length; i++)
      {
        if (ii < Ins.Length && Ins[ii].Index == names[i].Index)
        {
          modes[i] = Mode.In;
          ii++;
        }
        else if (oi < Outs.Length && Outs[oi].Index == names[i].Index)
        {
          modes[i] = Mode.Out;
          oi++;
        }
        else throw new ArgumentOutOfRangeException("Names for reordering Modes are not in the right order.");
      }
      return modes;
    }

    /// <summary>
    /// Returns the indices of someNames in the given allNames array. Assumes someNames are in the order they appear in the allNames array.
    /// OPTIMIZE: can return ins and outs at the same time like the one above.
    /// </summary>
    public (short[] ins, short[] outs) GetIndicesOfInsOrOutsIn(in NameVar[] allNames)
    {
      var indices = (ins: new short[Ins.Length], outs: new short[Outs.Length]);
      int ii = 0, oi = 0;
      for (short i=0; i<allNames.Length; i++)
      {
        if (ii<Ins.Length && allNames[i].Index == Ins[ii].Index)
          indices.ins[ii++] = i;
        else if (oi<Outs.Length && allNames[i].Index == Outs[oi].Index)
          indices.outs[oi++] = i;
        else throw new ArgumentException("Valence names are not in the same order as observation.");
      }
      return indices;
    }

    public static ValenceVar FromArray(KeyValuePair<NameVar, Mode>[] pairs)
    {
      var (ins, outs) = pairs.WhereAndNot(kv => kv.Value == Mode.In);
      return new ValenceVar(ins.Select(i => i.Key).ToArray(), outs.Select(o => o.Key).ToArray());
    }

    public static ValenceVar FromModeIndices(NameVar[] allNames, ModeIndices indices)
    {
      var insNames = indices.Ins.Select(i => allNames[i]).ToArray();
      var outsNames = indices.Outs.Select(i => allNames[i]).ToArray();
      return new ValenceVar(insNames, outsNames);
    }

    public ValenceVar Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public override int GetHashCode() => ModeNumber;

    /// <summary>
    /// This Valence has to be ground otherwise throws.
    /// </summary>
    public override bool Equals(object obj)
    {
      throw new NotImplementedException("ValenceVars cannot be compared for equality.");
      // comparing, if must be done, needs to take into account the lack of ordering in the arrays.
    }


    ///// <summary>
    ///// Takes a ground(?) name mode map, and returns all the possible assignments for free domains in this
    ///// one to those in the given one.
    ///// ?: nothing in the code requires the given valence to be ground. If it's partially ground, and its ground domains match some of the free domains in this valence, the returning unifications are a bit more ground-ing. Otherwise, if none of the domains in the given valence is ground, the returned unification only produces an alpha-equivalent valence.
    ///// </summary>
    ///// <param name="targetDomains"></param>
    ///// <returns></returns>
    //public IEnumerable<TermReferenceDictionary> PossibleGroundings(Valence targetDomains)
    //{
    //  IEnumerable<KeyValuePair<NameVar, Mode>> myGroundDomains, myFreeDomains;
    //  (myGroundDomains, myFreeDomains) = this.WhereAndNot(n => n.Key.IsGround());
    //  // if this valence is already ground, just return empty cloning map
    //  if (!myFreeDomains.Any())
    //    return Iterators.Singleton(new TermReferenceDictionary());
    //  var targetDomsToMatch = targetDomains.Except(myGroundDomains);
    //  if (targetDomains.Count() != targetDomsToMatch.Count() + myGroundDomains.Count())
    //    return Iterators.Empty<TermReferenceDictionary>();
    //  if (myFreeDomains.Count() != targetDomsToMatch.Count())
    //    return Iterators.Empty<TermReferenceDictionary>();
    //  var myFreeIns = myFreeDomains.Where(d => d.Value == Mode.In).Select(d => d.Key);
    //  var myFreeOuts = myFreeDomains.Where(d => d.Value == Mode.Out).Select(d => d.Key);
    //  var targetGroundIns = targetDomsToMatch.Where(d => d.Value == Mode.In).Select(d => d.Key);
    //  var targetGroundOuts = targetDomsToMatch.Where(d => d.Value == Mode.Out).Select(d => d.Key);
    //  if (myFreeIns.Count() != targetGroundIns.Count() || myFreeOuts.Count() != targetGroundOuts.Count())
    //    return Iterators.Empty<TermReferenceDictionary>();
    //  var targetInPerms = targetGroundIns.Permutations();
    //  var targetOutPerms = targetGroundOuts.Permutations();
    //  var inBindings = targetInPerms.Select(gs => myFreeIns.Zip(gs)).Where(b => b.All(pair => pair.Item1.NameConstraintsAllow(pair.Item2)));
    //  var outBindings = targetOutPerms.Select(gs => myFreeOuts.Zip(gs)).Where(b => b.All(pair => pair.Item1.NameConstraintsAllow(pair.Item2)));
    //  var allBindings = (myFreeIns.Count(), myFreeOuts.Count()) switch
    //  {
    //    (0, _) => outBindings.Select(b => b.Select(pair => new KeyValuePair<ITerm, ITerm>(pair.Item1, pair.Item2))),
    //    (_, 0) => inBindings.Select(b => b.Select(pair => new KeyValuePair<ITerm,ITerm>(pair.Item1,pair.Item2))),
    //    (_, _) => inBindings.Cartesian(outBindings, (ibs, obs) => ibs.Concat(obs).Select(b => new KeyValuePair<ITerm,ITerm>(b.Item1,b.Item2)))
    //  };
    //  var allMaps = allBindings.Select(o => new TermReferenceDictionary(o));
    //  return allMaps;
    //}

    ///// <summary>
    ///// Throws if there are shared names among the two valences.
    ///// </summary>
    //public static Valence Combine(Valence val1, Valence val2)
    //{
    //  return new Valence(val1.Concat(val2));
    //}
  }
}

