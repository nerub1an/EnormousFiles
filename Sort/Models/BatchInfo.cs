using System.IO;

namespace Sort.Models
{
  /// <summary>
  /// Struct to keep info about one batch.
  /// </summary>
  public struct BatchInfo
  {
    /// <summary>
    /// Since batch size cannot be more than int.MaxValue offset cannot be more than it.
    /// </summary>
    public int[] Indexes { get; set;  }

    /// <summary>
    /// Name of the batch.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Reader to read the batch.
    /// </summary>
    public StreamReader Reader { get; set; }
  }
}