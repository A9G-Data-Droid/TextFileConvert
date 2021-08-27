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
    ''' <param name="originalFile">The file to convert.</param>
    ''' <param name="newFile">The name of a new file to create.</param>
    ''' <param name="convertMode">This is the type of conversion we are going to perform</param>
    ''' <returns>Exit code.</returns>
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

        Return 0 ' Exit status 0 is a good thing
    End Function

    Private Shared Async Function GetConvertedText(originalFile As FileStream, fileEncoding As Encoding, convertMode As TextConvertMode) As Task(Of String)
        Const CR As Char = ChrW(13)
        Const LF As Char = ChrW(10)

        Dim convertedLines As New StringBuilder
        Using oldFile As New StreamReader(originalFile, fileEncoding, True)
            Do Until oldFile.EndOfStream  ' Read through the whole file
                Dim readBuffer(0) As Char
                Dim readChars As Integer = Await oldFile.ReadAsync(readBuffer, 0, 1)
                If readChars >= 1 Then
                    Select Case convertMode
                        Case TextConvertMode.Dos2Ux
                            If readBuffer(0) = CR AndAlso oldFile.Peek() = 10 Then
                                ' Strip out CR chars if followed by LF
                                Await oldFile.ReadAsync(readBuffer, 0, 1)
                            End If
                        Case TextConvertMode.Ux2Dos
                            If readBuffer(0) = CR AndAlso oldFile.Peek() = 10 Then
                                ReDim Preserve readBuffer(1)
                                ' This is a DOS line ending, keep it.
                                Dim tempBuffer(1) As Char
                                Await oldFile.ReadAsync(tempBuffer, 0, 1)
                                readBuffer(1) = tempBuffer(0)
                            ElseIf readBuffer(0) = LF Then
                                ReDim readBuffer(1)
                                ' Add preceeding CR
                                readBuffer(0) = CR
                                readBuffer(1) = LF
                            End If
                    End Select

                    'Yield readBuffer
                    convertedLines.Append(readBuffer)
                End If
            Loop
        End Using

        Return convertedLines.ToString
    End Function
End Class
