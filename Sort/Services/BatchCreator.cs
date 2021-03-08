using Sort.Models;

namespace Sort.Services
{
  /// <summary>
  /// Class to determine indexes of lines in a batch to further merging.
  /// </summary>
  public class BatchCreator
  {
    private readonly char[] chars;

    /// <summary>
    /// Initialize new instance of <see cref="BatchCreator"/>.
    /// </summary>
    public BatchCreator(char[] chars)
    {
      this.chars = chars;
    }

    /// <summary>
    /// Creates <see cref="BatchInfo"/> by array of strings.
    /// </summary>
    public BatchInfo CreateBatch(string[] array, int length)
    {
      // remember position of each index in char array in memory
      var newBatch = new BatchInfo
      {
        // add one more to remember end of batch
        Indexes = new int[chars.Length + 1]
      };

      for (var i = 0; i < newBatch.Indexes.Length; i++)
      {
        newBatch.Indexes[i] = -1;
      }

      char previous = chars[0];

      for (int i = 0, charIndex = 0; i < length; i++)
      {
        char current = array[i][array[i].IndexOf('.') + 2];

        if (current == chars[charIndex])
        {
          previous = current;
          newBatch.Indexes[charIndex++] = i;
          if (charIndex == chars.Length)
          {
            break;
          }
        }
        // next char is absent - try to find another one
        else if (current != previous)
        {
          if (++charIndex == chars.Length)
          {
            break;
          }
          i--;
        }
      }

      // remember lines count in the batch
      newBatch.Indexes[^1] = length;

      return newBatch;
    }
  }
}