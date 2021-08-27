Imports System.IO
Imports TextFileConvert



Module Dos2Ux

    'Public Async Function Main(args As String()) As Task(Of Integer)
    Public Function Main(args As String()) As Integer
        Dim exitCode As Integer
        If args.Length <> 2 OrElse String.IsNullOrEmpty(args(0)) OrElse String.IsNullOrEmpty(args(1)) Then
            PrintUsage(Path.GetFileName(Environment.GetCommandLineArgs()(0)))
            exitCode = -1
        Else
            Console.WriteLine("Perform DOS to UNIX conversion")
            Try
                'exitCode = Await ConvertLineEndings.Dos2Ux(args(0), args(1))
                exitCode = ConvertLineEndings.Dos2Ux(args(0), args(1)).GetAwaiter().GetResult()
                Console.WriteLine("Conversion complete: " & args(1))
            Catch ex As Exception
                Console.WriteLine("Conversion failed.")
                exitCode = ex.HResult
            End Try
        End If

        Return exitCode
    End Function
End Module
