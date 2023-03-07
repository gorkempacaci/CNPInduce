using Microsoft.VisualStudio.TestTools.UnitTesting;
using CNP.Language;
using CNP.Helper;
using System.Diagnostics;
using System.Linq;
using System;
using CNP;

namespace Language
{
  [TestClass]
  public class Unification : TestBase
  {
    NameVarBindings names;
    FreeFactory frees;
    Free fX, fY, fZ;
    NameVar nA, nB, nC, nD;
    NameVar nU, nV;
    NameVar n1, n2, n3, n4;
    ConstantTerm cnst_a = new ConstantTerm("a");
    ConstantTerm cnst_u = new ConstantTerm("u");
    ConstantTerm cnst_i = new ConstantTerm("i");
    ConstantTerm cnst_o = new ConstantTerm("o");
    ConstantTerm cnst1 = new ConstantTerm(1);
    PrettyStringer pretty;

    [TestInitialize]
    public void SetupEnvironment()
    {
      names = new();
      frees = new();
      fX = frees.NewFree();
      fY = frees.NewFree();
      fZ = frees.NewFree();
      nA = names.AddNameVar("a");
      nB = names.AddNameVar("b");
      nC = names.AddNameVar("c");
      nD = names.AddNameVar("d");
      nU = names.AddNameVar("u");
      nV = names.AddNameVar("v");
      n1 = names.AddNameVar("1");
      n2 = names.AddNameVar("2");
      n3 = names.AddNameVar("3");
      n4 = names.AddNameVar("4");
      pretty = new PrettyStringer(names);
    }

    [TestMethod]
    public void ReplaceFrees_InComplexProgram()
    {
      var obs1Rel = new AlphaRelation(new[] { nA, nB }, new[] { new ITerm[] { fX, fY }, new ITerm[] { constterm("h"), constterm("h") } });
      var obs1Val = new ValenceVar(new[] { nA }, new[] { nB });
      var obs1 = new ObservedProgram(obs1Rel, obs1Val, 1, ObservedProgram.Constraint.None);

      var obs2Rel = new AlphaRelation(new[] { nB, nA }, new[] { new ITerm[] { fY, constterm("j") }, new ITerm[] { constterm("h"), constterm("h") } });
      var obs2Val = new ValenceVar(new[] { nB }, new[] { nA });
      var obs2 = new ObservedProgram(obs2Rel, obs2Val, 1, ObservedProgram.Constraint.None);

      var and = new And(proj(obs1, (nA, nU), (nB, nV)), proj(obs2, (nB, nU), (nA, nV)));

      var penv = new ProgramEnvironment(and, names, frees);

      Assert.AreEqual(fY, (((Proj)((And)penv.Root).RHOperand).Source as ObservedProgram).Observables.Tuples[0][0]);

      penv.ReplaceFree(fY, constterm("j"));

      Assert.AreEqual(constterm("j"), (((Proj)((And)penv.Root).RHOperand).Source as ObservedProgram).Observables.Tuples[0][0]);

    }

    [TestMethod]
    public void ReplaceFrees_InComplexTerm()
    {
      var complexTerm = list(list(), list(fX, fY), fX, list(list(list(), fX), fX));
      var afterReplacement = complexTerm.GetFreeReplaced(fX, constterm(1));
      var expectedTerm = list(list(), list(constterm(1), fY), constterm(1), list(list(list(), constterm(1)), constterm(1)));
      Assert.AreEqual(expectedTerm.Accept(pretty), afterReplacement.Accept(pretty));
    }

    [TestMethod]
    public void ReplaceFrees_AlphaRelation()
    {
      FreeFactory fact = new();
      NameVarBindings nms = new();
      NameVar a = nms.AddNameVar("a");
      
      Free f = fact.NewFree();
      ITerm[][] tups = new ITerm[][] { new ITerm[] { f } };
      AlphaRelation rel = new AlphaRelation(new[] { a }, tups);

      Assert.AreEqual(f, rel.Tuples[0][0]);
      rel.ReplaceFreeInPlace(f, constterm(9));
      Assert.AreEqual(constterm(9), rel.Tuples[0][0]);
    }

    [TestMethod]
    public void ReplaceFrees_Observation()
    {
      FreeFactory fact = new();
      NameVarBindings nms = new();
      NameVar a = nms.AddNameVar("a");

      Free f = fact.NewFree();
      ITerm[][] tups = new ITerm[][] { new ITerm[] { f } };
      AlphaRelation rel = new AlphaRelation(new[] { a }, tups);

      ValenceVar vv = new ValenceVar(new[] { a }, Array.Empty<NameVar>());
      ObservedProgram obs = new ObservedProgram(rel, vv, 2, ObservedProgram.Constraint.None);

      Assert.AreEqual(f, obs.Observables.Tuples[0][0]);
      obs.ReplaceFree(f, constterm(9));
      Assert.AreEqual(constterm(9), obs.Observables.Tuples[0][0]);
    }


    [TestMethod]
    public void ReplaceFrees_Penv_Observation()
    {
      FreeFactory fact = new();
      NameVarBindings nms = new();
      NameVar a = nms.AddNameVar("a");

      Free f = fact.NewFree();
      ITerm[][] tups = new ITerm[][] { new ITerm[] { f } };
      AlphaRelation rel = new AlphaRelation(new[] { a }, tups);

      ValenceVar vv = new ValenceVar(new[] { a }, Array.Empty<NameVar>());
      ObservedProgram obs = new ObservedProgram(rel, vv, 2, ObservedProgram.Constraint.None);

      ProgramEnvironment penv = new ProgramEnvironment(obs, nms, fact);

      Assert.AreEqual(f, (penv.Root as ObservedProgram).Observables.Tuples[0][0]);
      penv.ReplaceFree(f, constterm(9));
      Assert.AreEqual(constterm(9), (penv.Root as ObservedProgram).Observables.Tuples[0][0]);
    }

    [TestMethod]
    public void ReplaceFrees_Penv_Proj_Observation()
    {
      FreeFactory fact = new();
      NameVarBindings nms = new();
      NameVar a = nms.AddNameVar("a");

      Free f = fact.NewFree();
      ITerm[][] tups = new ITerm[][] { new ITerm[] { f } };
      AlphaRelation rel = new AlphaRelation(new[] { a }, tups);

      ValenceVar vv = new ValenceVar(new[] { a }, Array.Empty<NameVar>());
      ObservedProgram obs = new ObservedProgram(rel, vv, 2, ObservedProgram.Constraint.None);

      Proj projProg = proj(obs, (a, a));

      ProgramEnvironment penv = new ProgramEnvironment(projProg, nms, fact);

      Assert.AreEqual(f, (((Proj)penv.Root).Source as ObservedProgram).Observables.Tuples[0][0]);
      penv.ReplaceFree(f, constterm(9));
      Assert.AreEqual(constterm(9), (((Proj)penv.Root).Source as ObservedProgram).Observables.Tuples[0][0]);
    }


    [TestMethod]
    public void ReplaceFrees_Penv_And_Proj_Observation()
    {
      FreeFactory fact = new();
      NameVarBindings nms = new();
      NameVar a = nms.AddNameVar("a");

      Free f = fact.NewFree();
      ITerm[][] tups = new ITerm[][] { new ITerm[] { f } };
      AlphaRelation rel = new AlphaRelation(new[] { a }, tups);

      ValenceVar vv = new ValenceVar(new[] { a }, Array.Empty<NameVar>());
      ObservedProgram obs = new ObservedProgram(rel, vv, 2, ObservedProgram.Constraint.None);

      Proj projProg = proj(obs, (a, a));
      And andProg = and(projProg, projProg);

      ProgramEnvironment penv = new ProgramEnvironment(andProg, nms, fact);

      Assert.AreEqual(f, (((Proj)((And)penv.Root).RHOperand).Source as ObservedProgram).Observables.Tuples[0][0]);
      penv.ReplaceFree(f, constterm(9));
      Assert.AreEqual(constterm(9), (((Proj)((And)penv.Root).RHOperand).Source as ObservedProgram).Observables.Tuples[0][0]);
    }


    [TestMethod]
    public void UnifyValenceVar_WithValenceSeries()
    {
      ElementaryValenceSeries IdValences =
      ElementaryValenceSeries.SeriesFromArrays(new[] { "a", "b" },
                                    new[]
                                    {
                                      new[]{  Mode.In,  Mode.In},
                                      new[]{  Mode.In,  Mode.Out},
                                      new[]{  Mode.Out, Mode.In}
                                    });

      var vv = new ValenceVar(ins: new[] { nA }, outs: new[] { nB });
      IdValences.GroundingAlternatives(vv, names, out var alternatives);
      Assert.IsTrue(alternatives.Any());
      var expectedAlts = new (string[] ins, string[] outs)[] { (ins: new[] { "a" }, outs: new[] { "b" }) };
      Assert.AreEqual(1, alternatives.Count());
      CollectionAssert.AreEqual(expectedAlts[0].ins, alternatives[0].ins);
      CollectionAssert.AreEqual(expectedAlts[0].outs, alternatives[0].outs);
  }

    private void assert_terms_unify(ITerm origTerm, ITerm unifierTerm, string origTermAfterUniExpected, string message)
    {
      AlphaRelation rel = new AlphaRelation(new[] { nA }, new ITerm[][] { new[] { origTerm } });
      ValenceVar vv = new ValenceVar(new[] { nA }, Array.Empty<NameVar>());
      ObservedProgram obs = new ObservedProgram(rel, vv, 1, ObservedProgram.Constraint.None);
      ProgramEnvironment env = new ProgramEnvironment(obs, names, frees);
      ITerm[] unifierTerms = new[] { unifierTerm };
      bool success = env.UnifyInPlace(rel.Tuples[0], unifierTerms);
      Assert.IsTrue(success, "Terms should unify.");
      PrettyStringer ps = new PrettyStringer(names);
      string actualTermString = rel.Tuples[0][0].Accept(ps);
      Assert.AreEqual(origTermAfterUniExpected, actualTermString, "Term should unify as expected.");
    }

    [TestMethod]
    public void ListsWithComplexTermsUnify()
    {
      ConstantTerm a = new ConstantTerm("a"),
          u = new ConstantTerm("u"),
          i = new ConstantTerm("i"),
          o = new ConstantTerm("o");
      ITerm site =  list(list(fX, a), u, fX, list(i, fY, fX), fZ) ;
      ITerm other =  list(list(fZ, fY), u, fX, list(i, fY, fX), o) ;
      assert_terms_unify(site, other, "[['o', 'a'], 'u', 'o', ['i', 'a', 'o'], 'o']", "Lists with complex terms should unify.");
    }

    [TestMethod]
    public void ListUnifiesWithConsToFormUngroundTerm()
    {
      FreeFactory fact = new();
      ITerm left =  list(constterm(1), fact.NewFree(), constterm(3)) ;
      ITerm right =  cns(constterm(1), fact.NewFree()) ;
      assert_terms_unify(left, right, "[1, F0, 3]", "List should unify with complex list with frees.");
    }

    [TestMethod]
    public void ListUnifiesWithGroundList()
    {
      ITerm left = list(fX, fY, fZ);
      ITerm right = list(constterm(2), constterm(4), constterm(6));
      assert_terms_unify(left, right, "[2, 4, 6]", "List should unify with complex list with frees.");
    }


    [TestMethod]
    public void ListUnifiesWithConsToFormGroundTerm()
    {
      FreeFactory fact = new();
      Free a = fact.NewFree();
      Free b = fact.NewFree();
      ITerm left = list(a, list(constterm("u"), a, constterm("y")), constterm(3));
      ITerm right = cns(constterm(1), b);
      assert_terms_unify(left, right, "[1, ['u', 1, 'y'], 3]", "List should unify with cons to ground");
    }

  }

}