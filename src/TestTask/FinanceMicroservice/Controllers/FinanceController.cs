using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Currency;

namespace FinanceMicroservice.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FinanceController(CurrencyService.CurrencyServiceClient currencyServiceClient) : ControllerBase
	{
		public record UserCurrencyRate(string Code, string Name, decimal? Rate);

		[Authorize]
		[HttpGet("getrates")]
		public async Task<IActionResult> GetRates()
		{
			var login = User.Identity!.Name;
			var request = new GetCurrencyRatesForUserRequest { Login = login };
			var response = await currencyServiceClient.GetCurrencyRatesForUserAsync(request);
			return Ok(response.Rates.Select(rate => new UserCurrencyRate(rate.Code, rate.Name,
				rate.Rate == String.Empty ? null : Decimal.Parse(rate.Rate, NumberFormatInfo.InvariantInfo))).ToList());
		}
	}
}
