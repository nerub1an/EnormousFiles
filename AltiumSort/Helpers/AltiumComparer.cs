using System;
using System.Collections;
using System.Collections.Generic;

namespace AltiumSort.Helpers
{
  /// <summary>
  /// Class to compare two strings of '_. _' format.
  /// </summary>
  public class AltiumComparer : IComparer<string>, IComparer
  {
    /// <inheritdoc />
    public int Compare(string x, string y)
    {
      try
      {
        int xNameIndex = x.IndexOf('.');
        int yNameIndex = y.IndexOf('.');

        var xName = x.AsSpan(xNameIndex + 2);
        var yName = y.AsSpan(yNameIndex + 2);

        var nameComparison = xName.CompareTo(yName, StringComparison.Ordinal);
        if (nameComparison != 0)
        {
          return nameComparison;
        }

        // if names are equal compare numbers
        int.TryParse(x.AsSpan(0, xNameIndex), out int xNumber);
        int.TryParse(y.AsSpan(0, yNameIndex), out int yNumber);

        return xNumber.CompareTo(yNumber);
      }
      catch
      {
        throw new Exception("File is corrupted. Lines should be format '{number}. {name}'");
      }
    }

    /// <inheritdoc />
    public int Compare(object x, object y)
    {
      return Compare(x?.ToString(), y?.ToString());
    }
  }
}