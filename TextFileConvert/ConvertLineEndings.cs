using System;
using System.IO;
using System.Threading.Tasks;
using TextFileConvert.Converters;

namespace TextFileConvert;

public class ConvertLineEndings
{
    /// <summary>
    ///     Converts a DOS text file to have Unix line endings.
    /// </summary>
    /// <param name="originalFile">The file to convert.</param>
    /// <param name="newFile">The name of a new file to create.</param>
    /// <returns>Exit code.</returns>
    public static async Task<int> Dos2Ux(string originalFile, string newFile)
    {
        return await ReplaceLineEndings(originalFile, newFile, new ConvertDos2Ux());
    }

    /// <summary>
    ///     Converts a DOS text file to have Unix line endings.
    /// </summary>
    /// <param name="originalFile">The file to convert.</param>
    /// <param name="newFile">The name of a new file to create.</param>
    /// <returns>Exit code.</returns>
    public static async Task<int> Ux2Dos(string originalFile, string newFile)
    {
        return await ReplaceLineEndings(originalFile, newFile, new ConvertUx2Dos());
    }

    /// <summary>
    ///     Loads a whole text file in to memory, Performs a find\replace, and writes a new file.
    /// </summary>
    /// <param name="originalFile">The file path to convert.</param>
    /// <param name="newFile">The name of a new file to create.</param>
    /// <param name="convertMode">This is the type of conversion we are going to perform.</param>
    /// <returns>Exit code. 0 is success. -1 is a symbolic link.</returns>
    private static async Task<int> ReplaceLineEndings(string originalFile, string newFile, ITextConverter convertMode)
    {
        try
        {
            // Do not attempt to work on symbolic links
            if (Common.IsSymbolic(originalFile))
                return -1;

            using var oldFileStream = new FileStream(originalFile, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.Asynchronous);
            using var newFileStream = new FileStream(newFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 65536, FileOptions.Asynchronous);

            // Reading and writing is done in this one line
            convertMode.WriteConvertedText(oldFileStream, newFileStream);
            await oldFileStream.FlushAsync();
            await newFileStream.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Number: {ex.HResult}");
            return ex.HResult;
        }

        return 0;
    }
}