using System.Security.Claims;

namespace TestBlobStorage.Services.JwtService
{
    public interface IJwtService
    {
        string GenerateSecurityToken(string id, string email, IEnumerable<string> roles, IEnumerable<Claim> userClaims);
    }
}
