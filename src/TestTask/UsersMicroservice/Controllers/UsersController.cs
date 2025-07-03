using JwtLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Users;

namespace UsersMicroservice.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController(UsersService.UsersServiceClient usersClient, IJwtService jwtService) : ControllerBase
	{
		[HttpPost("register")]
		public async Task<IActionResult> Register(string login, string password, List<string> codes)
		{
			var isUserExistsRequest = new IsUserExistsRequest { Login = login };
			if ((await usersClient.IsUserExistsAsync(isUserExistsRequest)).Exists)
				return Conflict($"Логин {login} уже есть.");

			var request = new AddUserRequest { Login = login, Password = password };
			request.Codes.AddRange(codes);
			return (await usersClient.AddUserAsync(request)).Success
				? Ok()
				: throw new Exception("Произошла ошибка при добавлении пользователя.");
		}

		[HttpGet("login")]
		public async Task<IActionResult> Login(string login, string password)
		{
			var validateUserRequest = new ValidateUserRequest { Login = login, Password = password };
			var validateUserResponse = await usersClient.ValidateUserAsync(validateUserRequest);
			if (!validateUserResponse.Correct)
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
