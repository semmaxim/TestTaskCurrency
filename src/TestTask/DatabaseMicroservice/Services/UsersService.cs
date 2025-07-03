using System.Globalization;
using DatabaseMicroservice.Entities;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Users;

namespace DatabaseMicroservice.Services;

public class UsersService(TestTaskDbContext dbContext) : Users.UsersService.UsersServiceBase
{
	public override async Task<IsUserExistsResponse> IsUserExists(IsUserExistsRequest request, ServerCallContext context)
	{
		return new IsUserExistsResponse
		{
			Exists = await dbContext.Users.FirstOrDefaultAsync(user => user.Name == request.Login) != null
		};
	}

	public override async Task<AddUserResponse> AddUser(AddUserRequest request, ServerCallContext context)
	{
		var newUser = new User
		{
			Name = request.Login,
			Password = request.Password,
			ObservableCurrencyCodes = request.Codes.ToList()
		};
		await dbContext.Users.AddAsync(newUser);
		await dbContext.SaveChangesAsync();
		return new AddUserResponse { Success = true };
	}

	public override async Task<ValidateUserResponse> ValidateUser(ValidateUserRequest request, ServerCallContext context)
	{
		return new ValidateUserResponse
		{
			Correct = await dbContext.Users.FirstOrDefaultAsync(user => user.Name == request.Login && user.Password == request.Password) != null
		};
	}

	public override async Task<GetRatesResponse> GetRates(GetRatesRequest request, ServerCallContext context)
	{
		var codes = (await dbContext.Users.FirstOrDefaultAsync(user => user.Name == request.Login))?.ObservableCurrencyCodes ?? [];
		var rates = await dbContext.Currency.Where(currency => codes.Contains(currency.Code)).ToListAsync();
		var result = new GetRatesResponse();
		result.Rates.AddRange(rates.Select(currency => new CurrencyRate
		{
			Code = currency.Code,
			Name = currency.Name,
			Rate = currency.Rate.ToString(NumberFormatInfo.InvariantInfo)
		}));
		return result;
	}
}