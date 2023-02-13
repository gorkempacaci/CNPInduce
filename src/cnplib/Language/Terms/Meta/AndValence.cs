using System;
using System.Collections.Generic;
using System.Linq;
using CNP.Helper;
using System.Net;

namespace CNP.Language
{
  public record AndValence(ValenceVar OperatorValence, ValenceVar LHValence, ValenceVar RHValence, NameVar[] OnlyLHNames, NameVar[] OnlyRHNames)
  {

    public record ProtoAndValence(Dictionary<NameVar, Mode> Operator, Dictionary<NameVar, Mode> LH, Dictionary<NameVar, Mode> RH)
    {}


    /// <summary>
    /// Given the operation valence for an and/or expression, generates combinations of p,q valences, considering well-modedness constraints.
    /// </summary>
    /// OPTIMIZE: extremely inefficient. Both algorithmically and QOP-wise.
    public static IEnumerable<AndValence> Generate(ValenceVar opVal)
    {
      List<AndValence> allValences = new ((opVal.Ins.Length*2+opVal.Outs.Length)*2);
      var allNamesModes = VVToNamesToModes(opVal);
      // opval ~ {a:in, b:in, c:out, d:out}
      var joinedNamesAlternatives = Mathes.Combinations(allNamesModes.Keys);
      // joinedNames ~ {b, c}
      foreach(var joinedNames in joinedNamesAlternatives)
      {
        // distinctNames ~ {a, d}
        var unjoined = allNamesModes.Keys.Except(joinedNames);
        var onlyPNamesAlternatives = Mathes.Subsets(unjoined);
        // onlyPNames can be {}, {a}, {d}, {a,d}
        foreach(var onlyPNames in onlyPNamesAlternatives)
        {
          // (onlyP, onlyQ) ~ ({}, {a, d}), ({d}, {a})
          var onlyQNames = unjoined.Except(onlyPNames);
          var newValences = make_wellmoded_valences(allNamesModes, onlyPNames, onlyQNames, joinedNames);
          allValences.AddRange(newValences);
        }
      }
      return allValences;
    }


    public static ProtoAndValence Combine(ProtoAndValence a, ProtoAndValence b)
    {
      return new ProtoAndValence(Combine(a.Operator, b.Operator),
        Combine(a.LH, b.LH), Combine(a.RH, b.RH));
    }

    public static Dictionary<NameVar, Mode> Combine(Dictionary<NameVar, Mode> a, Dictionary<NameVar, Mode> b)
    {
      return new Dictionary<NameVar, Mode>(a.Concat(b));
    }

    /// <summary>
    /// Makes a new array that contains Ins and Outs in that order.
    /// </summary>
    public static IDictionary<NameVar, Mode> VVToNamesToModes(ValenceVar vv)
    {
      Dictionary<NameVar, Mode> dic = new Dictionary<NameVar, Mode>(vv.Ins.Length + vv.Outs.Length);
      foreach (var ni in vv.Ins)
        dic.Add(ni, Mode.In);
      foreach (var no in vv.Outs)
        dic.Add(no, Mode.Out);
      return dic;
    }

    /// <summary>
    /// takes an operation valence, names for the p and q operators, and generates well-moded functional valences including modes for p and q
    /// </summary>
    private static IEnumerable<AndValence> make_wellmoded_valences(IDictionary<NameVar,Mode> namesToModes, IEnumerable<NameVar> onlyPNames, IEnumerable<NameVar> onlyQNames, IEnumerable<NameVar> sharedNames)
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
      var sharedNamesPartValencesPerName = sharedNames.Select(sn => modesForSharedNames[namesToModes[sn]].Select(lr => new ProtoAndValence(new() { { sn, namesToModes[sn] } }, new() { { sn, lr.Item1 } }, new() { { sn, lr.Item2 } })));
      // [{a:in,b:out}->{a:in,b:out}->{a:in,b:out}, ...]
      var sharedNamesPartValences = sharedNamesPartValencesPerName.Aggregate((a, b) => Mathes.Cartesian(a, b, Combine));

      IEnumerable<ProtoAndValence> allValences = sharedNamesPartValences;
      if (onlyPNames.Any())
      {
        // [[{a:in}, {a:out}], [{b:in}], ...]
        var onlyPNamesPartValencesPerName = onlyPNames.Select(pn => modesForExclusiveNames[namesToModes[pn]].Select(m => new Dictionary<NameVar,Mode>() { { pn, m } }));
        // [{a:in,b:out}->{a:in,b:out}->{a:in,b:out}, ...]
        var onlyPNamesPartValences = onlyPNamesPartValencesPerName.Aggregate((a, b) => Mathes.Cartesian(a, b, Combine));
        allValences = Mathes.Cartesian(onlyPNamesPartValences, allValences, (p, all) => new ProtoAndValence(all.Operator, Combine(p, all.LH), all.RH)); // all was opval
      }
      if (onlyQNames.Any())
      {
        var onlyQNamesPartValencesPerName = onlyQNames.Select(qn => modesForExclusiveNames[namesToModes[qn]].Select(m => new Dictionary<NameVar, Mode>() { { qn, m } }));
        var onlyQNamesPartValences = onlyQNamesPartValencesPerName.Aggregate((a, b) => Mathes.Cartesian(a, b, Combine)); ;
        allValences = Mathes.Cartesian(onlyQNamesPartValences, allValences, (q, all) => new ProtoAndValence(all.Operator, all.LH, Combine(q, all.RH))); // all was opval
      }

      var andValences = allValences.Select(pv => new AndValence(ValenceVar.FromDict(pv.Operator), ValenceVar.FromDict(pv.LH), ValenceVar.FromDict(pv.RH), onlyPNames.ToArray(), onlyQNames.ToArray()));
      return andValences;
    }


  }
}