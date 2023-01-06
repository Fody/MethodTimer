using System.Collections.Generic;

public class ParameterFormattingInfo
{
    public ParameterFormattingInfo()
    {
        Format = string.Empty;
        ParameterNames = new();
    }

    public string Format { get; set; }

    public List<string> ParameterNames { get; set; }
}