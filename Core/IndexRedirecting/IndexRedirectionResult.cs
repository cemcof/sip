namespace sip.Core.IndexRedirecting;

public record IndexRedirectionResult(
    string TargetUrl,
    bool Refresh
    );