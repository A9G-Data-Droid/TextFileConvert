using System.IO;

namespace TextFileConvert.Converters;

public interface ITextConverter
{
    /// <summary>
    /// This is where the conversion logic lives.
    /// </summary>
    /// <param name="originalFile">The file to convert.</param>
    /// <param name="newFile">The file to write with new line endings</param>
    public void WriteConvertedText(FileStream originalFile, FileStream newFile);
}