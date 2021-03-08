namespace Sort.Helpers
{
  /// <summary>
  /// Interface of instance to sort string array.
  /// </summary>
  public interface ISorter
  {
    /// <summary>
    /// Sorts specified part of strings array.
    /// </summary>
    void Sort(string[] source, int length);
  }
}