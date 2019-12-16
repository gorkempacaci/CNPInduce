using System;
using System.Collections.Generic;
using System.Collections;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    public enum ArgumentMode { In, Out }

    public class Signature : IEnumerable<KeyValuePair<string, ArgumentMode>>
    {
        int hashcode;
        Dictionary<string, ArgumentMode> dict;

        public Signature(params (string, ArgumentMode)[] tups)
            : this(tups.ToDictionary(x => x.Item1, x => x.Item2))
        {}

        public Signature(IEnumerable<KeyValuePair<string, ArgumentMode>> args)
            : this(args.ToDictionary(x=>x.Key, x=>x.Value))
        {}

        private Signature(IDictionary<string, ArgumentMode> dic)
        {
            dict = new Dictionary<string, ArgumentMode>(dic);
            hashcode = HashCode.OfDictionary(dict);
        }

        public override bool Equals(object obj)
        {
            if (obj is Signature otherPs && dict.Keys.Count() == otherPs.Keys.Count())
            {
                return Keys.All(k => this.TryGetValue(k, out ArgumentMode myMode) &&
                                     otherPs.TryGetValue(k, out ArgumentMode othersMode) &&
                                     myMode.Equals(othersMode));
            }
            return false;
        }

        public IEnumerable<string> ArgNames => dict.Keys;

        public ArgumentMode this [string name]
        {
            get => dict[name];
        }

        public IEnumerable<string> Keys => dict.Keys;
        public IEnumerable<ArgumentMode> Values => dict.Values;

        public bool TryGetValue(string name, out ArgumentMode mode)
        {
            return dict.TryGetValue(name, out mode);
        }

        public IEnumerator<KeyValuePair<string, ArgumentMode>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        public override int GetHashCode() => hashcode;


    }
}
