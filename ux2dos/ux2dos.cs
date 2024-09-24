using System;
using System.IO;
using TextFileConvert;

namespace ux2dos
{



    static class Ux2Dos
    {

        // Public Async Function Main(args As String()) As Task(Of Integer)
        public static int Main(string[] args)
        {
            int exitCode;
            if (args.Length != 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]))
            {
                Common.PrintUsage(Path.GetFileName(Environment.GetCommandLineArgs()[0]));
                exitCode = -1;
            }
            else
            {
                Console.WriteLine("Perform UNIX to DOS conversion");
                try
                {
                    // exitCode = Await ConvertLineEndings.Ux2Dos(args(0), args(1))
                    exitCode = ConvertLineEndings.Ux2Dos(args[0], args[1]).GetAwaiter().GetResult();
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
}