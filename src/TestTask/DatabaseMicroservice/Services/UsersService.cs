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
			Exists = await dbContext.Users.FirstOrDefaultAsync(user => user.Name == request.Login, context.CancellationToken) != null
		};
	}

	public override async Task<AddUserResponse> AddUser(AddUserRequest request, ServerCallContext context)
	{
		var currencies = await dbContext.Currency.Where(currency => request.Codes.Contains(currency.Code)).ToListAsync(context.CancellationToken);
		var newUser = new User
		{
			Name = request.Login,
			PasswordHash = request.PasswordHash,
			PasswordSalt = request.PasswordSalt,
			Currencies = currencies
		};

		dbContext.Users.Add(newUser);

		await dbContext.SaveChangesAsync(context.CancellationToken);
		return new AddUserResponse { Success = true };
	}

	public override async Task<GetUserCredentialsResponse> GetUserCredentials(GetUserCredentialsRequest request, ServerCallContext context)
	{
		var result = await dbContext.Users.FirstOrDefaultAsync(user => user.Name == request.Login, context.CancellationToken);
		return new GetUserCredentialsResponse
		{
			PasswordHash = result?.PasswordHash,
			PasswordSalt = result?.PasswordSalt
		};
	}
}