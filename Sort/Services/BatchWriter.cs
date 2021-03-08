using System.IO;

namespace Sort.Services
{
  /// <summary>
  /// Class to write a batch to the temp file.
  /// </summary>
  public class BatchWriter
  {
    /// <summary>
    /// Writes batch into file.
    /// </summary>
    public void Write(string[] array, int length, string batchFileName)
    {
      if (File.Exists(batchFileName))
      {
        File.Delete(batchFileName);
      }

      using (var writer = new StreamWriter(batchFileName))
      {
        for (int i = 0; i < length; i++)
        {
          if (i == length - 1)
          {
            writer.Write(array[i]);
          }
          else
          {
            writer.WriteLine(array[i]);
          }
        }
      }
    }
  }
}