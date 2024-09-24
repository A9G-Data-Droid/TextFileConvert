using System;
using System.IO;

namespace TextFileConvert.Converters;

public class ConvertUx2Dos : ITextConverter
{
    public void WriteConvertedText(FileStream originalFile, FileStream newFile)
    {
        while (originalFile.CanRead)
        {
            var readBuffer = new byte[1];
            int readChars = originalFile.Read(readBuffer, 0, 1);
            if (readChars == 0) break;

            // Check for CR first to avoid doubling the CR character when LF is found
            if (readBuffer[0] == Common.Cr)
            {
                var peekBuffer = new byte[1];
                readChars = originalFile.Read(peekBuffer, 0, 1);
                if (readChars == 0) break;

                if (peekBuffer[0] == Common.Lf)
                {
                    // This is a DOS line ending, keep it.
                    Array.Resize(ref readBuffer, 2);
                    readBuffer[1] = peekBuffer[0];
                }
                else
                {
                    // Rewind
                    originalFile.Position -= 1;
                }
            }
            else if (readBuffer[0] == Common.Lf)
            {
                // This is a Unix line ending. Add preceding CR.
                readBuffer = new byte[2];
                readBuffer[0] = Common.Cr;
                readBuffer[1] = Common.Lf;
            }

            newFile.Write(readBuffer, 0, readBuffer.Length);
        }
    }
}