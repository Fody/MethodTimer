using System.Collections.Generic;

public static class IgnoreCodes
{
    public static IEnumerable<string> GetIgnoreCoders()
    {
#if NET471
        return System.Linq.Enumerable.Empty<string>();
#endif
#if NETCOREAPP2_1
        return new[] { "0x80131869" };
#endif
    }
}