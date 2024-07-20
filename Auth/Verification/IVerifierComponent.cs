using Microsoft.AspNetCore.Components;

namespace sip.Auth.Verification;

public interface IVerifierComponent
{
    EventCallback OnVerified { get; set; }
    ClaimsPrincipal User { get; set; }
}