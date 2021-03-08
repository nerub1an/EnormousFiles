using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AltiumSort.Helpers
{
  /// <summary>
  /// Class to sort array by several threads.
  /// </summary>
  public class MultiThreadSorter : ISorter
  {
    private readonly IComparer comparer;
    public static int ProcessorCount { get; set; } = Environment.ProcessorCount;

    public MultiThreadSorter(IComparer comparer)
    {
      this.comparer = comparer;
    }

    /// <inheritdoc />
    public void Sort(string[] source, int length)
    {
      List<string[]> parts = Divide(source, length);

      var tasks = new List<Task>();

      foreach (string[] part in parts)
      {
        string[] temp = part;
        tasks.Add(Task.Run(() =>
        {
          new Sorter(comparer).Sort(temp, temp.Length);
        }));
      }

      Task.WaitAll(tasks.ToArray());

      List<Part> sortedParts = parts.Select(x => new Part { Data = x, Pointer = 0 }).ToList();

      Merge(source, length, sortedParts, comparer);
    }

    private class Part
    {
      public string[] Data { get; set; }
      public int Pointer { get; set; }
    }

    private static void Merge(string[] destArr, int length, List<Part> parts, IComparer comparer)
    {
      for (int i = 0; i < length; i++)
      {
        Part first = parts.First();
        string minValue = first.Data[first.Pointer];
        Part min = first;

        for (int j = 1; j < parts.Count; j++)
        {
          if (comparer.Compare(parts[j].Data[parts[j].Pointer], minValue) < 0)
          {
            minValue = parts[j].Data[parts[j].Pointer];
            min = parts[j];
          }
        }

        destArr[i] = minValue;
        min.Pointer++;

        if (min.Pointer >= min.Data.Length)
        {
          parts.Remove(min);
        }
      }
    }

    private List<string[]> Divide(string[] a, int length)
    {
      var arrays = new List<string[]>();

      int divisionSize = length / ProcessorCount;

      for (int i = 0; i < ProcessorCount; i++)
      {
        int size = divisionSize;

        if (i == ProcessorCount - 1)
        {
          size += length % ProcessorCount;
        }

        var part = new string[size];

        Array.Copy(a, divisionSize * i, part, 0, part.Length);

        arrays.Add(part);
      }

      return arrays;
    }
  }
}