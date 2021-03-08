using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Create
{
  /// <summary>
  /// Class that creates file with random strings.
  /// </summary>
  public class FileCreator
  {
    /// <summary>
    /// Queue of generated strings.
    /// </summary>
    private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

    /// <summary>
    /// Deletes file if exists.
    /// </summary>
    private void DeleteFileIfExists(string fileName)
    {
      if (File.Exists(fileName))
      {
        File.Delete(fileName);
      }
    }

    /// <summary>
    /// Writes text from <see cref="queue"/> into the file.
    /// </summary>
    private void Write(string fileName, long fileSize)
    {
      var spin = new SpinWait();
      var starvation = Stopwatch.StartNew();

      using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
      {
        do
        {
          // until queue is not empty
          while (queue.TryDequeue(out string text))
          {
            if (stream.Length + text.Length >= fileSize)
            {
              // need to replace last '\r\n' if it is the last one part
              string replace = new StringRandomizer().CreateRandomString(2);
              text = text.Substring(0, text.Length - 2) + replace;
              stream.Write(Encoding.UTF8.GetBytes(text));
              return;
            }

            stream.Write(Encoding.UTF8.GetBytes(text));

            starvation.Restart();
          }

          // give to another threads processor quants
          spin.SpinOnce();

          // small waiting if any object has not in queue yet
        } while (starvation.ElapsedMilliseconds < 300);
      }
    }

    /// <summary>
    /// Creates the file with specified size.
    /// Do not use any lock to increase performance
    /// </summary>
    public void CreateFile(string fileName, long fileSize)
    {
      DeleteFileIfExists(fileName);

      long totalSize = 0;

      for (var i = 0; i < Environment.ProcessorCount - 1; i++)
      {
        Task.Run(() =>
        {
          var randomizer = new StringRandomizer();
          while (totalSize < fileSize)
          {
            queue.Enqueue(randomizer.GetString(Settings.BatchGenerationSize, Settings.RepeatCount));

            totalSize += Settings.BatchGenerationSize;
          }
        });
      }

      Write(fileName, fileSize);
    }
  }
}