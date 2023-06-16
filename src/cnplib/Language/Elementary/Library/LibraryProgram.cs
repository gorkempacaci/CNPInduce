using System;
using System.Collections.Generic;
using System.Xml.Linq;
using CNP;
using CNP.Language;

namespace CNP.Language
{
  public abstract class LibraryProgram : ElementaryProgram
  {
    public readonly string Name;

    public LibraryProgram(string nm)
    {
      this.Name = nm;
    }

    public override IProgram Clone(CloningContext cc)
    {
      return CreateNew();
    }

    public override int GetHashCode()
    {
      return 7;
    }

    public override bool Equals(object obj)
    {
      return obj is LibraryProgram li && li.Name == this.Name;
    }

    public override string Accept(ICNPVisitor ps) => ps.Visit(this);

    public override string[] GetGroundNames(NameVarBindings nvb) => Valences.Names;

    protected abstract ElementaryValenceSeries Valences { get; }

    protected abstract LibraryProgram CreateNew();

    protected abstract bool RunElementary(BaseEnvironment env, AlphaRelation args);


    public IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      var obs = env.Root.FindHole();
      var programs = new List<ProgramEnvironment>();
      for (int oi = 0; oi < obs.Observations.Length; oi++)
      {
        Valences.GroundingAlternatives(obs.Observations[oi].Valence, env.NameBindings, out var alternatives);
        foreach (var alt in alternatives)
        {
          var newEnv = env.Clone();
          var newObs = newEnv.Root.FindHole();
          if (newEnv.NameBindings.TryBindingAllNamesToGround(newObs.Observations[oi].Valence, alt))
          {
            if (!RunElementary(newEnv, newObs.Observations[oi].Examples))
            {
              return Array.Empty<ProgramEnvironment>(); ;
            }
            var outEnv = newEnv.Clone((newObs, CreateNew()));
            programs.Add(outEnv);
          }
        }
      }
      return programs;
    }

  }
}

