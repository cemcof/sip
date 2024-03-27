namespace sip.Utils;

public static class PathLib
{
    // Naive quick implementation to get the name of a file from a path
    // Regardless of the OS
    public static char WinSep = '\\';
    public static char UnixSep = '/';
    
    public static string GetName(ReadOnlySpan<char> path)
    {
        var tralingSep = path[^1] == WinSep || path[^1] == UnixSep;
        if (tralingSep)
            path = path[..^1];
        
        var separatorPos = path.LastIndexOf(UnixSep);
        if (separatorPos == -1)
            // Windows path
            separatorPos = path.LastIndexOf(WinSep);
        
        var result = path[(separatorPos + 1)..];
        if (result.Length == 0 || result[0] == '.')
            throw new ArgumentException("Path must not be empty", nameof(path));
        
        return result.ToString();
    }
}