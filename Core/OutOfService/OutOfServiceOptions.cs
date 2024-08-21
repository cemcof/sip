namespace sip.Core.OutOfService;

public class OutOfServiceOptions
{
    public bool Enabled { get; set; }
    public IPAddr[] AllowIps { get; set; } = [];

    public string? Reason { get; set; }

    public Type Component { get; set; } = typeof(OutOfServicePage);

    public bool IsClientAllowed(HttpContext context)
    {
         var ipStr = context.Connection.RemoteIpAddress?.ToString();
         if (!string.IsNullOrWhiteSpace(ipStr) && IPAddr.TryParse(ipStr, out var ip))
             return ip.CheckAgainst(AllowIps);

         return false;
    }
}