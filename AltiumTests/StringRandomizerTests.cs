using System;
using System.Linq;
using AltiumCreate;
using NUnit.Framework;

namespace AltiumTests
{
  /// <summary>
  /// Tests for <see cref="StringRandomizer"/>
  /// </summary>
  public class StringRandomizerTests
  {
    private StringRandomizer randomizer;
    [SetUp]
    public void Setup()
    {
      randomizer = new StringRandomizer();
    }

    [Test]
    public void TestSize()
    {
      int expected = Settings.BatchGenerationSize;

      string actual = randomizer.GetString(expected, 1);

      Assert.AreEqual(expected, actual.Length);
    }

    [Test]
    public void TestRepeat()
    {
      int repeat = 4;

      string[] lines = randomizer.GetString(5000, repeat).Split(
        Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

      for (var i = 0; i < lines.Length; i++)
      {
        lines[i] = lines[i].Substring(lines[i].IndexOf('.') + 2);
      }

      int actual = 0;

      for (var i = 0; i < lines.Length; i++)
      {
        int r = 1;

        for (var j = 0; j < lines.Length; j++)
        {
          if (j == i)
          {
            continue;
          }

          if (lines[i] == lines[j])
          {
            r++;
          }
        }

        if (r > 1)
        {
          actual = r;
          break;
        }
      }

      // possible that repeating lines is the last one and it was not repeated
      Assert.IsTrue(new[] { 3, 4 }.Contains(actual));
    }
  }
}