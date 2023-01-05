using System.Text.RegularExpressions;

public class ParameterFormattingProcessor
{
    Regex regex = new Regex("{.*?}");

    public ParameterFormattingInfo ParseParameterFormatting(string formatting)
    {
        var info = new ParameterFormattingInfo();

        if (formatting != null)
        {
            var matches = regex.Matches(formatting);

            foreach (Match match in matches)
            {
                var matchValue = CleanMatchValue(match.Value);
                if (!info.ParameterNames.Contains(matchValue))
                {
                    info.ParameterNames.Add(matchValue);
                }
            }

            var finalFormat = formatting;

            for (var i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                var matchValue = CleanMatchValue(match.Value);

                finalFormat = finalFormat.Remove(match.Index, match.Length);

                var textToInsert = $"{{{info.ParameterNames.IndexOf(matchValue)}}}";
                finalFormat = finalFormat.Insert(match.Index, textToInsert);
            }

            info.Format = finalFormat;
        }

        return info;
    }

    static string CleanMatchValue(string matchValue) =>
        matchValue.Replace("{", string.Empty).Replace("}", string.Empty);
}