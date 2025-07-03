using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users;

namespace FinanceMicroservice.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FinanceController(UsersService.UsersServiceClient usersServiceClient) : ControllerBase
	{
		public record UserCurrencyRate(string Code, string Name, decimal Rate);

		[Authorize]
		[HttpGet("getrates")]
		public async Task<IActionResult> GetRates()
		{
			var login = User.Identity!.Name;
			var rates = await usersServiceClient.GetRatesAsync(new GetRatesRequest { Login = login });
			return Ok(rates.Rates.Select(rate => new UserCurrencyRate(rate.Code, rate.Name, Decimal.Parse(rate.Rate, NumberFormatInfo.InvariantInfo))).ToList());
		}
	}
}
