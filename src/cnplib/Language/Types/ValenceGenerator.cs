//using System;
//using System.Collections.Generic;
//using CNP.Helper.EagerLinq;
//using CNP.Helper;
//using CNP.Language;

//using NameMode = System.Collections.Generic.KeyValuePair<CNP.Language.NameVar, CNP.Language.Mode>;

//namespace Types
//{
//  public class ValenceGenerator
//  {
//    public ValenceGenerator()
//    {
//    }



//    (Mode, Mode)[] modesForSharedInArgument = new[]{
//                            (Mode.In, Mode.In), // op in, p in, q in
//                            (Mode.In, Mode.Out),
//                            (Mode.Out, Mode.In),
//                            (Mode.Out, Mode.Out) };
//    (Mode, Mode)[] modesForSharedOutArgument = new[]{
//                            (Mode.Out, Mode.In), // op out, p out, q in
//                            (Mode.Out, Mode.Out) };
//    Mode[] modesForExclusiveArgumentIn = new[] {
//                            Mode.In, // op in, p/q in,
//                            Mode.Out }; // op in, p/q out
//    Mode[] modesForExclusiveArgumentOut = new[] { Mode.Out }; // op out/ o/q out

//    private void generateForAnd(Valence andOpValence)
//    {

//    }

//    private IEnumerable<(Valence OnlyP, Valence P, Valence Shared, Valence Q, Valence OnlyQ)> generateNameOptionsFor(Valence andValence)
//    {

//      foreach(NameMode nm in andValence)
//      {
//        var nameModesIfShared = OpPQ_forNameMode(nm, isShared:true);
//        var nameModesIfNotShared = OpPQ_forNameMode(nm, isShared: false);


//      }

//      return null;
//    }

//    private IEnumerable<(NameMode, NameMode?, NameMode?)> OpPQ_forNameMode(NameMode nm, bool isShared)
//    {
//      var name = nm.Key;
//      if (isShared)
//      {
//        if (nm.Value == Mode.In)
//        {
//          yield return new(new(name, Mode.In), new(name, Mode.In), new(name, Mode.In));
//          yield return new(new(name, Mode.In), new(name, Mode.In), new(name, Mode.Out));
//          yield return new(new(name, Mode.In), new(name, Mode.Out), new(name, Mode.In));
//          yield return new(new(name, Mode.In), new(name, Mode.Out), new(name, Mode.Out));
//        }
//        else
//        {
//          yield return new(new(name, Mode.Out), new(name, Mode.Out), new(name, Mode.In));
//          yield return new(new(name, Mode.Out), new(name, Mode.Out), new(name, Mode.Out));
//        }
//      }
//      else // EXCLUSIVE to either P or Q
//      {
//        if (nm.Value == Mode.In)
//        {
//          yield return new(new(name, Mode.In), new(name, Mode.In), null); // op IN, exclusive to P
//          yield return new(new(name, Mode.In), new(name, Mode.Out), null);
//          yield return new(new(name, Mode.In), null, new(name, Mode.In)); // op IN, exclusive to Q
//          yield return new(new(name, Mode.In), null, new(name, Mode.Out));
//        }
//        else
//        {
//          yield return new(new(name, Mode.Out), new(name, Mode.Out), null); // op OUT, exclusive to P
//          yield return new(new(name, Mode.Out), null, new(name, Mode.Out)); // op OUT, exclusive to Q
//        }
//      }
//    }

//  }
//}
