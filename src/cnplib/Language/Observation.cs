namespace CNP.Language
{
  /// <summary>
  /// A set of examples with a valence. Examples are a sample from the extension of a predicate expression which has the given valence.
  /// </summary>
  public class Observation
  {
    /// <summary>
    /// Examples are conjunctive. All have to hold 
    /// </summary>
    public readonly AlphaRelation Examples;
    public readonly ValenceVar Valence;
    private readonly short[] _indicesOfInArgs;
    private readonly short[] _indicesOfOutArgs;

    public Observation(AlphaRelation examples, ValenceVar valence)
    {
      (_indicesOfInArgs, _indicesOfOutArgs) = valence.GetIndicesOfInsOrOutsIn(examples.Names);
      Examples = examples;
      Valence = valence;
    }


    public (string valenceString, string observationString) GetDebugInformation(BaseEnvironment env)
    {
      var db = new DebugPrinter(env.NameBindings);
      var valence = Valence.Accept(db);
      var observ = Examples.Accept(db);
      return (valence, observ);
    }


    /// <summary>
    /// Returns true if all arguments where Mode is IN are ground terms in the first tuple of this observation. Doesn't check the remaining tuples because they might becomes ground as the first one is unified.
    /// </summary>
    public bool IsAllINArgumentsGroundForFirstTuple()
    {
      var firstTuple = Examples.Tuples[0];
      for (int i = 0; i < _indicesOfInArgs.Length; i++)
      {
        if (!firstTuple[_indicesOfInArgs[i]].IsGround())
          return false;
      }
      return true;
    }

    /// <summary>
    /// Returns true if all out arguments of all tuples are ground.
    /// </summary>
    public bool IsAllOutArgumentsGround()
    {
      for (int i = 0; i < Examples.TuplesCount; i++)
      {
        var tuple = Examples.Tuples[i];
        for (int oii = 0; oii < _indicesOfOutArgs.Length; oii++)
          if (!tuple[_indicesOfOutArgs[oii]].IsGround())
            return false;
      }

      return true;
    }

    public Observation Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }
  }
}
