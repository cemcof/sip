using System.Security.Principal;

namespace sip.Auth.Verification;

public interface IVerificator
{
    public const string USER_VERIFICATION_POLICY = "USER_VERIFICATION_POLICY";
    
    /// <summary>
    /// Whether the user is verified or not.
    /// </summary>
    /// 
    /// <param name="user"></param>
    /// <returns></returns>
    Task<bool> IsVerifiedAsync(ClaimsPrincipal user);
    
    /// <summary>
    /// Component that interacts with the user and handles the verification process.
    /// </summary>
    /// <returns></returns>
    Type GetVerificationComponent();

    /// <summary>
    /// Marks the user as verified.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task SetVerifiedAsync(ClaimsPrincipal user);
}