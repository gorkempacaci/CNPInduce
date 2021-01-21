using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    /// <summary>
    /// Only contains ground type signatures.
    /// </summary>
    public class TypeStore<TType> where TType:Valence
    {
        /// <summary>
        /// Maps mode numbers to lists of types (for optimization).
        /// </summary>
        private Dictionary<int, IEnumerable<TType>> typeLists;

        public TypeStore(IEnumerable<TType> types)
        {
            if (!types.All(t => t.IsGround()))
                throw new Exception("Types in a type store must be ground.");
            typeLists = types.GroupBy(t => t.ModeNumber)
                             .ToDictionary(g => g.Key, g => g as IEnumerable<TType>);
        }

        /// <summary>
        /// Returns ground types that match the given domains. The given domains may not be completely ground, therefore where given domains are not ground it returns all possible matching types for those domains.
        /// </summary>
        public IEnumerable<TType> FindCompatibleTypes(Valence doms)
        {
            // ModeNumber guarantees same no of input and output arguments
            if (typeLists.TryGetValue(doms.ModeNumber, out IEnumerable<TType> potentials))
            {
                var groundDomainNames = doms.Names.Where(n => n.IsGround());
                // filters only those that cover the ground domain names wished for
                IEnumerable<TType> types = potentials.Where(p => !groundDomainNames.Except(p.Names).Any());
                return types;
            }
            return Iterators.Empty<TType>();
        }
    }
}