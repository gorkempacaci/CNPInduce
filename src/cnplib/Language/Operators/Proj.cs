using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public class Proj : Program
  {
    private const int N_ELIMINATED_OUT_ARGUMENTS_ALLOWED = 2;
    public readonly Program Source;
    public readonly ProjectionMap Projection;

    public Proj(Program sourceProgram, ProjectionMap projection) : base(sourceProgram.IsClosed)
    {
      this.Source = sourceProgram;
      this.Projection = projection;
    }

    public override int GetHeight()
    {
      return Source.GetHeight() + 1;
    }

    public override void SetAllRootsTo(Program newRoot)
    {
      this.Root = newRoot;
      Source.SetAllRootsTo(newRoot);
    }

    internal override Program Clone(TermReferenceDictionary trd)
    {
      return new Proj(this.Source.Clone(trd), this.Projection.Clone(trd));
    }

    internal override Program CloneAndReplaceObservation(ObservedProgram oldComponent, Program newComponent, TermReferenceDictionary plannedParenthood)
    {
      return new Proj(Source.CloneAndReplaceObservation(oldComponent, newComponent, plannedParenthood), Projection.Clone(plannedParenthood));
    }

    internal override ObservedProgram FindFirstHole()
    {
      return Source.FindFirstHole();
    }

    public static IEnumerable<Program> CreateAtFirstHole(Program originalProgram)
    {
      List<Program> programs = new List<Program>(N_ELIMINATED_OUT_ARGUMENTS_ALLOWED + 1);
      // for each program to be branched to
      for (int n_outs = 0; n_outs < N_ELIMINATED_OUT_ARGUMENTS_ALLOWED; n_outs++)
      {
        TermReferenceDictionary plnprn = new();
        var rootProgram = originalProgram.Clone(plnprn);
        ObservedProgram obs = rootProgram.FindFirstHole();
        // with proj the subtree height will be 1
        if (obs.DTL == 0)
          return Iterators.Empty<Program>();
        // terms are needed for the eliminated domains for the new observation
        Func<IEnumerable<NameVar>, Dictionary<NameVar, Term>> makeFreeTerms = doms => doms.ToDictionary(d => d, _ => new Free() as Term);
        // projections map the domains(non-eliminated) of the new observation to the domains of proj expression.
        var projection = obs.Valence.Keys.ToDictionary(n => NameVar.NewUnbound(), n => n);
        // inverse projection maps the domains of proj to domains of observation
        var invProjection = projection.ToDictionary(kv => kv.Value, kv => kv.Key);
        // proj may have eliminated some domains, these will have free names and Out modes.
        var eliminatedDoms = Enumerable.Range(0, n_outs).ToDictionary(_ => NameVar.NewUnbound(), _ => Mode.Out);
        // function returns a new alpha tuple where the terms are the same but domains are replaced with those in the source.
        Func<AlphaTuple, AlphaTuple> projToSource = patu => new AlphaTuple(patu.Terms.ToDictionary(t => invProjection[t.Key], t => t.Value.Clone(plnprn)).Concat(makeFreeTerms(eliminatedDoms.Keys)));
        var sourceTuples = obs.Observables.Select(projToSource);
        var sourceDoms = obs.Valence.ToDictionary(nv => invProjection[nv.Key], nv => nv.Value).Concat(eliminatedDoms);
        var sourceProgram = new ObservedProgram(sourceTuples, new Valence(sourceDoms), obs.DTL - 1);
        var program = new Proj(sourceProgram, new(projection));
        rootProgram = rootProgram.CloneAndReplaceObservation(obs, program);
        programs.Add(rootProgram);
      }
      return programs;
    }
  }
}
