//using System;//using System.Collections.Generic;//using System.Reflection;//using CNP.Parsing;//using CNP.Helper;//using System.Linq;//using System.Threading;

//namespace CNP.Language//{//  public abstract class Fold : IProgram//  {
//    private const Mode In = Mode.In;
//    private const Mode Out = Mode.Out;//    public IProgram Recursive { get; init; }

//    public Fold(IProgram rec)
//    {
//      Recursive = rec;
//    }

//    //static Fold()
//    //{
//    //  Valences = FoldValenceSeries.FoldSerieFromArrays(
//    //  new[] { "b0", "as", "b" }, new[] { "a", "b", "ab" },
//    //  new[] {
//    //    (new[]{In, In, Out}, new[]{In, In, Out }),
//    //    (new[]{In, In, Out}, new[]{In, Out, Out }),
//    //    (new[]{In, In, Out}, new[]{Out, In, Out }),
//    //    (new[]{In, In, Out}, new[]{Out, Out, Out }),

//    //    (new[]{Out,In,Out}, new[]{In,Out,Out}),
//    //    (new[]{Out,In,Out}, new[]{Out,Out,Out}),

//    //    (new[]{In, Out, Out}, new[]{Out, In, Out}),
//    //    (new[]{In, Out, Out}, new[]{Out, Out, Out}),

//    //    (new[]{Out, Out, Out}, new[]{Out, Out, Out})
//    //  });
//    //}

//    public readonly static FoldValenceSeries Valences =  FoldValenceSeries.FoldSerieFromArrays(
//      new[] { "b0", "as", "b" }, new[] { "a", "b", "ab" },
//      new[] {
//        (new[]{In, In, Out}, new[]{In, In, Out }),
//        (new[]{In, In, Out}, new[]{In, Out, Out }),
//        (new[]{In, In, Out}, new[]{Out, In, Out }),
//        (new[]{In, In, Out}, new[]{Out, Out, Out }),

//        (new[]{Out,In,Out}, new[]{In,Out,Out}),
//        (new[]{Out,In,Out}, new[]{Out,Out,Out}),

//        (new[]{In, Out, Out}, new[]{Out, In, Out}),
//        (new[]{In, Out, Out}, new[]{Out, Out, Out}),

//        (new[]{Out, Out, Out}, new[]{Out, Out, Out})
//      });


//    /// <summary>
//    /// The valence that lead to this program.
//    /// </summary>
//    public string DebugValenceString { get; set; }
//    /// <summary>
//    /// The observations that lead to this program.
//    /// </summary>
//    public string DebugObservationString { get; set; }//    bool IProgram.IsClosed => Recursive.IsClosed;//    int IProgram.GetHeight()//    {//      return Recursive.GetHeight() + 1;//    }

//    public int GetComplexityExponent()
//    {
//      return Recursive.GetComplexityExponent() + 1;
//    }

//    public abstract string Accept(ICNPVisitor ps);

//    public abstract IProgram Clone(CloningContext cc);

//    public abstract string GetTreeQualifier();


//    public string[] GetGroundNames(NameVarBindings nvb)
//    {
//      return Valences.Names;
//    }

//    ObservedProgram IProgram.FindLeftmostHole()
//    {
//      return Recursive.FindLeftmostHole();
//    }

//    public void ReplaceFree(Free free, ITerm term)
//    {
//      Recursive.ReplaceFree(free, term);
//    }//    protected abstract UnFold GetUnfolder();

//    public RunResult _Run(ExecutionEnvironment env, GroundRelation args)
//    {
//      (short b0, short @as, short b) indices = args.GetNameIndices(env.NameBindings, "b0", "as", "b");
//      var unfolder = GetUnfolder();
//      if (unfolder(args, indices, env.Frees, out var unfoldedTuples))
//      {
//        var recursiveArgs = new GroundRelation(Valences.RecursiveCaseNames, unfoldedTuples);
//        var result = Recursive.Run(env, recursiveArgs);
//        return result;
//      }
//      else throw new Exception("Cannot execute foldX for some reason.");
//    }//    protected delegate Fold CreateFold(IProgram recursive);//    protected delegate bool UnFold(RelationBase foldRel, (short b0, short @as, short b) nameIndices, FreeFactory freeFac, out ITerm[][] pRelation);

//    protected static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment oldEnv, CreateFold newFold, UnFold unfolder)//    {//      ObservedProgram origObservation = oldEnv.Root.FindHole();//      if (origObservation.RemainingSearchDepth<2)//        return Array.Empty<ProgramEnvironment>();
//      var newPrograms = new List<ProgramEnvironment>();//      for (int oi = 0; oi < origObservation.Observations.Length; oi++)
//      {
//        if (origObservation.Observations[oi].Examples.TuplesCount == 0)
//          return Array.Empty<ProgramEnvironment>();
//        Valences.GroundingAlternatives(origObservation.Observations[oi].Valence, oldEnv.NameBindings, out var groundingAlternatives);
//        foreach (var alt in groundingAlternatives)
//        {
//          var newEnv = oldEnv.Clone();
//          var newObs = newEnv.Root.FindHole();
//          var debugInfo = newObs.Observations[oi].GetDebugInformation(newEnv);
//          if (newEnv.NameBindings.TryBindingAllNamesToGround(newObs.Observations[oi].Valence, (ins: alt.ins, outs: alt.outs)))
//          {
//            string[] groundNames = newEnv.NameBindings.GetNamesForVars(newObs.Observations[oi].Examples.Names);
//            short b0 = (short)Array.IndexOf(groundNames, "b0");
//            short @as = (short)Array.IndexOf(groundNames, "as");
//            short b = (short)Array.IndexOf(groundNames, "b");

//            var nameIndices = (b0: b0, @as: @as, b: b);
//            if (unfolder(newObs.Observations[oi].Examples, nameIndices, newEnv.Frees, out var pTuples))
//            {
//              // build p-observation
//              NameVar[] pNames = new[] { newEnv.NameBindings.AddNameVar(Valences.RecursiveCaseNames[0]), newEnv.NameBindings.AddNameVar(Valences.RecursiveCaseNames[1]), newEnv.NameBindings.AddNameVar(Valences.RecursiveCaseNames[2]) };
//              AlphaRelation pRelation = new(pNames, pTuples);
//              ValenceVar pVal = ValenceVar.FromModeIndices(pNames, alt.rec);
//              Observation obs = new Observation(pRelation, pVal);
//              ObservedProgram pObs = new ObservedProgram(new[] { obs }, newObs.RemainingSearchDepth - 1, newObs.RemainingUnboundArguments, ObservedProgram.Constraint.None);
//              // build fold
//              Fold fld = newFold(pObs);
//              (fld as IProgram).SetDebugInformation((debugInfo.valenceString, debugInfo.observationString + $" with order (b0={b0}, as={@as}, b={b})"));
//              var outEnv = newEnv.Clone((newObs, fld));
//              newPrograms.Add(outEnv);
//            }
//          }
//        }//      }//      return newPrograms;//    }//  }  //}