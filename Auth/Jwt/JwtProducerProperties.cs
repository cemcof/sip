namespace sip.Auth.Jwt;

public class JwtProducerProperties
{
    /// <summary>
    /// Collection of claims to be inserted to the token body.
    /// </summary>
    public IEnumerable<Claim> IncludeClaims { get; set; } = Enumerable.Empty<Claim>();

    /// <summary>
    /// After what amount of time the token will expire
    /// </summary>
    public TimeSpan? ExpireAfter { get; set; }

    /// <summary>
    /// If set, token will be considered as regeneratable.
    /// When user supplies expired regeneratable token, email with same refreshed token is sent to the given email address.
    /// </summary>
    public string? RegenerationEmail { get; set; }

    /// <summary>
    /// If set, special claim identifiying purpose of this token is added.
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Volatile tokens are less secure tokens, because they are expected to be either sent via insecure medium (email) or
    /// given to the user that can easily accidentally hand it to someone else.
    ///
    /// Such token contains a special flag claim that determines this. Authorization policies can then decide whether they will trust
    /// volatile tokens or not. 
    /// </summary>
    public bool TokenVolatile { get; set; } = true;
}