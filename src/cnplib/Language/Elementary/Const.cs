using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

    public class Const : ElementaryProgram
    {
        public string ArgumentName { get; private set; }
        public Term Value { get; private set; }
        
        
        public Const(string argName, Term groundTerm)
        {
            if (!groundTerm.IsGround())
            {
                throw new Exception("The term for the Const can only be a ground term.");
            }

            ArgumentName = argName;
            Value = groundTerm;
        }

        public static IEnumerable<Const> FromObservation(ObservedProgram op)
        {
            if (op.ProgramType.Keys.Count() != 1 || !op.Observables.Any())
                return Iterators.Empty<Const>();
            Free candidateConstant = new Free();
            string argName = op.ProgramType.Keys.First();
            var allTups = Enumerable.ToList(op.Observables);
            int count = allTups.Count();
            for (int i = 1; i < count; i++)
                if (!Term.UnifyInPlace(allTups[0].Terms[argName], allTups[i].Terms[argName]))
                    return Iterators.Empty<Const>();
            if (!allTups[0].Terms[argName].IsGround())
                return Iterators.Empty<Const>();
            return Iterators.Singleton(new Const(argName, allTups[0].Terms[argName]));
        }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Const constProgram))
                return false;
            bool sameName = constProgram.ArgumentName.Equals(this.ArgumentName);
            bool sameValue = constProgram.Value.Equals(this.Value);
            return sameName && sameValue;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return "const("+ArgumentName+", "+Value.ToString()+")";
        }
    }
}
