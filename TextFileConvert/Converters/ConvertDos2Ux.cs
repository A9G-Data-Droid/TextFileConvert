using System.IO;

namespace TextFileConvert.Converters;

public class ConvertDos2Ux : ITextConverter
{
    public void WriteConvertedText(FileStream originalFile, FileStream newFile)
    {
        var readBuffer = new byte[1];
        while (originalFile.CanRead)
        {
            int readChars = originalFile.Read(readBuffer, 0, 1);
            if (readChars == 0) break;

            // Look for Dos line endings
            if (readBuffer[0] == Common.Cr)
            {
                // Strip out CR chars if followed by LF
                var peekBuffer = new byte[1];
                readChars = originalFile.Read(peekBuffer, 0, 1);
                if (readChars == 0) break;

                if (peekBuffer[0] != Common.Lf)
                {
                    originalFile.Position -= 1;
                }
                else
                {
                    readBuffer = peekBuffer;
                }
            }

            newFile.Write(readBuffer, 0, 1);
        }
    }
}