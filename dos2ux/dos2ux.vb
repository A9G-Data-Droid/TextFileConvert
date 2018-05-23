Imports TextFileConvert

Module dos2ux
    Sub Main(args As String())
        Dim exitCode As Integer
        If args.Length <> 2 OrElse args(0) = Nothing OrElse args(1) = Nothing Then
            PrintInstructions()
            exitCode = 87
        Else
            Console.WriteLine("Perform DOS to UNIX conversion")
            Dim runAsync As Task(Of Integer) = ConvertLineEndings.Dos2Ux(args(0), args(1))
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
    dos2ux - Convert ASCII file format

SYNOPSIS
    dos2ux oldfilename newfilename

DESCRIPTION
    dos2ux reads oldfilename and writes out newfilename, converting line endings from UNIX (LF) to DOS (CRLF)
            "
        Console.WriteLine(helpMessage)
    End Sub
End Module
