using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

    public class Const : ElementaryProgram
    {
        public Term Value { get; private set; }
        
        private readonly ISet<string> constArgNames;
        public override ISet<string> ArgumentNames => constArgNames;
        
        public Const(string argName, Term groundTerm)
        {
            if (!groundTerm.IsGround())
            {
                throw new Exception("The term for the Const can only be a ground term.");
            }
            constArgNames = new HashSet<string>() { argName };
            Value = groundTerm;
        }

        public static Const FromObservation(ObservedProgram op)
        {
            if (op.ArgumentNames.Count() != 1 || !op.Observables.Any())
            {
                return null;
            }
            Free candidateConstant = new Free();
            string argName = op.ArgumentNames.First();
            var allTups = Enumerable.ToList(op.Observables);
            int count = allTups.Count();
            for (int i = 1; i < count; i++)
            {
                if (!Term.UnifyInPlace(allTups[0].Terms[argName], allTups[i].Terms[argName]))
                {
                    return null;
                }
            }
            if (!allTups[0].Terms[argName].IsGround())
                return null;
            return new Const(argName, allTups[0].Terms[argName]);
        }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Const constProgram))
                return false;
            bool sameName = constProgram.ArgumentNames.SetEquals(this.ArgumentNames);
            bool sameValue = constProgram.Value.Equals(this.Value);
            return sameName && sameValue;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return "Const("+ArgumentNames.First()+", "+Value.ToString()+")";
        }
    }
}
