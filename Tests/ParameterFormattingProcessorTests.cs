using NUnit.Framework;

[TestFixture]
internal class ParameterFormattingProcessorTests
{
    [TestCase(null, "")]
    [TestCase("", "")]
    public void ParseEmptyFormatting(string input, string expectedOutput)
    {
        var processor = new ParameterFormattingProcessor();

        var info = processor.ParseParameterFormatting(input);
        
        Assert.IsNotNull(info);
        Assert.AreEqual(expectedOutput, info.Format);
    }

    [Test]
    public void ParseSimpleFormatting()
    {
        var processor = new ParameterFormattingProcessor();

        var info = processor.ParseParameterFormatting("This is a {fileName}");

        Assert.IsNotNull(info);

        Assert.AreEqual("This is a {0}", info.Format);
        Assert.AreEqual(info.ParameterNames[0], "fileName");
    }

    [Test]
    public void ParseComplexFormatting()
    {
        var processor = new ParameterFormattingProcessor();

        var info = processor.ParseParameterFormatting("This is a {fileName} test with id = '{id}' and {fileName} but don't replace fileName");

        Assert.AreEqual("This is a {0} test with id = '{1}' and {0} but don't replace fileName", info.Format);
        Assert.AreEqual(info.ParameterNames[0], "fileName");
        Assert.AreEqual(info.ParameterNames[1], "id");
    }
}