using System;
using System.Collections.Generic;
using System.Collections;
using CNP.Helper;
using CNP.Helper.EagerLinq;


namespace CNP.Language
{
    public class AlphaTuple : IEnumerable<KeyValuePair<NameVar, Term>>, IFreeContext
    {
        private readonly SortedList<NameVar, Term> _terms;
        public IReadOnlyDictionary<NameVar, Term> Terms => _terms;

        public IEnumerable<NameVar> DomainNames => _terms.Keys;

        public AlphaTuple(params (string, Term)[] terms)
            :this(terms.ToDictionary(t => new NameVar(t.Item1), t=>t.Item2))
        {

        }

        /// <summary>
        /// Builds a new alphatuple from a dictionary. Creates a new data structure internally. Does not do a deep copy, so the same name and terms are used.
        /// </summary>
        /// <param name="terms"></param>
        public AlphaTuple(IEnumerable<KeyValuePair<NameVar, Term>> terms)
        {
            _terms = new SortedList<NameVar, Term>();
            foreach (KeyValuePair<NameVar, Term> nv in terms)
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
            return new AlphaTuple(_terms.Select(e => new KeyValuePair<NameVar, Term>(e.Key, e.Value.Clone(plannedParenthood))));
        }

        public IEnumerator<KeyValuePair<NameVar, Term>> GetEnumerator()
        {
            return Terms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Terms.GetEnumerator();
        }

        public void ReplaceAllInstances(Free oldTerm, Term newTerm)
        {
            var keys = new List<NameVar>(_terms.Keys);
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

        public Term this[string name] => Terms[new NameVar(name)];

        public Term this[NameVar name] => Terms[name];

        public override string ToString()
        {
            return "{" + string.Join(", ", _terms.Select(kv => kv.Key + ":" + kv.Value.ToString())) + "}";
        }
    }


}
