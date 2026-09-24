
public class ParameterFormattingProcessorTests
{
    [Test]
    [Arguments(null, "")]
    [Arguments("", "")]
    public async Task ParseEmptyFormatting(string input, string expectedOutput)
    {
        var processor = new ParameterFormattingProcessor();

        var info = processor.ParseParameterFormatting(input);

        await Assert.That(info).IsNotNull();
        await Assert.That(info.Format).IsEqualTo(expectedOutput);
    }

    [Test]
    public async Task ParseSimpleFormatting()
    {
        var processor = new ParameterFormattingProcessor();

        var info = processor.ParseParameterFormatting("This is a {fileName}");

        await Assert.That(info).IsNotNull();

        await Assert.That(info.Format).IsEqualTo("This is a {0}");
        await Assert.That(info.ParameterNames[0]).IsEqualTo("fileName");
    }

    [Test]
    public async Task ParseComplexFormatting()
    {
        var processor = new ParameterFormattingProcessor();

        var info = processor.ParseParameterFormatting("This is a {fileName} test with id = '{id}' and {fileName} but don't replace fileName");

        await Assert.That(info.Format).IsEqualTo("This is a {0} test with id = '{1}' and {0} but don't replace fileName");
        await Assert.That(info.ParameterNames[0]).IsEqualTo("fileName");
        await Assert.That(info.ParameterNames[1]).IsEqualTo("id");
    }
}