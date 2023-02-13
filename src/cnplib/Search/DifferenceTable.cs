using System;
using System.Collections;

namespace CNP.Search
{
  //Records and allows to query a record of which NameVar should be different from which other namevars.
  public struct DifferenceTable
  {

    private BitArray[] diffArr; // when a bit is set to false at (y,x), it means namevars with these two indices have to be different to each other.
    /*
     diffArr structure:
      0 1 2 3 4 5 6 ... (x axis)
    0  [          t]    L=6   (7-0-1)
    1    [         ]    L=5   (7-1-1)
    2      [t      ]    L=4
    3        [     ]    L=3
    4          [   ]    L=2
    5            [ ]    L=1   (7-5-1)
    ...
    (y axis)
     */
    public DifferenceTable(int n)
    {
      diffArr = new BitArray[n];
      for (int y = 0; y < n - 1; y++)
        diffArr[y] = new BitArray(n - y - 1);
    }
    private (int y, int x) getYX(int i, int j)
    {
      int y = Math.Min(i, j);
      int x = Math.Max(i, j) - y - 1;
      return (y, x);
    }
    public bool IsDifferent(int i, int j)
    {
      if (i == j)
        return false;
      var c = getYX(i, j);
      return diffArr[c.y][c.x];
    }
    public void AssertDifferent(int i, int j)
    {
      if (i == j)
        throw new ArgumentOutOfRangeException("NameVar cannot be asserted to be different from itself: NameVar:" + i);
      var c = getYX(i, j);
      diffArr[c.y][c.x] = true;
    }
  }
}

