using Xunit;

public class ParameterFormattingProcessorTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void ParseEmptyFormatting(string input, string expectedOutput)
    {
        var processor = new ParameterFormattingProcessor();

        var info = processor.ParseParameterFormatting(input);

        Assert.NotNull(info);
        Assert.Equal(expectedOutput, info.Format);
    }

    [Fact]
    public void ParseSimpleFormatting()
    {
        var processor = new ParameterFormattingProcessor();

        var info = processor.ParseParameterFormatting("This is a {fileName}");

        Assert.NotNull(info);

        Assert.Equal("This is a {0}", info.Format);
        Assert.Equal("fileName", info.ParameterNames[0]);
    }

    [Fact]
    public void ParseComplexFormatting()
    {
        var processor = new ParameterFormattingProcessor();

        var info = processor.ParseParameterFormatting("This is a {fileName} test with id = '{id}' and {fileName} but don't replace fileName");

        Assert.Equal("This is a {0} test with id = '{1}' and {0} but don't replace fileName", info.Format);
        Assert.Equal("fileName", info.ParameterNames[0]);
        Assert.Equal("id", info.ParameterNames[1]);
    }
}