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

    public Proj(Program sourceProgram, ProjectionMap projection)
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

    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgram)
    {
      List<ObservedProgram> programs = new List<ObservedProgram>(N_ELIMINATED_OUT_ARGUMENTS_ALLOWED + 1);
      // for each program to be branched to
      for (int n_outs = 0; n_outs < N_ELIMINATED_OUT_ARGUMENTS_ALLOWED; n_outs++)
      {
        TermReferenceDictionary plnprn = new();
        rootProgram = rootProgram.Clone(plnprn);
        ObservedProgram obs = rootProgram.FindFirstHole();
        // with proj the subtree height will be 1
        if (obs.DTL == 0)
          return Iterators.Empty<Program>();
        var eliminatedDoms = Enumerable.Range(0, n_outs).Select(_ => new KeyValuePair<NameVar, Mode>(NameVar.NewUnbound(), Mode.Out));
        var sourceDoms = obs.Domains.Clone(plnprn).Concat(eliminatedDoms);
        Func<IEnumerable<KeyValuePair<NameVar, Mode>>, IEnumerable<KeyValuePair<NameVar, Term>>> makeFreeTermsFor =
          v => v.Select(d => new KeyValuePair<NameVar, Term>(d.Key, new Free()));
        var sourceTuples = obs.Observables.Select(atu => new AlphaTuple(atu.Clone(plnprn).Terms.Concat(makeFreeTermsFor(eliminatedDoms))));
        programs.Add(new ObservedProgram(sourceTuples, new Valence(sourceDoms), obs.DTL - 1));
      }
      return programs;
    }
  }
}
