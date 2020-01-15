using System;
using System.Collections.Generic;
using System.Collections;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Common Practices and Code Improvements", "RECS0063:Warns when a culture-aware 'StartsWith' call is used by default.", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Common Practices and Code Improvements", "RECS0061:Warns when a culture-aware 'EndsWith' call is used by default.", Justification = "<Pending>")]
    public class AlphaTuple : IEnumerable<KeyValuePair<ArgumentName, Term>>, IFreeContainer
    {
        private readonly SortedList<ArgumentName, Term> _terms;
        public IReadOnlyDictionary<ArgumentName, Term> Terms => _terms;

        public IEnumerable<ArgumentName> DomainNames => _terms.Keys;

        public AlphaTuple(params (string, Term)[] terms)
            :this(terms.ToDictionary(t => new ArgumentName(t.Item1), t=>t.Item2))
        {

        }

        /// <summary>
        /// Builds a new alphatuple from a dictionary. Creates a new data structure internally. Does not do a deep copy.
        /// </summary>
        /// <param name="terms"></param>
        public AlphaTuple(IEnumerable<KeyValuePair<ArgumentName, Term>> terms)
        {
            _terms = new SortedList<ArgumentName, Term>();
            foreach (KeyValuePair<ArgumentName, Term> nv in terms)
            {
                _terms.Add(nv.Key, nv.Value);
                if (nv.Value is Free freeValue)
                {
                    freeValue.RegisterContainer(this);
                }
            }
        }

        public AlphaTuple Clone(TermReferenceDictionary plannedParenthood)
        {
            return new AlphaTuple(_terms.Select(e => new KeyValuePair<ArgumentName, Term>(e.Key, e.Value.Clone(plannedParenthood))));
        }
        IFreeContainer IFreeContainer.Clone(TermReferenceDictionary plannedParenthood)
        {
            return this.Clone(plannedParenthood);
        }

        public IEnumerator<KeyValuePair<ArgumentName, Term>> GetEnumerator()
        {
            return Terms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Terms.GetEnumerator();
        }

        public void SubstituteFreeInPlace(Free oldTerm, Term newTerm)
        {
            var keys = new List<ArgumentName>(_terms.Keys);
            var values = new List<Term>(_terms.Values);
            bool foundFree = false;
            for(int i=0; i<values.Count; i++)
            {
                if (object.ReferenceEquals(values[i], oldTerm))
                {
                    _terms[keys[i]] = newTerm;
                    if (newTerm is Free newTermFree)
                    {
                        newTermFree.RegisterContainer(this);
                    }
                    foundFree = true;
                }
            }
            if (!foundFree)
            {
                throw new Exception("Free was not found in AlphaTuple to be replaced.");
            }
        }

        public Term this[string name] => Terms[new ArgumentName(name)];

        public Term this[ArgumentName name] => Terms[name];

        public override string ToString()
        {
            return "{" + string.Join(", ", _terms.Select(kv => kv.Key + ":" + kv.Value.ToString())) + "}";
        }
    }


}
