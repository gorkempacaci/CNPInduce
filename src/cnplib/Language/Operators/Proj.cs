using System;
using System.Collections.Generic;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{
  public class Proj : IProgram
  {
    /// <summary>
    /// How many out arguments is proj allowed to eliminate. (or inversely, 'introduce' during synthesis)
    /// </summary>
    private const int MAX_ELIMINATED_OUT_ARGS = 1;
    public readonly IProgram Source;
    public readonly ProjectionMap Projection;

    /// <summary>
    /// The valence that lead to this program.
    /// </summary>
    public string DebugValenceString { get; set; }
    /// <summary>
    /// The observations that lead to this program.
    /// </summary>
    public string DebugObservationString { get; set; }

    public Proj(IProgram sourceProgram, ProjectionMap projection)
    {
      this.Source = sourceProgram;
      this.Projection = projection;
    }

    public void ReplaceFree(Free free, ITerm term)
    {
      Source.ReplaceFree(free, term);
    }

    public bool IsClosed => Source.IsClosed;

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public string GetTreeQualifier()
    {
      return "proj(" + Source.GetTreeQualifier() + ")";
    }

    public int GetHeight()
    {
      return Source.GetHeight() + 1;
    }

    public ObservedProgram FindLeftmostHole()
    {
      return Source.FindLeftmostHole();
    }

    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment origEnv)
    {
      var origObservation = origEnv.Root.FindHole();
      if (origObservation.RemainingSearchDepth<2)
        return Array.Empty<ProgramEnvironment>();
      if ((origObservation.Constraints & ObservedProgram.Constraint.NotProjection) == ObservedProgram.Constraint.NotProjection)
        return Array.Empty<ProgramEnvironment>();
      List<ProgramEnvironment> programs = new List<ProgramEnvironment>();
      for (int oi = 0; oi < origObservation.Observations.Length; oi++)
      {
        // if proj has no outs, it cannot have a source with outs. Otherwise proj(id, {a->a}) is produced as a universal relation.
        int max_elim_outs = origObservation.Observations[oi].Valence.Outs.Length == 0 ? 0 : Math.Min(origObservation.RemainingUnboundArguments, MAX_ELIMINATED_OUT_ARGS);     // if proj is 3i1o, source can be 3i1o, 3i2o, 3i3o. all inputs should be projected, and at least one output should be projected, if there are any outputs.
        for (int elim_outs = 0; elim_outs <= max_elim_outs; elim_outs++)
        {
          var env = origEnv.Clone();
          ObservedProgram obs = env.Root.FindHole();

          NameVar[] projNames = obs.Observations[oi].Examples.Names.Clone() as NameVar[];
          // projections map the domains(non-eliminated) of the new observation to the domains of proj expression.
          var projection = new KeyValuePair<NameVar, NameVar>[projNames.Length];
          ProjectionMap projMap = new ProjectionMap(projection);
          var sourceNamesProjected = new NameVar[projection.Length];
          for (int i = 0; i < sourceNamesProjected.Length; i++)
          {
            sourceNamesProjected[i] = env.NameBindings.AddNameVar(null);
            projection[i] = new(sourceNamesProjected[i], projNames[i]);
          }
          var sourceNamesEliminated = Enumerable.Range(0, elim_outs).Select(_ => env.NameBindings.AddNameVar(null)).ToArray();
          var sourceNamesAll = sourceNamesProjected.Concat(sourceNamesEliminated).ToArray();
          var sourceValIns = obs.Observations[oi].Valence.Ins.Select(nv => projMap.GetReverse(nv)).ToArray();
          var sourceValOuts = obs.Observations[oi].Valence.Outs.Select(nv => projMap.GetReverse(nv)).Concat(sourceNamesEliminated).ToArray();
          var sourceValence = new ValenceVar(sourceValIns, sourceValOuts);
          var newTups = new ITerm[obs.Observations[oi].Examples.TuplesCount][];
          var debugInfo = obs.Observations[oi].GetDebugInformation(env);
          for (int ri = 0; ri < obs.Observations[oi].Examples.TuplesCount; ri++)
          {
            newTups[ri] = new ITerm[sourceNamesAll.Length];
            Array.Copy(obs.Observations[oi].Examples.Tuples[ri], newTups[ri], obs.Observations[oi].Examples.ColumnsCount); // old terms in place
            for (int nti = 0; nti < elim_outs; nti++) //new term index
            { // free terms in place
              newTups[ri][obs.Observations[oi].Examples.ColumnsCount + nti] = env.Frees.NewFree();
            }
          }
          var sourceRel = new AlphaRelation(sourceNamesAll, newTups);
          // if allowing new unbound arguments in the source, forcing the source to be an AND is 
          // a way to make sure the predicate expression will be well-formed. Other ways of doing this
          // are more convoluted because for example there's no way to fold
          var constraint = elim_outs > 0 ? ObservedProgram.Constraint.OnlyAndElemLib : ObservedProgram.Constraint.NotProjection;
          var sourceObs = new Observation(sourceRel, sourceValence);
          var sourceObsP = new ObservedProgram(new[] {sourceObs}, obs.RemainingSearchDepth - 1, obs.RemainingUnboundArguments - elim_outs, constraint);
          Proj newProj = new Proj(sourceObsP, projMap);
          (newProj as IProgram).SetDebugInformation(debugInfo);
          var newEnv = env.Clone((obs, newProj));
          programs.Add(newEnv);
        }
      }
      return programs;
    }

  }
}
