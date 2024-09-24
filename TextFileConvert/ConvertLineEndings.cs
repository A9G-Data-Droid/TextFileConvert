using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TextFileConvert;

public class ConvertLineEndings
{
    private const byte Cr = 13;  //'\r';  // Carriage Return
    private const byte Lf = 10;  //'\n';  // Line Feed

    /// <summary>
    ///     These are the different conversions that this library can perform.
    /// </summary>
    private enum TextConvertMode
    {
        Dos2Ux,
        Ux2Dos
        // Dos2Mac
        // Mac2Dos
        // Mac2Ux
        // Ux2Mac
    }

    /// <summary>
    ///     Converts a DOS text file to have Unix line endings.
    /// </summary>
    /// <param name="originalFile">The file to convert.</param>
    /// <param name="newFile">The name of a new file to create.</param>
    /// <returns>Exit code.</returns>
    public static async Task<int> Dos2Ux(string originalFile, string newFile)
    {
        return await ReplaceLineEndings(originalFile, newFile, TextConvertMode.Dos2Ux);
    }

    /// <summary>
    ///     Converts a DOS text file to have Unix line endings.
    /// </summary>
    /// <param name="originalFile">The file to convert.</param>
    /// <param name="newFile">The name of a new file to create.</param>
    /// <returns>Exit code.</returns>
    public static async Task<int> Ux2Dos(string originalFile, string newFile)
    {
        return await ReplaceLineEndings(originalFile, newFile, TextConvertMode.Ux2Dos);
    }

    /// <summary>
    ///     Loads a whole text file in to memory, Performs a find\replace, and writes a new file.
    /// </summary>
    /// <param name="originalFile">The file path to convert.</param>
    /// <param name="newFile">The name of a new file to create.</param>
    /// <param name="convertMode">This is the type of conversion we are going to perform.</param>
    /// <returns>Exit code. 0 is success. -1 is a symbolic link.</returns>
    private static async Task<int> ReplaceLineEndings(string originalFile, string newFile, TextConvertMode convertMode)
    {
        try
        {
            // Do not attempt to work on symbolic links
            if (Common.IsSymbolic(originalFile))
                return -1;

            using var oldFileStream = new FileStream(originalFile, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.Asynchronous);

            // Attempt to detect encoding we will use for reading and writing
            var fileEncoding = await Common.GetEncoding(oldFileStream);
            Debug.Print(fileEncoding.ToString());

            // Rewind stream
            oldFileStream.Position = 0L;
            await oldFileStream.FlushAsync();

            // Reading and writing is done in this one line
            await WriteConvertedText(oldFileStream, convertMode, newFile);
        }
        catch (Exception ex)
        {
            Debug.Print("Error: " + ex.Message + Environment.NewLine + "Number: " + ex.HResult.ToString());
            return ex.HResult;
        }

        return 0;
    }

    /// <summary>
    ///     This is where the actual conversion logic lives.
    /// </summary>
    /// <param name="originalFile">The file you want to convert.</param>
    /// <param name="convertMode">The type of conversion you want to perform.</param>
    /// <param name="newFile">Full path to the new file to write</param>
    /// <returns>The full text of the file with new line endings.</returns>
    private static async Task WriteConvertedText(FileStream originalFile, TextConvertMode convertMode, string newFile)
    {
        //using var oldFile = new StreamReader(originalFile, fileEncoding, true);
        using var newFileStream = new FileStream(newFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 65536, FileOptions.Asynchronous);
        switch (convertMode)
        {
            case TextConvertMode.Dos2Ux:
            {
                while (originalFile.CanRead)
                {
                    var readBuffer = new byte[1];
                    int readChars = await originalFile.ReadAsync(readBuffer, 0, 1);
                    if (readChars == 0) break;

                    // Look for Dos line endings
                    if (readBuffer[0] == Cr)
                    {
                        // Strip out CR chars if followed by LF
                        var peekBuffer = new byte[1];
                        readChars = await originalFile.ReadAsync(peekBuffer, 0, 1);
                        if (readChars == 0) break;

                        if (peekBuffer[0] != Lf)
                        {
                            originalFile.Position -= 1;
                        }
                        else
                        {
                            readBuffer = peekBuffer;
                        }
                    }

                    await newFileStream.WriteAsync(readBuffer, 0, 1);
                }

                break;
            }
            case TextConvertMode.Ux2Dos:
            {
                while (originalFile.CanRead)
                {
                    var readBuffer = new byte[1];
                    int readChars = await originalFile.ReadAsync(readBuffer, 0, 1);
                    if (readChars == 0) break;

                    // Check for CR first to avoid doubling the CR character when LF is found
                    if (readBuffer[0] == Cr)
                    {
                        var peekBuffer = new byte[1];
                        readChars = await originalFile.ReadAsync(peekBuffer, 0, 1);
                        if (readChars == 0) break;

                        if (peekBuffer[0] == Lf)
                        {
                            // This is a DOS line ending, keep it.
                            Array.Resize(ref readBuffer, 2);
                            readBuffer[1] = peekBuffer[0];
                        }
                        else
                        {
                            // Rewind
                            newFileStream.Position -= 1;
                        }
                    }
                    else if (readBuffer[0] == Lf)
                    {
                        // This is a Unix line ending. Add preceding CR.
                        readBuffer = new byte[2];
                        readBuffer[0] = Cr;
                        readBuffer[1] = Lf;
                    }

                    await newFileStream.WriteAsync(readBuffer, 0, readBuffer.Length);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(convertMode), convertMode, "TextConvertMode not yet supported.");
        }
    }
}