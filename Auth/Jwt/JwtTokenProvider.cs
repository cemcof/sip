using System.IdentityModel.Tokens.Jwt;

namespace sip.Auth.Jwt;

public class JwtTokenProvider(IOptionsMonitor<JwtProducerOptions> jwtOptions, TimeProvider timeProvider)
{
    private string CreateToken(IEnumerable<Claim> claims, TimeSpan? expireAfter = null)
    {
        var opts = jwtOptions.CurrentValue;
        expireAfter ??= opts.DefaultExpireAfter;

        // If tokens is volatile, we embed special claim that determines it
        claims = claims.Concat(new Claim[] { new(JwtProducerOptions.JWT_VOLATILE_CLAIM_TYPE, string.Empty) });

        var secKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(opts.IssuerSigningKey));
        var issuer = opts.Issuer;
        var expires = timeProvider.DtUtcNow() + expireAfter;

        // TODO - from https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme),
            Expires = expires,
            Issuer = issuer,
            Audience = issuer, // Generating token for self => issuer is equal to audience
            SigningCredentials = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string CreateToken(JwtProducerProperties properties)
    {
        var clms = new List<Claim>();

        if (properties.TokenVolatile)
        {
            clms.Add(new Claim(JwtProducerOptions.JWT_VOLATILE_CLAIM_TYPE, string.Empty));
        }

        if (!string.IsNullOrWhiteSpace(properties.Purpose))
        {
            clms.Add(new(JwtProducerOptions.JWT_FOR_PURPOSE_CLAIM_TYPE, properties.Purpose));
        }

        if (!string.IsNullOrWhiteSpace(properties.RegenerationEmail))
        {
            clms.Add(new(JwtProducerOptions.JWT_REGEN_EMAIL_CLAIM_TYPE, properties.RegenerationEmail));
        }

        return CreateToken(properties.IncludeClaims.Concat(clms), properties.ExpireAfter);
    }

    public string CreateToken(IEnumerable<Claim> claims, string regenEmail, string purpose,
        TimeSpan? expireAfter = null, bool tokenVolatile = true)
    {
        return CreateToken(new JwtProducerProperties()
        {
            ExpireAfter = expireAfter,
            TokenVolatile = tokenVolatile,
            RegenerationEmail = regenEmail,
            Purpose = purpose,
            IncludeClaims = claims
        });
    }
}