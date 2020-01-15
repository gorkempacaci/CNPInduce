using System;
using System.Collections;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using Enumerable = System.Linq.Enumerable;

namespace CNP.Language
{
    public class NameModeMap : IEnumerable<KeyValuePair<ArgumentName, ArgumentMode>>
    {
        readonly int hashcode; // persists as the names become ground
        readonly IReadOnlyDictionary<ArgumentName, ArgumentMode> dict;

        public NameModeMap(params (string, ArgumentMode)[] tups)
            : this(tups.ToDictionary(x => new ArgumentName(x.Item1), x => x.Item2))
        {
        }

        public NameModeMap(params (ArgumentName, ArgumentMode)[] tups)
            : this(tups.ToDictionary(x => x.Item1, x => x.Item2))
        {
        }

        public NameModeMap(IEnumerable<KeyValuePair<string, ArgumentMode>> args)
            : this(args.ToDictionary(x => new ArgumentName(x.Key), x => x.Value))
        {

        }

        public NameModeMap(IEnumerable<KeyValuePair<ArgumentName, ArgumentMode>> args)
            : this(args.ToDictionary(x => x.Key, x => x.Value))
        {

        }

        public NameModeMap(IDictionary<ArgumentName, ArgumentMode> dic)
        {
            dict = new Dictionary<ArgumentName, ArgumentMode>(dic);
            int ins = dic.Count(kv => kv.Value == ArgumentMode.In);
            int outs = dic.Count(kv => kv.Value == ArgumentMode.Out);
            hashcode = ins * 23 + outs * 17;
        }

        public NameModeMap Clone(TermReferenceDictionary plannedParenthood)
        {
            return new NameModeMap(dict.ToDictionary(kvp => kvp.Key.Clone(plannedParenthood) as ArgumentName, kvp => kvp.Value));
        }

        public bool IsGround()
        {
            return dict.Keys.All(k => k.IsGround());
        }


        public IEnumerable<ArgumentName> Names => dict.Keys;

        public bool TryGetValue(ArgumentName name, out ArgumentMode mode)
        {
            return dict.TryGetValue(name, out mode);
        }

        public ArgumentMode this[string name] => dict[new ArgumentName(name)];
        public ArgumentMode this[ArgumentName name] => dict[name];

        public IEnumerator<KeyValuePair<ArgumentName, ArgumentMode>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        public override int GetHashCode() => hashcode;
        
        public override bool Equals(object obj)
        {
            if (obj is NameModeMap otherPs && dict.Keys.Count() == otherPs.Names.Count())
            {
                foreach (ArgumentName an in Names)
                {
                    bool ifMe = this.TryGetValue(an, out ArgumentMode myMode);
                    bool ifOther = otherPs.TryGetValue(an, out ArgumentMode otherMode);
                    bool modes = myMode.Equals(otherMode);
                    return ifMe && ifOther && modes;
                }

                return Names.All(k => this.TryGetValue(k, out ArgumentMode myMode) &&
                                      otherPs.TryGetValue(k, out ArgumentMode othersMode) &&
                                      myMode.Equals(othersMode));
            }

            return false;
        }
    }
}