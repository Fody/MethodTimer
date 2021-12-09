using System.Collections.Generic;

public static class IgnoreCodes
{
    public static IEnumerable<string> GetIgnoreCoders()
    {
        return new[] { "0x80131869" };
    }
}