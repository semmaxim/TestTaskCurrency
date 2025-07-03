using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JwtLibrary.Services;

public class JwtService(IConfiguration configuration) : IJwtService
{
	public string GenerateToken(string user)
	{
		var description = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, user)]),
			Expires = DateTime.UtcNow + GetLifetime(),
			SigningCredentials = new SigningCredentials(GetSecurityKey(), SecurityAlgorithms.HmacSha256Signature),
			Issuer = GetIssuer(),
			Audience = GetAudience()
		};
		var handler = new JwtSecurityTokenHandler();
		var token = handler.CreateJwtSecurityToken(description);

		return handler.WriteToken(token);
	}

	public bool ValidateToken(string token, out ClaimsPrincipal? principal)
	{
		var tokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = GetSecurityKey(),
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidIssuer = GetIssuer(),
			ValidAudience = GetAudience()
		};
		var handler = new JwtSecurityTokenHandler();
		try
		{
			principal = handler.ValidateToken(token, tokenValidationParameters, out _);
			return true;
		}
		catch (Exception)
		{
			principal = null;
			return false;
		}
	}

	public SecurityKey GetSecurityKey() => new SymmetricSecurityKey(Convert.FromBase64String(configuration.GetValue<string>("Jwt:SecretKeyBase64")!));

	public TimeSpan GetLifetime() => TimeSpan.Parse(configuration.GetValue<string>("Jwt:Lifetime")!);

	private string GetIssuer() => configuration.GetValue<string>("Jwt:Issuer")!;

	private string GetAudience() => configuration.GetValue<string>("Jwt:Audience")!;

	public string GetCookieName() => configuration.GetValue<string>("Jwt:TokenCookieName")!;
}