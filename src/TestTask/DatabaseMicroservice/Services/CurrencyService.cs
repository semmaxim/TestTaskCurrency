using System.Globalization;
using Currency;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMicroservice.Services;

public class CurrencyService(TestTaskDbContext dbContext) : Currency.CurrencyService.CurrencyServiceBase
{
	public override async Task<SetCurrencyRatesResponse> SetCurrencyRates(SetCurrencyRatesRequest request, ServerCallContext context)
	{
		var newCurrencies = request.Rates.ToDictionary(rate => rate.Code);
		var oldCurrencies = dbContext.Currency.ToDictionary(currency => currency.Code);

		var currencyForAddCodes = newCurrencies.Keys.Where(code => !oldCurrencies.ContainsKey(code));
		var currencyForDeleteCodes = oldCurrencies.Keys.Where(code => !newCurrencies.ContainsKey(code));
		var currencyForModifyCodes = newCurrencies.Keys.Where(code => oldCurrencies.ContainsKey(code));

		foreach (var code in currencyForAddCodes)
		{
			var newCurrency = newCurrencies[code];
			await dbContext.Currency.AddAsync(new Entities.Currency
			{
				Code = newCurrency.Code,
				Name = newCurrency.Name,
				Rate = Decimal.Parse(newCurrency.Rate)
			});
		}

		foreach (var code in currencyForDeleteCodes)
		{
			var oldCurrency = oldCurrencies[code];
			oldCurrency.Rate = null;
		}

		foreach (var code in currencyForModifyCodes)
		{
			var oldCurrency = oldCurrencies[code];
			var newCurrency = newCurrencies[code];
			var newRate = Decimal.Parse(newCurrency.Rate);
			if (oldCurrency.Name != newCurrency.Name)
				oldCurrency.Name = newCurrency.Name;

			if (oldCurrency.Rate != newRate)
				oldCurrency.Rate = newRate;
		}

		await dbContext.SaveChangesAsync();

		return new SetCurrencyRatesResponse { Success = true };
	}

	public override async Task<GetCurrencyRatesForUserResponse> GetCurrencyRatesForUser(GetCurrencyRatesForUserRequest request, ServerCallContext context)
	{
		var currencies = await dbContext.Users
			.Where(user => user.Name == request.Login)
			.SelectMany(user => user.Currencies)
			.Select(currency => new CurrencyRate
			{
				Name = currency.Name,
				Code = currency.Code,
				Rate = currency.Rate == null ? String.Empty : currency.Rate.Value.ToString(NumberFormatInfo.InvariantInfo)
			})
			.OrderBy(rate => rate.Code)
			.ToListAsync();
		var result = new GetCurrencyRatesForUserResponse { Rates = { currencies } };
		return result;
	}
}