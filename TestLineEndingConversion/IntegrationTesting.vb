Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TextFileConvert

Namespace TestLineEndingConversion
    <TestClass>
    Public Class IntegrationTesting
        <TestMethod>
        Sub TestDos2Ux()
            ' Where am I?
            Debug.WriteLine(Directory.GetCurrentDirectory())

            ' Do Conversion
            Dim exitCode As Integer = ConvertLineEndings.Dos2Ux("dos.txt", "testunix.txt").GetAwaiter().GetResult()

            ' Zero is success
            Assert.AreEqual(exitCode, 0)

            ' Compare the test file to the artifact file
            Assert.IsTrue(File.ReadAllBytes("unix.txt").SequenceEqual(File.ReadAllBytes("testunix.txt")))

            ' Clean up
            File.Delete("testunix.txt")
        End Sub

        <TestMethod>
        Sub TestUx2Dos()
            ' Where am I?
            Debug.WriteLine(Directory.GetCurrentDirectory())

            ' Do Conversion
            Dim exitCode As Integer = ConvertLineEndings.Ux2Dos("unix.txt", "testdos.txt").GetAwaiter().GetResult()

            ' Zero is success
            Assert.AreEqual(exitCode, 0)

            ' Compare the test file to the artifact file
            Assert.IsTrue(File.ReadAllBytes("dos.txt").SequenceEqual(File.ReadAllBytes("testdos.txt")))

            ' Clean up
            File.Delete("testdos.txt")
        End Sub
    End Class
End Namespace

