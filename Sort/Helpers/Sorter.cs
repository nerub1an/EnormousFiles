using System.Collections;

namespace Sort.Helpers
{
  /// <summary>
  /// Class to sort strings array in memory.
  /// </summary>
  public class Sorter : ISorter
  {
    private readonly IComparer comparer;

    public Sorter(IComparer comparer)
    {
      this.comparer = comparer;
    }

    /// <inheritdoc />
    public void Sort(string[] source, int length)
    {
      Sort(source, 0, length - 1);
    }

    private void Sort(string[] source, int left, int right)
    {
      if (right < left)
      {
        return;
      }

      int i = left;
      int j = right;
      string x = source[(left + right) / 2];

      do
      {
        while (comparer.Compare(source[i], x) < 0 && i < right)
        {
          i++;
        }

        while (comparer.Compare(x, source[j]) < 0 && j > left)
        {
          j--;
        }

        if (i <= j)
        {
          string c = source[i];
          source[i] = source[j];
          source[j] = c;
          i++;
          j--;
        }
      } while (i <= j);

      if (left < j)
      {
        Sort(source, left, j);
      }

      if (i < right)
      {
        Sort(source, i, right);
      }

    }
  }
}