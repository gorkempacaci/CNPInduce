using System;
using System.Collections.Generic;
using System.Net.Http;
using CNP.Language;
namespace CNP.Helper
{
  /// <summary>
  /// A dictionary that only identifies Frees and argument names by their reference.
  /// When duplicating a program expression, starting with an empty termdictionary
  /// is useful so the variables shared among the multiple ObservedPrograms
  /// stay consistent in the dupicated program.
  /// </summary>
  public class TermReferenceDictionary : ReferenceDictionary<Term, Term>
  {
    public TermReferenceDictionary() { }
    public TermReferenceDictionary(Free f, Term t)
    {
      this.Add(f, t);
    }
    public TermReferenceDictionary(IEnumerable<KeyValuePair<Term, Term>> pairs) : base(pairs) { }
  }

}
