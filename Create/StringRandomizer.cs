using System;
using System.Linq;
using System.Text;

namespace Create
{
  /// <summary>
  /// Class that queue randomize string.
  /// </summary>
  public class StringRandomizer
  {
    private readonly Random random = new Random();

    /// <summary>
    /// Generates specifying length random string of only alphabetic symbols.
    /// </summary>
    public string CreateRandomString(int length)
    {
      string chars = Settings.Chars;

      var stringChars = new char[length];

      for (int i = 0; i < stringChars.Length; i++)
      {
        stringChars[i] = chars[random.Next(chars.Length)];
      }

      return new string(stringChars);
    }

    /// <summary>
    /// Generates specifying length random string of all symbols.
    /// </summary>
    private string CreateTotalRandomString(int length)
    {
      byte[] bytes = new byte[random.Next(1, length)];
      random.NextBytes(bytes);
      return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Randomize repeating indexes of lines.
    /// </summary>
    private (int repeatableIndex, int[] repeatingIndexes) GetRepeatableIndexes(
      int repeatCount, int minimumLineCount)
    {
      int repeatableIndex = random.Next(1, minimumLineCount);
      int[] repeatingIndexes = new int[repeatCount];

      for (int i = 0; i < repeatingIndexes.Length; i++)
      {
        repeatingIndexes[i] = repeatableIndex;

        while (repeatingIndexes[i] == repeatableIndex)
        {
          repeatingIndexes[i] = random.Next(1, minimumLineCount);
        }

        if (repeatingIndexes[i] < repeatableIndex)
        {
          int c = repeatableIndex;
          repeatableIndex = repeatingIndexes[i];
          repeatingIndexes[i] = c;
        }
      }

      return (repeatableIndex, repeatingIndexes);
    }

    /// <summary>
    /// Creates string that contains list of pattern string '{number}. {name}'.
    /// It's truncated by specified size.
    /// </summary>
    public string GetString(int textLength, int repeatCount)
    {
      var stringBuilder = new StringBuilder();

      int numberLength = Settings.NumberMaxValue.ToString().Length;
      int nameLength = Settings.NameMaxLength;

      // need to keep one random name and repeat it.
      int minimumLineCount = textLength / (nameLength + numberLength + 2);

      var repeatIndexes = GetRepeatableIndexes(repeatCount, minimumLineCount);

      string repeatString = null;
      bool isLast = false;

      for (int i = 0; stringBuilder.Length < textLength; i++)
      {
        int number = random.Next(1, Settings.NumberMaxValue);

        string name;

        if (isLast)
        {
          name = CreateRandomString(textLength - stringBuilder.Length - number.ToString().Length - 4);
        }
        else
        {
          if (repeatIndexes.repeatingIndexes.Contains(i))
          {
            name = repeatString;
          }
          else
          {
            int nameLengthRandom = random.Next(1, nameLength);
            name = CreateRandomString(nameLengthRandom);
          }

          if (i == repeatIndexes.repeatableIndex)
          {
            repeatString = name;
          }
        }

        string line = $"{number}. {name}";

        // the next line is the last one - pattern is _. _\r\n
        if (!isLast && textLength - stringBuilder.Length - line.Length < numberLength + nameLength + 4)
        {
          isLast = true;
        }

        stringBuilder.AppendLine(line);
      }

      return stringBuilder.ToString();
    }
  }
}