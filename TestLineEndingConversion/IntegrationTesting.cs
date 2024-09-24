using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextFileConvert;



namespace TestLineEndingConversion;

[TestClass]
public class IntegrationTesting
{
    private const string TestUnixTxt = "testunix.txt";
    private const string TestDosTxt = "testdos.txt";

    [TestMethod]
    public async Task TestDos2Ux()
    {
        // Where am I?
        Debug.WriteLine(Directory.GetCurrentDirectory());
        if(File.Exists(TestUnixTxt))
            File.Delete(TestUnixTxt);

        // ensure we start well
        var precondition = await File.ReadAllTextAsync("dos.txt");
        Assert.IsTrue(precondition.Contains("\r\n"));

        // Do Conversion
        var exitCode = await ConvertLineEndings.Dos2Ux("dos.txt", TestUnixTxt);

        // Zero is success
        Assert.AreEqual(exitCode, 0);

        // Compare the test file to the artifact file
        var result = await File.ReadAllTextAsync(TestUnixTxt);
        Assert.IsFalse(result.Contains("\r\n"));
        Assert.AreEqual(await File.ReadAllTextAsync("unix.txt"), result);

        // Clean up
        File.Delete(TestUnixTxt);
    }

    [TestMethod]
    public async Task TestUx2Dos()
    {
        // Where am I?
        Debug.WriteLine(Directory.GetCurrentDirectory());

        if(File.Exists(TestDosTxt))
            File.Delete(TestDosTxt);
        
        // ensure we start well
        var precondition = await File.ReadAllTextAsync("unix.txt");
        Assert.IsFalse(precondition.Contains("\r\n"));
        
        // Do Conversion
        int exitCode = await ConvertLineEndings.Ux2Dos("unix.txt", TestDosTxt);

        // Zero is success
        Assert.AreEqual(exitCode, 0);

        // Compare the test file to the artifact file
        var result = await File.ReadAllTextAsync(TestDosTxt);
        Assert.IsTrue(result.Contains("\r\n"));
        Assert.AreEqual(await File.ReadAllTextAsync("dos.txt"), result);

        // Clean up
        File.Delete(TestDosTxt);
    }
}