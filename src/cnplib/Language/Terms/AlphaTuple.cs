using System;
using System.Collections.Generic;
using System.Collections;
using CNP.Helper;
using CNP.Helper.EagerLinq;


namespace CNP.Language
{
    public class AlphaTuple : IEnumerable<KeyValuePair<ArgumentNameVar, Term>>, IFreeContext
    {
        private readonly SortedList<ArgumentNameVar, Term> _terms;
        public IReadOnlyDictionary<ArgumentNameVar, Term> Terms => _terms;

        public IEnumerable<ArgumentNameVar> DomainNames => _terms.Keys;

        public AlphaTuple(params (string, Term)[] terms)
            :this(terms.ToDictionary(t => new ArgumentNameVar(t.Item1), t=>t.Item2))
        {

        }

        /// <summary>
        /// Builds a new alphatuple from a dictionary. Creates a new data structure internally. Does not do a deep copy, so the same name and terms are used.
        /// </summary>
        /// <param name="terms"></param>
        public AlphaTuple(IEnumerable<KeyValuePair<ArgumentNameVar, Term>> terms)
        {
            _terms = new SortedList<ArgumentNameVar, Term>();
            foreach (KeyValuePair<ArgumentNameVar, Term> nv in terms)
            {
                _terms.Add(nv.Key, nv.Value);
                if (nv.Value is Free freeValue)
                {
                    freeValue.AddAContext(this);
                }
            }
        }

        public AlphaTuple Clone(TermReferenceDictionary plannedParenthood)
        {
            return new AlphaTuple(_terms.Select(e => new KeyValuePair<ArgumentNameVar, Term>(e.Key, e.Value.Clone(plannedParenthood))));
        }

        public IEnumerator<KeyValuePair<ArgumentNameVar, Term>> GetEnumerator()
        {
            return Terms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Terms.GetEnumerator();
        }

        public void ReplaceAllInstances(Free oldTerm, Term newTerm)
        {
            var keys = new List<ArgumentNameVar>(_terms.Keys);
            var values = new List<Term>(_terms.Values);
            bool foundFree = false;
            for(int i=0; i<values.Count; i++)
            {
                if (object.ReferenceEquals(values[i], oldTerm))
                {
                    _terms[keys[i]] = newTerm;
                    if (newTerm is Free newTermFree)
                    {
                        newTermFree.AddAContext(this);
                    }
                    foundFree = true;
                }
            }
            if (!foundFree)
            {
                throw new Exception("Free was not found in AlphaTuple to be replaced.");
            }
        }

        public Term this[string name] => Terms[new ArgumentNameVar(name)];

        public Term this[ArgumentNameVar name] => Terms[name];

        public override string ToString()
        {
            return "{" + string.Join(", ", _terms.Select(kv => kv.Key + ":" + kv.Value.ToString())) + "}";
        }
    }


}
