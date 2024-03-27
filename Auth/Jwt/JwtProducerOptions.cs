namespace sip.Auth.Jwt;

public class JwtProducerOptions
{
    public const string JWT_REGEN_EMAIL_CLAIM_TYPE = "JWT_REGENERATE_TO_EMAIL";
    public const string JWT_FOR_PURPOSE_CLAIM_TYPE = "JWT_GENERATED_FOR_PURPOSE";
    public const string JWT_VOLATILE_CLAIM_TYPE = "JWT_VOLATILE";
    [Required] public string Issuer { get; set; } = null!;
    [Required] public string IssuerSigningKey { get; set; } = null!;
    public TimeSpan DefaultExpireAfter { get; set; } = TimeSpan.FromDays(1);
}