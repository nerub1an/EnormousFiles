using AltiumSort.Helpers;
using AltiumSort.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltiumSort.Services
{
  /// <summary>
  /// Class to split and sort big file.
  /// </summary>
  public class BatchService
  {
    private readonly string fileName;
    private readonly long fileSize;
    private readonly BatchCreator batchCreator;
    private readonly BatchWriter batchWriter;
    private readonly ISorter sorter;

    /// <summary>
    /// Initialize new instance of <see cref="BatchService"/>.
    /// </summary>
    public BatchService(
      string fileName,
      long fileSize,
      BatchCreator batchCreator,
      BatchWriter batchWriter,
      ISorter sorter)
    {
      this.fileName = fileName;
      this.fileSize = fileSize;
      this.batchCreator = batchCreator;
      this.batchWriter = batchWriter;
      this.sorter = sorter;
    }

    /// <summary>
    /// Splits the file to the batches, sorts and writes it to temp files.
    /// </summary>
    public BatchInfo[] Execute()
    {
      var bathes = new List<BatchInfo>();
      var tasks = new List<Task>();

      int batchIndex = 0;
      string tail = string.Empty;
      long offset = 0;
      bool isTail = false;

      FileStream stream = null;

      try
      {
        stream = new FileStream(fileName, FileMode.Open);

        // keep in mind tails between branches to not allocate extra memory
        using (var reader = new BufferedStream(stream))
        {
          while (offset < fileSize)
          {
            long take = Settings.BatchSize;

            // take tail
            if (offset + take > fileSize)
            {
              take = fileSize - offset;
            }

            // allocate memory
            byte[] byteArray = new byte[take];
            reader.Read(byteArray);

            string[] array = Encoding.UTF8.GetString(byteArray)
              .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            // was tail? - join tail
            if (isTail)
            {
              array[0] = tail + array[0];
            }

            offset += take;

            isTail = false;

            // if not last - keep tail
            if (offset < fileSize)
            {
              // if buffered stream read last byte between '\r' and '\n'
              if (array[^1][^1] == '\r')
              {
                // read '\n'
                reader.ReadByte();

                // remove '\r'
                array[^1] = array[^1].Remove(array[^1].Length - 1, 1);
              }
              else
              {
                tail = array.Last();
                isTail = true;
              }
            }

            int workLength = isTail ? array.Length - 1 : array.Length;

            // sort is happened in threads so we load all existed cores here
            sorter.Sort(array, workLength);

            int tempIndex = batchIndex++;

            tasks.Add(Task.Run(() =>
            {
              BatchInfo batch = batchCreator.CreateBatch(array, workLength);
              batch.FileName = $"{fileName}{tempIndex}";

              batchWriter.Write(array, workLength, batch.FileName);

              bathes.Add(batch);
            }));
          }
        }
      }
      finally
      {
        stream?.Dispose();
      }

      Task.WaitAll(tasks.ToArray());

      return bathes.ToArray();
    }
  }
}