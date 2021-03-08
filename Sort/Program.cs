using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sort.Helpers;
using Sort.Models;
using Sort.Services;

namespace Sort
{
  class Program
  {
    private static void DebugInfo(string fileName, string sortFileName)
    {
      using (var stream = new FileStream(fileName, FileMode.Open))
      {
        Console.WriteLine($"not sorted:{CountLinesHelper.CountLines(stream)}");
      }

      using (var stream = new FileStream(sortFileName, FileMode.Open))
      {
        Console.WriteLine($"sorted:{CountLinesHelper.CountLines(stream)}");
      }
    }

    /// <summary>
    /// Sorts file in memory. Use multi-threading if processor count > 1.
    /// </summary>
    private static void SortInMemory(string fileName, string sortFilePath, ISorter sorter)
    {
      string[] lines = File.ReadLines(fileName).ToArray();

      sorter.Sort(lines, lines.Length);

      using (var writer = new StreamWriter(sortFilePath))
      {
        for (var i = 0; i < lines.Length; i++)
        {
          if (i == lines.Length - 1)
          {
            writer.Write(lines[i]);
          }
          else
          {
            writer.WriteLine(lines[i]);
          }
        }
      }
    }

    /// <summary>
    /// Sorts file by batches. Splits to batches, sorts it and writes in temp files.
    /// Then merge temp files and sorts during the splitting. Writes to output sorted file.
    /// </summary>
    private static void Sort(
      string fileName, long fileSize, string sortFilePath, ISorter sorter, Stopwatch timer)
    {
      char[] chars = Settings.Chars.ToArray();

      Console.WriteLine("File size too big - splitting and sorting batches of the file....");

      var batchService = new BatchService(
        fileName,
        fileSize,
        new BatchCreator(chars),
        new BatchWriter(),
        sorter);

      BatchInfo[] batches = batchService.Execute();

      Console.WriteLine($"The file was batched to '{batches.Length}' peaces: {timer.Elapsed:g}.");
      Console.WriteLine("Merging...");

      var mergeService = new MergeService(sortFilePath, chars, sorter, new Comparer());
      mergeService.Merge(batches);
    }

    static void Main(string[] args)
    {
      string fileName = Path.Combine(Path.GetTempPath(), Settings.FileName);

      if (!File.Exists(fileName))
      {
        Console.WriteLine($"File '{fileName}' does not exist.");
        return;
      }

      Console.CancelKeyPress += (sender, e) => Environment.Exit(-1);

      var timer = new Stopwatch();

      string sortFileName = $"{fileName}.Sort";

      ISorter sorter = Environment.ProcessorCount == 1
        ? (ISorter)new Sorter(new Comparer())
        : (ISorter)new MultiThreadSorter(new Comparer());

      while (true)
      {
        long fileSize = new FileInfo(fileName).Length;

        Console.Clear();
        Console.WriteLine($"Sorting the file '{fileName}'... Size: {fileSize} (Ctrl+C for exit)");

        timer.Restart();

        // Correct exceptions handling that managed by async await pattern is avoided to improve performance.
        try
        {
          if (fileSize < Settings.InMemorySize)
          {
            SortInMemory(fileName, sortFileName, sorter);
          }
          else
          {
            Sort(fileName, fileSize, sortFileName, sorter, timer);
          }

          timer.Stop();

          Console.WriteLine($"File '{sortFileName}' is created.");
          Console.WriteLine($"Creation time: {timer.Elapsed:g}.");

#if DEBUG
          DebugInfo(fileName, sortFileName);
#endif
          Console.WriteLine($"Size of unsorted file: {new FileInfo(fileName).Length}");
          Console.WriteLine($"Size of sorted file: {new FileInfo(sortFileName).Length}");
        }
        catch (AggregateException e)
        {
          Console.WriteLine(e.InnerExceptions[0].Message);
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }

        Console.WriteLine("Enter any key for sort file again.");
        Console.ReadKey();
      }
    }
  }
}
