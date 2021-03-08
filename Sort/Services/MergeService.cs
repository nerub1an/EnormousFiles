using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sort.Helpers;
using Sort.Models;

namespace Sort.Services
{
  /// <summary>
  /// Class to merge sorted files into one sorted big.
  /// </summary>
  public class MergeService
  {
    private readonly string sortFileName;
    private readonly char[] chars;
    private readonly ISorter sorter;
    private readonly Comparer comparer;

    /// <summary>
    /// Class to remember indexes of searching letter in current <see cref="BatchInfo"/>
    /// to write it one by one batch by batch.
    /// </summary>
    private class BatchIndexes
    {
      public BatchInfo BatchInfo { get; set; }

      public int Start { get; set; }

      public int End { get; set; }

      public int Current { get; set; }
    }

    /// <summary>
    /// Collects indexes for each batch file for specified index of letters array.
    /// </summary>
    private BatchIndexes[] CountIndexes(BatchInfo[] sortedBatches, int charIndex)
    {
      var indexes = new BatchIndexes[sortedBatches.Length];

      for (var i = 0; i < sortedBatches.Length; i++)
      {
        int start = sortedBatches[i].Indexes[charIndex];

        indexes[i] = new BatchIndexes
        {
          BatchInfo = sortedBatches[i],
          Start = start,
          Current = start,
          End = -1
        };

        if (start == -1)
        {
          continue;
        }

        int nextCharIndex = charIndex + 1;

        int end = -1;

        // at least the last one will not be equal -1 - end of the file.
        while (end == -1)
        {
          end = sortedBatches[i].Indexes[nextCharIndex++];
        }

        indexes[i].End = end;
      }

      return indexes;
    }

    /// <summary>
    /// Files indexes too large to keep it in memory. Read, sort, write one by one.
    /// </summary>
    private void MergeOnTheFly(StreamWriter writer, long count, BatchIndexes[] charIndexes, bool isLast)
    {
      List<BatchIndexes> actualIndexes = charIndexes.Where(x => x.Start != -1).ToList();
      var currentLines = new string[actualIndexes.Count];

      // read first lines of all sorted files
      for (int j = 0; j < actualIndexes.Count; j++)
      {
        if (charIndexes[j].Current++ < charIndexes[j].End)
        {
          currentLines[j] = charIndexes[j].BatchInfo.Reader.ReadLine();
        }
        else
        {
          currentLines[j] = null;
        }
      }

      // collect batch for current index of letters array from all sorted files
      // line by line from each file - because they have been already sorted
      for (int i = 0; i < count; i++)
      {
        string minValue = null;
        int minIndex = 0;

        for (var j = 0; j < currentLines.Length; j++)
        {
          if (currentLines[j] == null)
          {
            continue;
          }

          if (minValue == null || comparer.Compare(currentLines[j], minValue) < 0)
          {
            minValue = currentLines[j];
            minIndex = j;
          }
        }

        // need to avoid last '\r\n'
        if (isLast && i == count - 1)
        {
          writer.Write(minValue);
        }
        else
        {
          writer.WriteLine(minValue);
        }

        if (charIndexes[minIndex].Current++ < charIndexes[minIndex].End)
        {
          // read line from current sorted file
          currentLines[minIndex] = charIndexes[minIndex].BatchInfo.Reader.ReadLine();
        }
        else
        {
          currentLines[minIndex] = null;
        }
      }
    }

    /// <summary>
    /// Merges all files in one batch by one index of letters array with sorting in memory.
    /// </summary>
    private void MergeInMemory(StreamWriter writer, long count, BatchIndexes[] charIndexes, bool isLast)
    {
      string[] batch = new string[count];

      // collect batch for current index of letters array from all sorted files
      // line by line from each file - because they have been already sorted
      for (int i = 0; i < batch.Length;)
      {
        foreach (BatchIndexes batchIndex in charIndexes.Where(x => x.Start != -1 && x.Current < x.End))
        {
          // read line from current sorted file
          batch[i++] = batchIndex.BatchInfo.Reader.ReadLine();

          batchIndex.Current++;
        }
      }

      sorter.Sort(batch, batch.Length);

      for (int i = 0; i < batch.Length; i++)
      {
        if (isLast && i == batch.Length - 1)
        {
          writer.Write(batch[i]);
        }
        else
        {
          writer.WriteLine(batch[i]);
        }
      }
    }

    /// <summary>
    /// Merges and writes batches one by one based on sorted indexes to output file.
    /// </summary>
    private void MergeInternal(BatchInfo[] sortedBatches)
    {
      for (var i = 0; i < sortedBatches.Length; i++)
      {
        sortedBatches[i].Reader = new StreamReader(sortedBatches[i].FileName);
      }

      using (var writer = new StreamWriter(sortFileName))
      {
        for (int i = 0; i < chars.Length; i++)
        {
          // indexes for all batches for specified char index of letters array
          BatchIndexes[] charIndexes = CountIndexes(sortedBatches, i);

          long count = charIndexes.Where(x => x.Start != -1).Select(x => x.End - x.Start).Sum();

          // if no one of bathes does not contain this index of letters array
          if (count == 0)
          {
            continue;
          }

          // batch too large to sort in memory
          if (count > Settings.InMemorySize)
          {
            MergeOnTheFly(writer, count, charIndexes, i == chars.Length - 1);
          }
          else
          {
            MergeInMemory(writer, count, charIndexes, i == chars.Length - 1);
          }
        }
      }
    }

    /// <summary>
    /// Initialize new instance of <see cref="MergeService"/>.
    /// </summary>
    public MergeService(string sortFileName, char[] chars, ISorter sorter, Comparer comparer)
    {
      this.sortFileName = sortFileName;
      this.chars = chars;
      this.sorter = sorter;
      this.comparer = comparer;
    }

    /// <summary>
    /// Merge sorted batches.
    /// </summary>
    public void Merge(BatchInfo[] sortedBatches)
    {
      if (File.Exists(sortFileName))
      {
        File.Delete(sortFileName);
      }

#if DEBUG
      long length = 0;
      long linesCount = 0;
      foreach (var sortedBatch in sortedBatches)
      {
        length += new FileInfo(sortedBatch.FileName).Length;
        using (FileStream stream = new FileStream(sortedBatch.FileName, FileMode.Open))
        {
          linesCount += CountLinesHelper.CountLines(stream);
        }
      }

      Console.WriteLine($"length of batched files: {length}");
      Console.WriteLine($"lines of files: {linesCount}.");
#endif

      MergeInternal(sortedBatches);

      // delete the temp files
      foreach (BatchInfo batch in sortedBatches)
      {
        batch.Reader.Dispose();
        File.Delete(batch.FileName);
      }
    }
  }
}