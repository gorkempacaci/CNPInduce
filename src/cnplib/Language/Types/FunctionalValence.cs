using System;
using CNP.Display;
using CNP.Language;

namespace CNP.Language
{
  /// <summary>
  /// Class of valences where the valence is conditional on the valence of operands.
  /// For example, valence of foldr(P,Q) is conditional of valences of P and Q.
  /// Subclasses should publicise a constructor that takes operand valences as a parameter preceeding the operator valence.
  /// </summary>
  public abstract class FunctionalValence : Valence
  {
    protected FunctionalValence(Valence domains) : base(domains)
    {
    }

    public abstract override string Pretty(PrettyStringer ps);

  }
}
