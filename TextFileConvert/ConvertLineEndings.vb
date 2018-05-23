Imports System.IO
Imports System.Text

Public Class ConvertLineEndings
    ''' <summary>
    ''' These are the different conversions we can handle in this library
    ''' </summary>
    Private Enum TextConvertMode
        Dos2Ux
        Ux2Dos
        Dos2Mac
        Mac2Dos
        Mac2Ux
        Ux2Mac
    End Enum

    Const CR = ChrW(13)
    Const LF = ChrW(10)

    ''' <summary>
    ''' Converts a DOS text file to have Unix line endings.
    ''' </summary>
    ''' <param name="originalFile">The file to convert.</param>
    ''' <param name="newFile">The name of a new file to create.</param>
    ''' <returns>Exit code.</returns>
    Public Shared Async Function Dos2Ux(originalFile As String, newFile As String) As Task(Of Integer)
        Return Await ReplaceLineEndings(originalFile, newFile, TextConvertMode.Dos2Ux)
    End Function

    ''' <summary>
    ''' Converts a DOS text file to have Unix line endings.
    ''' </summary>
    ''' <param name="originalFile">The file to convert.</param>
    ''' <param name="newFile">The name of a new file to create.</param>
    ''' <returns>Exit code.</returns>
    Public Shared Async Function Ux2Dos(originalFile As String, newFile As String) As Task(Of Integer)
        Return Await ReplaceLineEndings(originalFile, newFile, TextConvertMode.Ux2Dos)
    End Function

    ''' <summary>
    ''' Loads a whole text file in to memory, Performs a find\replace, and writes a new file.
    ''' </summary>
    ''' <param name="originalFile">The file to convert.</param>
    ''' <param name="newFile">The name of a new file to create.</param>
    ''' <param name="convertMode">This is the type of conversion we are going to perform</param>
    ''' <returns>Exit code.</returns>
    Private Shared Async Function ReplaceLineEndings(originalFile As String, newFile As String, convertMode As TextConvertMode) As Task(Of Integer)
        Dim convertedText As New StringBuilder
        Dim oldFileStream As FileStream = Nothing
        Try
            oldFileStream = New FileStream(originalFile, FileMode.Open)
            Using oldFile As New StreamReader(oldFileStream)
                Do Until oldFile.EndOfStream
                    Dim readBuffer(2) As Char
                    Dim readChars As Integer = Await oldFile.ReadAsync(readBuffer, 0, 1)
                    If readChars < 1 Then Exit Do
                    Select Case convertMode
                        Case TextConvertMode.Dos2Ux
                            If readBuffer(0) = CR AndAlso oldFile.Peek() = 10 Then
                                ' Strip out CR chars if followed by LF
                                readBuffer(0) = Nothing
                            End If
                        Case TextConvertMode.Ux2Dos
                            If readBuffer(0) = CR AndAlso oldFile.Peek() = 10 Then
                                ' This is a DOS line ending, keep it.
                                Dim tempBuffer(1) As Char
                                Await oldFile.ReadAsync(tempBuffer, 0, 1)
                                readBuffer(1) = tempBuffer(0)
                            ElseIf readBuffer(0) = ChrW(10) Then
                                ' Add preceeding CR
                                readBuffer(0) = CR
                                readBuffer(1) = LF
                            End If
                        Case Else
                            Debug.Print("Unimplemented text conversion mode")
                            Return -1
                    End Select
                    convertedText.Append(readBuffer)
                Loop
            End Using
            oldFileStream = Nothing
        Catch ex As Exception
            Debug.Print("Error: " & ex.Message & vbCrLf & "Number: " & ex.HResult)
            Return ex.HResult
        Finally
            If oldFileStream IsNot Nothing Then oldFileStream.Dispose()
        End Try

        'Write the result out to a new file
        Try
            Await File.WriteAllTextAsync(newFile, convertedText.ToString())
        Catch ex As Exception
            Debug.Print("Error: " & ex.Message & vbCrLf & "Number: " & ex.HResult)
            Return ex.HResult
        End Try

        Return 0 ' Exit status 0 is a good thing
    End Function
End Class