using System;
using System.IO;
using System.Threading.Tasks;
using TextFileConvert;

namespace dos2ux;

public static class Dos2Ux
{
    // Public Async Function Main(args As String()) As Task(Of Integer)
    public static async Task<int> Main(string[] args) 
    {
        int exitCode;
        if (args.Length != 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]))
        {
            Common.PrintUsage(Path.GetFileName(Environment.GetCommandLineArgs()[0]));
            exitCode = -1;
        }
        else
        {
            Console.WriteLine("Perform DOS to UNIX conversion");
            try
            {
                // exitCode = Await ConvertLineEndings.Dos2Ux(args(0), args(1))
                exitCode = await ConvertLineEndings.Dos2Ux(args[0], args[1]);
                Console.WriteLine("Conversion complete: " + args[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Conversion failed.");
                exitCode = ex.HResult;
            }
        }

        return exitCode;
    }
}