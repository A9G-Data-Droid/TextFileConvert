Imports System.IO
Imports System.Text



Public Class ConvertLineEndings
    ''' <summary>
    '''     These are the different conversions that this library can perform.
    ''' </summary>
    Private Enum TextConvertMode
        Dos2Ux
        Ux2Dos
        'Dos2Mac
        'Mac2Dos
        'Mac2Ux
        'Ux2Mac
    End Enum

    ''' <summary>
    '''     Converts a DOS text file to have Unix line endings.
    ''' </summary>
    ''' <param name="originalFile">The file to convert.</param>
    ''' <param name="newFile">The name of a new file to create.</param>
    ''' <returns>Exit code.</returns>
    Public Shared Async Function Dos2Ux(originalFile As String, newFile As String) As Task(Of Integer)
        Return Await ReplaceLineEndings(originalFile, newFile, TextConvertMode.Dos2Ux)
    End Function

    ''' <summary>
    '''     Converts a DOS text file to have Unix line endings.
    ''' </summary>
    ''' <param name="originalFile">The file to convert.</param>
    ''' <param name="newFile">The name of a new file to create.</param>
    ''' <returns>Exit code.</returns>
    Public Shared Async Function Ux2Dos(originalFile As String, newFile As String) As Task(Of Integer)
        Return Await ReplaceLineEndings(originalFile, newFile, TextConvertMode.Ux2Dos)
    End Function

    ''' <summary>
    '''     Loads a whole text file in to memory, Performs a find\replace, and writes a new file.
    ''' </summary>
    ''' <param name="originalFile">The file path to convert.</param>
    ''' <param name="newFile">The name of a new file to create.</param>
    ''' <param name="convertMode">This is the type of conversion we are going to perform.</param>
    ''' <returns>Exit code. 0 is success. -1 is a symbolic link.</returns>
    Private Shared Async Function ReplaceLineEndings(originalFile As String, newFile As String,
                                                     convertMode As TextConvertMode) As Task(Of Integer)

        Try
            ' Do not attempt to work on symbolic links
            If IsSymbolic(originalFile) Then Return -1

            Using oldFileStream As New FileStream(originalFile, FileMode.Open, FileAccess.Read)
                ' Attempt to detect encoding we will use for reading and writing
                Dim fileEncoding As Encoding = Await GetEncoding(oldFileStream)
                Debug.Print(fileEncoding.ToString())

                ' Rewind stream
                oldFileStream.Position = 0
                Await oldFileStream.FlushAsync

                ' Reading and writing is done in this one line
                File.WriteAllText(newFile, Await GetConvertedText(oldFileStream, fileEncoding, convertMode), fileEncoding)
            End Using
        Catch ex As Exception
            Debug.Print("Error: " & ex.Message & Environment.NewLine & "Number: " & ex.HResult.ToString)
            Return ex.HResult
        End Try

        Return 0
    End Function

    ''' <summary>
    '''     This is where the actual conversion logic lives.
    ''' </summary>
    ''' <param name="originalFile">The file you want to convert.</param>
    ''' <param name="fileEncoding">The encoding you want to read that file as.</param>
    ''' <param name="convertMode">The type of conversion you want to perform.</param>
    ''' <returns>The full text of the file with new line endings.</returns>
    Private Shared Async Function GetConvertedText(originalFile As FileStream, fileEncoding As Encoding, convertMode As TextConvertMode) As Task(Of String)
        Const CR As Char = ChrW(13)  '  Carriage Return
        Const LF As Char = ChrW(10)  '  Line Feed

        Dim convertedLines As New StringBuilder
        Using oldFile As New StreamReader(originalFile, fileEncoding, True)
            Select Case convertMode
                Case TextConvertMode.Dos2Ux
                    Do Until oldFile.EndOfStream
                        Dim readBuffer(0) As Char
                        Dim readChars As Integer = Await oldFile.ReadAsync(readBuffer, 0, 1)
                        If readChars >= 1 Then
                            ' Look for Dos line endings
                            If readBuffer(0) = CR AndAlso oldFile.Peek() = 10 Then
                                ' Strip out CR chars if followed by LF
                                Await oldFile.ReadAsync(readBuffer, 0, 1)
                            End If

                            'Yield readBuffer
                            convertedLines.Append(readBuffer)
                        End If
                    Loop
                Case TextConvertMode.Ux2Dos
                    Do Until oldFile.EndOfStream
                        Dim readBuffer(0) As Char
                        Dim readChars As Integer = Await oldFile.ReadAsync(readBuffer, 0, 1)
                        If readChars >= 1 Then
                            ' Check for CR first to avoid doubling the CR character when LF is found
                            If readBuffer(0) = CR AndAlso oldFile.Peek() = 10 Then
                                ' This is a DOS line ending, keep it.
                                ReDim Preserve readBuffer(1)
                                Dim tempBuffer(1) As Char
                                Await oldFile.ReadAsync(tempBuffer, 0, 1)
                                readBuffer(1) = tempBuffer(0)
                            ElseIf readBuffer(0) = LF Then
                                ' This is a Unix line ending. Add preceeding CR.
                                ReDim readBuffer(1)
                                readBuffer(0) = CR
                                readBuffer(1) = LF
                            End If

                            'Yield readBuffer
                            convertedLines.Append(readBuffer)
                        End If
                    Loop
            End Select
        End Using

        Return convertedLines.ToString
    End Function
End Class
