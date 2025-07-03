using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace JwtLibrary.Services;

public interface IJwtService
{
	public TimeSpan GetLifetime();

	public SecurityKey GetSecurityKey();

	public string GetCookieName();

	public string GenerateToken(string user);

	public bool ValidateToken(string token, out ClaimsPrincipal? principal);
}