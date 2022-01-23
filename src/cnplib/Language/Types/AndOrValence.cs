using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using CNP.Helper;
using CNP.Display;

namespace CNP.Language
{
  public class AndOrValence : FunctionalValence
  {
    public readonly Valence LHDoms;
    public readonly Valence RHDoms;

    public AndOrValence(Valence lhDoms, Valence rhDoms, Valence domains) : base(domains)
    {
      LHDoms = lhDoms;
      RHDoms = rhDoms;
    }

    public override bool IsGround()
    {
      return base.IsGround() && LHDoms.IsGround() && RHDoms.IsGround();
    }

    public override bool Equals(object obj)
    {
      if (obj is AndOrValence other)
      {
        return base.Equals(obj) && LHDoms.Equals(other.LHDoms) && RHDoms.Equals(other.RHDoms);
      } else return false;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    /// <summary>
    /// Given the operation valence for an and/or expression, generates combinations of p,q valences, considering well-modedness constraints.
    /// </summary>
    /// TODO: inefficient. use truth table logic and bitwise ops to find the combinations?
    public static IEnumerable<AndOrValence> Generate(Valence opVal)
    {
      List<AndOrValence> allValences = new List<AndOrValence>(opVal.Count*5);
      // opval ~ {a:in, b:in, c:out, d:out}
      var joinedNamesAlternatives = Mathes.Combinations(opVal.Names);
      // joinedNames ~ {b, c}
      foreach(var joinedNames in joinedNamesAlternatives)
      {
        // distinctNames ~ {a, d}
        var unjoined = opVal.Names.Except(joinedNames);
        var onlyPNamesAlternatives = Mathes.Subsets(unjoined);
        // onlyPNames can be {}, {a}, {d}, {a,d}
        foreach(var onlyPNames in onlyPNamesAlternatives)
        {
          // (onlyP, onlyQ) ~ ({}, {a, d}), ({d}, {a})
          var onlyQNames = unjoined.Except(onlyPNames);
          var newValences = make_wellmoded_valences(opVal, onlyPNames, onlyQNames, joinedNames);
          allValences.AddRange(newValences);
        }
      }
      return allValences;
    }
    /// <summary>
    /// takes an operation valence, names for the p and q operators, and generates well-moded functional valences including modes for p and q
    /// </summary>
    private static IEnumerable<AndOrValence> make_wellmoded_valences(Valence opVal, IEnumerable<NameVar> onlyPNames, IEnumerable<NameVar> onlyQNames, IEnumerable<NameVar> sharedNames)
    {
      var modesForSharedNames = new Dictionary<Mode, IEnumerable<(Mode, Mode)>>
      {
        [Mode.In] = new[]{  (Mode.In, Mode.In), // op in, p in, q in
                            (Mode.In, Mode.Out),
                            (Mode.Out, Mode.In),
                            (Mode.Out, Mode.Out) },
        [Mode.Out] = new[]{ (Mode.Out, Mode.In), // op out, p out, q in
                            (Mode.Out, Mode.Out) }
      };
      var modesForExclusiveNames = new Dictionary<Mode, IEnumerable<Mode>>
      {
        [Mode.In] = new[] { Mode.In, // op in, p/q in,
                            Mode.Out }, // op in, p/q out
        [Mode.Out] = new[] { Mode.Out } // op out/ o/q out
      };

      //for each shared name, a partial andorvalence for that name, containing each op-p-q combination of modes for that domain.
      // [[({a:in} -> {a:in} -> {a:in}),...], [({b:out} -> {b:out -> {b:out}),...], ...]
      var sharedNamesPartValencesPerName = sharedNames.Select(sn => modesForSharedNames[opVal[sn]].Select(lr => new AndOrValence(new Valence((sn, lr.Item1)), new Valence((sn, lr.Item2)), new Valence((sn, opVal[sn])))));
      // [{a:in,b:out}->{a:in,b:out}->{a:in,b:out}, ...]
      var sharedNamesPartValences = sharedNamesPartValencesPerName.Aggregate((a, b) => Mathes.Cartesian(a, b, AndOrValence.Combine));

      IEnumerable<AndOrValence> allValences = sharedNamesPartValences;
      if (onlyPNames.Any())
      {
        // [[{a:in}, {a:out}], [{b:in}], ...]
        var onlyPNamesPartValencesPerName = onlyPNames.Select(pn => modesForExclusiveNames[opVal[pn]].Select(m => new Valence((pn, m))));
        // [{a:in,b:out}->{a:in,b:out}->{a:in,b:out}, ...]
        var onlyPNamesPartValences = onlyPNamesPartValencesPerName.Aggregate((a, b) => Mathes.Cartesian(a, b, Valence.Combine));
        allValences = Mathes.Cartesian(onlyPNamesPartValences, allValences, (p, all) => new AndOrValence(Valence.Combine(p, all.LHDoms), all.RHDoms, opVal));
      }
      if (onlyQNames.Any())
      {
        var onlyQNamesPartValencesPerName = onlyQNames.Select(qn => modesForExclusiveNames[opVal[qn]].Select(m => new Valence((qn, m))));
        var onlyQNamesPartValences = onlyQNamesPartValencesPerName.Aggregate((a, b) => Mathes.Cartesian(a, b, Valence.Combine)); ;
        allValences = Mathes.Cartesian(onlyQNamesPartValences, allValences, (q, all) => new AndOrValence(all.LHDoms, Valence.Combine(q, all.RHDoms), opVal));
      }

      return allValences;
    }


    /// <summary>
    /// Combines name-mode pairs in each part of the andorvalence into one.
    /// Combine({a:in}->{a:in}->{a:in}, {b:out}->{c:out}->{d:out}) returns {a:in,b:out}->{a:in,c:out}->{a:in,d:out}.
    /// </summary>
    public static AndOrValence Combine(AndOrValence a, AndOrValence b)
    {
      return new AndOrValence(Valence.Combine(a.LHDoms, b.LHDoms), Valence.Combine(a.RHDoms, b.RHDoms), Valence.Combine(a, b));
    }
  }
}


/*

a, b, c, d


a, b, c   AND   b, c, d


for n,m in {a, b, c, d}
  if m == in
    if n in p and not in q
      {n, in}, {n, out} \in pnm
    if n in q and not in p
      {n, in}, {n, out} \in qnm
    if n in q and in p
      {n, in} \in pnm and {n, 


 
 
 */
