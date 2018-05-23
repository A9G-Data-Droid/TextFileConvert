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
        Const cr As Char = ChrW(13)
        Const lf As Char = ChrW(10)

        ' Attempt to detect encoding
        Dim fileEncoding As Encoding = GetEncoding(originalFile)
        If fileEncoding Is Nothing Then Return 4
        Debug.Print(fileEncoding.ToString())

        Dim convertedText As New StringBuilder
        Dim oldFileStream As FileStream = Nothing
        Try
            oldFileStream = New FileStream(originalFile, FileMode.Open)
            Using oldFile As New StreamReader(oldFileStream, fileEncoding, True)
                Do Until oldFile.EndOfStream  ' Read through the whole file
                    Dim readBuffer(0) As Char
                    Dim readChars As Integer = Await oldFile.ReadAsync(readBuffer, 0, 1)
                    If readChars < 1 Then Exit Do  ' Short circuit 
                    Select Case convertMode
                        Case TextConvertMode.Dos2Ux
                            If readBuffer(0) = cr AndAlso oldFile.Peek() = 10 Then
                                ' Strip out CR chars if followed by LF
                                Await oldFile.ReadAsync(readBuffer, 0, 1)
                            End If
                        Case TextConvertMode.Ux2Dos
                            If readBuffer(0) = cr AndAlso oldFile.Peek() = 10 Then
                                ReDim Preserve readBuffer(1)
                                ' This is a DOS line ending, keep it.
                                Dim tempBuffer(1) As Char
                                Await oldFile.ReadAsync(tempBuffer, 0, 1)
                                readBuffer(1) = tempBuffer(0)
                            ElseIf readBuffer(0) = lf Then
                                ReDim readBuffer(1)
                                ' Add preceeding CR
                                readBuffer(0) = cr
                                readBuffer(1) = lf
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
            Debug.Print("Error: " & ex.Message & Environment.NewLine & "Number: " & ex.HResult.ToString)
            Return ex.HResult
        Finally
            If oldFileStream IsNot Nothing Then oldFileStream.Dispose()
        End Try

        'Write the result out to a new file
        Try
            File.WriteAllText(newFile, convertedText.ToString(), New UTF8Encoding(False))
        Catch ex As Exception
            Debug.Print("Error: " & ex.Message & Environment.NewLine & "Number: " & ex.HResult.ToString)
            Return ex.HResult
        End Try

        Return 0 ' Exit status 0 is a good thing
    End Function

    ''' <summary>
    '''     Attempt to detect the encoding of a file.
    ''' </summary>
    ''' <param name="filename">The file to get the encoding pattern from.</param>
    ''' <returns>Encoding type, defaults to ASCII.</returns>
    Public Shared Function GetEncoding(filename As String) As Encoding
        Dim bom = New Byte(3) {}
        Try  ' to read BOM
            Using file = New FileStream(filename, FileMode.Open, FileAccess.Read)
                file.Read(bom, 0, 4)
            End Using
        Catch ex As Exception
            Debug.Print("Error: " & ex.Message & Environment.NewLine & "Number: " & ex.HResult.ToString)
            Return Nothing
        End Try

        ' Detect BOM type
        If bom(0) = &H2B AndAlso bom(1) = &H2F AndAlso bom(2) = &H76 Then Return Encoding.UTF7
        If bom(0) = &HEF AndAlso bom(1) = &HBB AndAlso bom(2) = &HBF Then Return Encoding.UTF8
        If bom(0) = &HFF AndAlso bom(1) = &HFE Then Return Encoding.Unicode
        If bom(0) = &HFE AndAlso bom(1) = &HFF Then Return Encoding.BigEndianUnicode
        If bom(0) = 0 AndAlso bom(1) = 0 AndAlso bom(2) = &HFE AndAlso bom(3) = &HFF Then Return Encoding.UTF32

        ' Default to
        Return Encoding.ASCII
    End Function
End Class
