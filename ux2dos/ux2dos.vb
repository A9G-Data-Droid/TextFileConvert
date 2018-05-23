Imports TextFileConvert

Module Ux2Dos

    Sub Main(args As String())
        Dim exitCode As Integer
        If args.Length <> 2 OrElse args(0) = Nothing OrElse args(1) = Nothing Then
            PrintInstructions()
            exitCode = 87
        Else
            Console.WriteLine("Perform UNIX to DOS conversion")
            Dim runAsync As Task(Of Integer) = ConvertLineEndings.Ux2Dos(args(0), args(1))
            Try
                runAsync.Wait()
            Catch ex As Exception
                ' We handle failure later
            End Try

            If runAsync.Status = TaskStatus.Faulted Then
                Console.WriteLine("Conversion failed.")
                exitCode = 188
            Else
                Console.WriteLine("Conversion complete.")
                exitCode = runAsync.Result
            End If
        End If

        Environment.ExitCode = exitCode
    End Sub

    Private Sub PrintInstructions()
        Dim helpMessage As String
        helpMessage =
            "NAME
    ux2dos - Convert ASCII file format

SYNOPSIS
    ux2dos oldfilename newfilename

DESCRIPTION
    ux2dos reads oldfilename and writes out newfilename, converting line endings from UNIX (LF) to DOS (CRLF)
            "
        Console.WriteLine(helpMessage)
    End Sub

End Module
