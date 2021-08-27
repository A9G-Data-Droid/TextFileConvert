Imports System.IO
Imports TextFileConvert



Module Ux2Dos

    'Public Async Function Main(args As String()) As Task(Of Integer)
    Public Function Main(args As String()) As Integer
        Dim exitCode As Integer
        If args.Length <> 2 OrElse String.IsNullOrEmpty(args(0)) OrElse String.IsNullOrEmpty(args(1)) Then
            PrintUsage(Path.GetFileName(Environment.GetCommandLineArgs()(0)))
            exitCode = -1
        Else
            Console.WriteLine("Perform UNIX to DOS conversion")
            Try
                'exitCode = Await ConvertLineEndings.Ux2Dos(args(0), args(1))
                exitCode = ConvertLineEndings.Ux2Dos(args(0), args(1)).GetAwaiter().GetResult()
                Console.WriteLine("Conversion complete.")
            Catch ex As Exception
                Console.WriteLine("Conversion failed.")
                exitCode = ex.HResult
            End Try
        End If

        Return exitCode
    End Function
End Module
