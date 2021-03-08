using System;
using System.Diagnostics;
using System.IO;

namespace Create
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.CancelKeyPress += (sender, e) => Environment.Exit(-1);

      while (true)
      {
        // since necessary to create big file take size by default more or equal than 1Mb.
        // accuracy of created file also in Mb so size of it possible to be a bit more.
        long inputSize;

        while (true)
        {
          Console.Clear();
          Console.Write("Enter file size in Mb (Ctrl+C for exit): ");
          string fileSizeString = Console.ReadLine();

          if (long.TryParse(fileSizeString, out inputSize) && inputSize > 0)
          {
            break;
          }
        }

        string filePath = Path.Combine(Path.GetTempPath(), Settings.FileName);

        Stopwatch timer = new Stopwatch();

        long fileSize = inputSize * Settings.FileSizeMultiply;

        Console.WriteLine($"Creating the file '{filePath}' with size: {fileSize}");

        timer.Start();

        new FileCreator().CreateFile(filePath, fileSize);

        timer.Stop();

        Console.WriteLine($"File '{filePath}' is created.");
        Console.WriteLine($"Creation time: {timer.Elapsed:g}.");
        Console.WriteLine($"Size of created file: {new FileInfo(filePath).Length}");
        Console.WriteLine("Enter any key for create file again.");
        Console.ReadKey();
      }
    }
  }
}
