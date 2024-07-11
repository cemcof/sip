namespace sip.Core.OutOfService;

public class OutOfServiceOptions
{
    public bool Enabled { get; set; }
    public string[] AllowIps { get; set; } = [];

    public string? Reason { get; set; }

    public Type Component { get; set; } = typeof(OutOfServicePage);

    public bool IsClientAllowed(HttpContext context)
    {
         var ip = context.Connection.RemoteIpAddress?.ToString();
         if (ip is not null)
         {
             return ip.CheckIp(AllowIps);
         }

         return false;
    }
}