using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    /// <summary>
    /// 
    /// </summary>
    public class TypeStore<TType> where TType:ProgramType
    {
        private Dictionary<int, IEnumerable<TType>> typeLists;

        public TypeStore(IEnumerable<TType> types)
        {
            if (!types.All(t => t.IsGround()))
                throw new Exception("Types in a type store must be ground.");
            typeLists = types.GroupBy(t => keyFor(t.Domains))
                             .ToDictionary(g => g.Key, g => g as IEnumerable<TType>);
        }

        public bool TryGetValue(NameModeMap doms, out IEnumerable<TType> types)
        {
            types = Iterators.Empty<TType>();
            if (typeLists.TryGetValue(keyFor(doms), out IEnumerable<TType> potentials))
            {
                var groundDomainNames = doms.Names.Where(n => n.IsGround());
                var partialMatches = potentials.Where(p => !groundDomainNames.Except(p.Domains.Names).Any());
                if (partialMatches.Any())
                {
                    types = partialMatches;
                    return true;
                }
            }
            return false;
        }

        private int keyFor(NameModeMap doms)
        {
            return doms.GetHashCode();
            //return string.Concat(doms.Select(d => d.Value == ArgumentMode.In ? "i" : "o"));
        }
    }
}