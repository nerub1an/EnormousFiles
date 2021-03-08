namespace Create
{
  public static class Settings
  {
    /// <summary>
    /// Name of the file that will be created.
    /// </summary>
    public const string FileName = "FileToSort";

    /// <summary>
    /// Size of batch that will be generated as string to writer into the file - 1Mb.
    /// </summary>
    public const int BatchGenerationSize = 1 * 1024 * 1024;

    /// <summary>
    /// File size multiplication = 1Mb.
    /// </summary>
    public const int FileSizeMultiply = 1024 * 1024;

    /// <summary>
    /// Default max value to randomize number in generated string.
    /// </summary>
    public const int NumberMaxValue = int.MaxValue;

    /// <summary>
    /// Default length to randomize name in generated string.
    /// </summary>
    public const int NameMaxLength = 1000;

    /// <summary>
    /// Count of repeating names during the generating strings.
    /// </summary>
    public const int RepeatCount = 10;

    /// <summary>
    /// Available symbols.
    /// </summary>
    public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
  }
}