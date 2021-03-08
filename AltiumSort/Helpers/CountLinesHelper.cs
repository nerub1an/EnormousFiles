using System.IO;

namespace AltiumSort.Helpers
{
  public static class CountLinesHelper
  {
    private const char CR = '\r';
    private const char LF = '\n';
    private const char NULL = (char)0;

    public static long CountLines(Stream stream)
    {
      var lineCount = 0L;

      var byteBuffer = new byte[1024 * 1024];
      const int BytesAtTheTime = 4;
      var detectedEOL = NULL;
      var currentChar = NULL;

      int bytesRead;
      while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
      {
        var i = 0;
        for (; i <= bytesRead - BytesAtTheTime; i += BytesAtTheTime)
        {
          currentChar = (char)byteBuffer[i];

          if (detectedEOL != NULL)
          {
            if (currentChar == detectedEOL) { lineCount++; }

            currentChar = (char)byteBuffer[i + 1];
            if (currentChar == detectedEOL) { lineCount++; }

            currentChar = (char)byteBuffer[i + 2];
            if (currentChar == detectedEOL) { lineCount++; }

            currentChar = (char)byteBuffer[i + 3];
            if (currentChar == detectedEOL) { lineCount++; }
          }
          else
          {
            if (currentChar == LF || currentChar == CR)
            {
              detectedEOL = currentChar;
              lineCount++;
            }
            i -= BytesAtTheTime - 1;
          }
        }

        for (; i < bytesRead; i++)
        {
          currentChar = (char)byteBuffer[i];

          if (detectedEOL != NULL)
          {
            if (currentChar == detectedEOL) { lineCount++; }
          }
          else
          {
            if (currentChar == LF || currentChar == CR)
            {
              detectedEOL = currentChar;
              lineCount++;
            }
          }
        }
      }

      if (currentChar != LF && currentChar != CR && currentChar != NULL)
      {
        lineCount++;
      }
      return lineCount;
    }
  }
}