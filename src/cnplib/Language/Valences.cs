using System;
using System.Collections.Generic;
using CNP.Parsing;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    public static class Valences
    {
        public static IReadOnlyDictionary<Signature, (Signature,  Signature)> Fold;

        private static void _initializeFoldValences()
        {
            (string, string, string)[] foldModesStrs = { // foldValence, PValence, QValence
                ("{a0:in, as:in, b:out}", "{a:in, b:in, ab:out}", "{a:in, b:out}")
            };
            Fold = foldModesStrs.ToDictionary(cv => Parser.ParseProgramSignature(cv.Item1),
                                       cv => (Parser.ParseProgramSignature(cv.Item2), Parser.ParseProgramSignature(cv.Item3)));
        }

        static Valences()
        {
            _initializeFoldValences();
        }
    }
}
