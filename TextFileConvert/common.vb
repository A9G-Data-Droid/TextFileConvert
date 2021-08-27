Imports System.IO
Imports System.Text



Public Module common
    ''' <summary>
    ''' This is the command line help that is displayed when invalid parameters are given.
    ''' </summary>
    ''' <param name="programName"></param>
    Public Sub PrintUsage(programName As String)
        Console.WriteLine(
"NAME
    {0} - Text file format converter

SYNOPSIS
    {0} oldfilename newfilename

DESCRIPTION
    {0} reads oldfilename and writes out newfilename, converting line endings.", programName)
    End Sub

    ''' <summary>
    '''     Attempt to detect the encoding of a file.
    ''' </summary>
    ''' <param name="filename">The file to get the encoding pattern from.</param>
    ''' <returns>Encoding type, defaults to ASCII.</returns>
    Public Async Function GetEncoding(theFile As FileStream) As Task(Of Encoding)
        Dim bom As Byte() = New Byte(3) {}
        Await theFile.ReadAsync(bom, 0, 4)

        ' Detect BOM type
        If bom(0) = &H2B AndAlso bom(1) = &H2F AndAlso bom(2) = &H76 Then
            Return Encoding.UTF7
        ElseIf bom(0) = &HEF AndAlso bom(1) = &HBB AndAlso bom(2) = &HBF Then
            Return Encoding.UTF8
        ElseIf bom(0) = &HFF AndAlso bom(1) = &HFE Then
            Return Encoding.Unicode
        ElseIf bom(0) = &HFE AndAlso bom(1) = &HFF Then
            Return Encoding.BigEndianUnicode
        ElseIf bom(0) = 0 AndAlso bom(1) = 0 AndAlso bom(2) = &HFE AndAlso bom(3) = &HFF Then
            Return Encoding.UTF32
        Else
            ' Default to
            Return Encoding.ASCII
        End If
    End Function

    ''' <summary>
    '''     Detect when the file in the given path is a symbolic link.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns>True if </returns>
    Public Function IsSymbolic(path As String) As Boolean
        Dim pathInfo As New FileInfo(path)
        Return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint)
    End Function
End Module
