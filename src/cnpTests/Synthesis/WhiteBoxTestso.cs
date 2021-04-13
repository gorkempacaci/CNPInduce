using System;
using CNP.Language;
using CNP.Helper.EagerLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Synthesis
{
  /// <summary>
  /// These tests explicitly give variables for synthesis and follow them through a specific search path.
  /// </summary>
  [TestClass]
  public class WhiteBoxTests : TestBase
  {
    private FoldR get_open_foldr_program()
    {
      Free f1 = new(), f2 = new(), f3 = new(), f4 = new();
      NameVar a = new("a"), b = new("b"), ab = new("ab");
      AlphaTuple[] consTups = {   new((a, cnst(1)), (b, f1), (ab, list(1, 2, 3))),
                                  new((a, cnst(2)), (b, f2), (ab, f1)),
                                  new((a, cnst(3)), (b, f3), (ab, f2)) };
      Valence consVal = new Valence((a, Mode.In), (b, Mode.Out), (ab, Mode.In));
      NameVar ida = new("a"), idb = new("b");
      AlphaTuple[] idTups = { new((ida, list()), (idb, f3)) };
      Valence idVal = new Valence((ida, Mode.In), (idb, Mode.In));
      ObservedProgram consObs = new ObservedProgram(consTups, consVal, 3);
      ObservedProgram idObs = new ObservedProgram(idTups, idVal, 3);
      FoldR partialFoldProgram = foldr(consObs, idObs);
      return partialFoldProgram;
    }

    [TestMethod]
    public void Syn_Foldr_Cons_Id()
    {
      FoldR open_foldr = get_open_foldr_program();
      open_foldr = open_foldr.CloneAtRoot() as FoldR;
      var progs = Cons.CreateAtFirstHole(open_foldr);
      FoldR open_foldr_cons = progs.First() as FoldR;
      Assert.AreEqual(cons, open_foldr_cons.Recursive);
      ObservedProgram idObs2 = open_foldr_cons.Base as ObservedProgram;
      Assert.AreEqual(list(), idObs2.Observables.First()["a"]);
      Assert.AreEqual(list(), idObs2.Observables.First()["b"]);
      var progs2 = Id.CreateAtFirstHole(progs.First());
      FoldR closed_foldr = progs2.First() as FoldR;
      Assert.AreEqual(id, closed_foldr.Base);
    }


    [TestMethod]
    public void Syn_Foldr_Proj_Cons_Proj_Id()
    {
      FoldR open_foldr = get_open_foldr_program(); // foldr(..., ...)

      var progs = Proj.CreateAtFirstHole(open_foldr); // foldr(proj(...), ...)
      FoldR open_foldr_proj = progs.First() as FoldR;
      Assert.IsTrue(open_foldr_proj.Recursive is Proj, "Recursive case should be proj.");

      var progs2 = Cons.CreateAtFirstHole(open_foldr_proj); //foldr(proj(cons), ...)
      FoldR open_foldr_proj_cons = progs2.Skip(2).First() as FoldR; // THIS IS THE CORRECT CONS FOR foldr(cons,id).
      Assert.AreEqual(cons, ((open_foldr_proj_cons.Recursive as Proj).Source));

      var progs3 = Proj.CreateAtFirstHole(open_foldr_proj_cons); //foldr(proj(cons), proj(...))
      FoldR open_foldr_proj2 = progs3.First() as FoldR;
      Assert.IsTrue(open_foldr_proj2.Base is Proj, "Base case should be proj.");

      var idObs = (open_foldr_proj2.Base as Proj).Source as ObservedProgram;
      var idNameA = idObs.Valence.First().Key;
      var idNameB = idObs.Valence.Skip(1).First().Key;
      Assert.AreEqual(list(), idObs.Observables.First()[idNameA], "obs_id.a should be []");
      Assert.AreEqual(list(), idObs.Observables.First()[idNameB], "obs_id.b should be []");

      var progs4 = Id.CreateAtFirstHole(open_foldr_proj2); //foldr(proj(cons), proj( ! id))
      FoldR closed_foldr_proj_id = progs4.First() as FoldR;
      Assert.AreEqual(id, (closed_foldr_proj_id.Base as Proj).Source);
    }

    [TestMethod]
    public void Syn_Foldr_Proj_Cons_Proj_Id_Fails()
    {
      FoldR open_foldr = get_open_foldr_program(); // foldr(..., ...)

      var progs = Proj.CreateAtFirstHole(open_foldr); // foldr(proj(...), ...)
      FoldR open_foldr_proj = progs.First() as FoldR;
      Assert.IsTrue(open_foldr_proj.Recursive is Proj, "Recursive case should be proj.");

      var progs2 = Cons.CreateAtFirstHole(open_foldr_proj); //foldr(proj(cons), ...)
      FoldR open_foldr_proj_cons = progs2.First() as FoldR; // SHOULD FAIL BECAUSE THIS CONS IS THE WRONG ONE
      Assert.AreEqual(cons, ((open_foldr_proj_cons.Recursive as Proj).Source));

      var progs3 = Proj.CreateAtFirstHole(open_foldr_proj_cons); //foldr(proj(cons), proj(...))
      FoldR open_foldr_proj2 = progs3.First() as FoldR;
      Assert.IsTrue(open_foldr_proj2.Base is Proj, "Base case should be proj.");

      var idObs = (open_foldr_proj2.Base as Proj).Source as ObservedProgram;
      var idNameA = idObs.Valence.First().Key;
      var idNameB = idObs.Valence.Skip(1).First().Key;
      Assert.AreEqual(list(), idObs.Observables.First()[idNameA], "obs_id.a should be []");
      Assert.AreEqual(list(3,2,1,1,2,3), idObs.Observables.First()[idNameB], "obs_id.b should be [3,2,1,1,2,3]");

      var progs4 = Id.CreateAtFirstHole(open_foldr_proj2); //foldr(proj(cons), proj( ! id))
      Assert.IsFalse(progs4.Any(), "'id' program should not match.");
    }


  }
}
