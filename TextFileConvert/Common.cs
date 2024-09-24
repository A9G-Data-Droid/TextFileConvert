using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TextFileConvert;

public static class Common
{
    /// <summary>
    /// This is the command line help that is displayed when invalid parameters are given.
    /// </summary>
    /// <param name="programName"></param>
    public static void PrintUsage(string programName)
    {
        Console.WriteLine("""

                          NAME
                              {0} - Text file format converter

                          SYNOPSIS
                              {0} old-filename new-filename

                          DESCRIPTION
                              {0} reads old-filename and writes out new-filename, converting line endings.

                          """, programName);
    }

    /// <summary>
    ///     Attempt to detect the encoding of a file.
    /// </summary>
    /// <param name="theFile">The file to get the encoding pattern from.</param>
    /// <returns>Encoding type, defaults to ASCII.</returns>
    public static async Task<Encoding> GetEncoding(FileStream theFile)
    {
        var bom = new byte[4];
        var count = await theFile.ReadAsync(bom, 0, 4);

        // Detect BOM type
        if (count > 2 && bom[0] == 0x2B && bom[1] == 0x2F && bom[2] == 0x76)
        {
            return Encoding.UTF7;
        }
        if (count > 2 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        {
            return Encoding.UTF8;
        }
        if (count > 1 && bom[0] == 0xFF && bom[1] == 0xFE)
        {
            return Encoding.Unicode;
        }
        if (count > 1 && bom[0] == 0xFE && bom[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode;
        }
        if (count > 3 && bom[0] == 0 && bom[1] == 0 && bom[2] == 0xFE && bom[3] == 0xFF)
        {
            return Encoding.UTF32;
        }

        // Default to
        return Encoding.ASCII;
    }

    /// <summary>
    ///     Detect when the file in the given path is a symbolic link.
    ///     WARNING: Could have false positive for any file with a reparse point that is not a symbolic link. 
    /// </summary>
    /// <param name="path">Full path to file to test.</param>
    /// <returns>True if Reparse Point is found.</returns>
    public static bool IsSymbolic(string path)
    {
        var pathInfo = new FileInfo(path);
        return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }
}