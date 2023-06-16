using System;
namespace CNP.Language
{
  /// <summary>
  /// Creates frees with indices on a continuous space.
  /// </summary>
  public class FreeFactory
  {
    private int upperBound = 0;

    public FreeFactory()
    {

    }

    private FreeFactory(int uBound)
    {
      this.upperBound = uBound;
    }

    public static FreeFactory CopyFrom(FreeFactory other)
    {
      return new FreeFactory(other.upperBound);
    }

    /// <summary>
    /// Gets a new free with a unique index and increases the current upper bound.
    /// </summary>
    public Free NewFree()
    {
      return new Free(upperBound++);
    }
  }
}

