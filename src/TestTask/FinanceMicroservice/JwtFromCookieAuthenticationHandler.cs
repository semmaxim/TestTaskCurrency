using System.Text.Encodings.Web;
using JwtLibrary.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace FinanceMicroservice;

public class JwtFromCookieAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IJwtService jwtService)
	: AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
	public const string AuthenticationSchemeName = "JwtFromCookies";

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var token = Context.Request.Cookies[jwtService.GetCookieName()];
		if (String.IsNullOrWhiteSpace(token))
			return AuthenticateResult.Fail("Не найден токен в cookies");

		if (!jwtService.ValidateToken(token, out var claimsPrincipal))
			return AuthenticateResult.Fail("Токен не прошёл проверку.");

		var ticket = new AuthenticationTicket(claimsPrincipal!, Scheme.Name);
		return AuthenticateResult.Success(ticket);
	}
}