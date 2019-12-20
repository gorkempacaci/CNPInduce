using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    public class TermList : Term, IFreeContainer
    {
        public Term Head { get; set; }
        public Term Tail { get; set; }
        /// <summary>
        /// Singleton list where tail is nil
        /// </summary>
        /// <param name="h"></param>
        public TermList(Term h)
        {
            Head = h;
            Tail = NilTerm.Instance;
            if (h is Free hFree)
            {
                hFree.RegisterContainer(this);
            }
        }
        public TermList(Term h, Term t)
        {
            Head = h;
            Tail = t;
            if (h is Free hFree)
            {
                hFree.RegisterContainer(this);
            }
            if (t is Free tFree)
            {
                tFree.RegisterContainer(this);
            }
        }
        public static Term FromTerms(params Term[] terms)
        {
            return FromEnumerable(terms);
        }

        /// <summary>
        /// The list tail is by default nil (NilTerm).
        /// </summary>
        public static Term FromEnumerable(IEnumerable<Term> terms, Term tail = null)
        {
            return FromEnumerator(terms.GetEnumerator(), tail);
        }
        static Term FromEnumerator(IEnumerator<Term> it, Term tail)
        {
            if (it.MoveNext())
                return new TermList(it.Current, FromEnumerator(it, tail));
            else
                return tail ?? NilTerm.Instance;
        }
        public override bool IsGround()
        {
            return Head.IsGround() && Tail.IsGround();
        }
        public override Term Clone(FreeDictionary plannedParenthood)
        {
            return new TermList(Head.Clone(plannedParenthood),
                                Tail.Clone(plannedParenthood));
        }

        IFreeContainer IFreeContainer.Clone(FreeDictionary plannedParenthood)
        {
            return this.Clone(plannedParenthood) as IFreeContainer;
        }
        
        public override bool Contains(Free other)
        {
            return Head.Contains(other) || Tail.Contains(other);
        }
        
        public void SubstituteFreeInPlace(Free oldTerm, Term newTerm)
        {
            var foundFree = false;
            if (object.ReferenceEquals(Head, oldTerm))
            {
                Head = newTerm;
                if (newTerm is Free newTermFree)
                {
                    newTermFree.RegisterContainer(this);
                }
                foundFree = true;
            }
            if (object.ReferenceEquals(Tail, oldTerm))
            {
                Tail = newTerm;
                if (newTerm is Free newTermFree)
                {
                    newTermFree.RegisterContainer(this);
                }
                foundFree = true;
            }
            if (!foundFree)
            {
                throw new Exception("Free "+oldTerm.ToString()+" was not found in list's immediate head or tail. List:" + this.ToString()) ;
            }
        }

        private static string _internalListString(TermList li)
        {
            if (li.Tail is TermList tailList)
            {
                return li.Head.ToString() + ", " + _internalListString(tailList);
            } else if (li.Tail is NilTerm)
            {
                return li.Head.ToString();
            } else // tail is another term (unusual case)
            {
                return li.Head.ToString() + "|" + li.Tail.ToString();
            }
        }

        public override string ToString()
        {
            return "[" + _internalListString(this) + "]";
        }

        public override bool Equals(object obj)
        {
            return (obj is TermList li) &&
                li.Head.Equals(Head) &&
                li.Tail.Equals(Tail);
        }
        public override int GetHashCode()
        {
            return HashCode.Combined(Head, Tail);  
        }

        /// <summary>
        /// Returns the terms in an IEnumerable.
        /// </summary>
        /// <param name="includeTerminalNil">If set to true returns [] as an element. For example, returns [1,2,3,[]] for [1,2,3]. Still returns [1,2,3] for [1,2|3].</param>
        /// <returns>A list of all elements in the list. It returns [1,2,3] for both [1,2,3] and [1,2|3]. </returns>
        public IEnumerable<Term> ToEnumerable(bool includeTerminalNil = false)
        {
            yield return Head;
            if (Tail is NilTerm && includeTerminalNil)
            {
                yield return Tail;
            }
            else
            {
                if (!(Tail is TermList li))
                {
                    yield return Tail;
                }
                else
                {
                    foreach (Term t in li.ToEnumerable(includeTerminalNil))
                        yield return t;
                }
            }
        }
    }
}
