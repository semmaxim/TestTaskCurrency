using JwtLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Users;
using UsersMicroservice.Services;

namespace UsersMicroservice.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController(UsersService.UsersServiceClient usersClient, IJwtService jwtService, IPasswordService passwordService) : ControllerBase
	{
		[HttpPost("register")]
		public async Task<IActionResult> Register(string login, string password, List<string> codes)
		{
			var isUserExistsRequest = new IsUserExistsRequest { Login = login };
			if ((await usersClient.IsUserExistsAsync(isUserExistsRequest)).Exists)
				return Conflict($"Логин {login} уже есть.");

			passwordService.GenerateCredentials(password, out var hash, out var salt);

			var request = new AddUserRequest { Login = login, PasswordHash = hash, PasswordSalt = salt, Codes = { codes } };
			return (await usersClient.AddUserAsync(request)).Success
				? Ok()
				: throw new Exception("Произошла ошибка при добавлении пользователя.");
		}

		[HttpGet("login")]
		public async Task<IActionResult> Login(string login, string password)
		{
			var getUserCredentialsRequest = new GetUserCredentialsRequest { Login = login };
			var getUserCredentialsResponse = await usersClient.GetUserCredentialsAsync(getUserCredentialsRequest);

			var isAuthenticated = getUserCredentialsResponse.PasswordSalt is not null
				&& getUserCredentialsResponse.PasswordHash is not null
				&& passwordService.ValidateCredentials(password, getUserCredentialsResponse.PasswordHash, getUserCredentialsResponse.PasswordSalt);

			if (!isAuthenticated)
				return Unauthorized();

			var token = jwtService.GenerateToken(login);
			HttpContext.Response.Cookies.Append(jwtService.GetCookieName(), token,
				new CookieOptions { Expires = DateTime.UtcNow.Add(jwtService.GetLifetime()), Secure = true });
			return Ok();
		}

		[HttpGet("logout")]
		public IActionResult Logout()
		{
			HttpContext.Response.Cookies.Delete(jwtService.GetCookieName());
			return Ok();
		}
	}
}
