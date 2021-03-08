namespace Sort
{
  public static class Settings
  {
    /// <summary>
    /// Name of the file that will be sorted.
    /// </summary>
    public const string FileName = "FileToSort";

    /// <summary>
    /// Available symbols.
    /// <remarks>
    /// Also, it possible to do it as string[] and increase to split each letter to peaces by these array again
    /// AA, AB, AC, ..., Az if understand during the execution that file is enormous.
    /// But here we have InMemorySize 2Gb so ~100Gb file should be possible to keep in memory 2Gb for memory sorting.
    /// If generation of file was randomly then each letter should be ~ 100/52Gb that more than 2Gb.
    /// </remarks>
    /// </summary>
    public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// Size of each batch.
    /// </summary>
    public const int BatchSize = 50000000;

    /// <summary>
    /// Size of memory that we can use - 2Gb as default.
    /// </summary>
    public static long InMemorySize { get; set; } = 0x80000000;
  }
}