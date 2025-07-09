using JwtLibrary.Services;
using Microsoft.AspNetCore.Authentication;

namespace FinanceMicroservice
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			var commonSettingsFolder = Path.Combine(builder.Environment.ContentRootPath, "..\\CommonSettings");
			builder.Configuration.AddJsonFile(Path.Combine(commonSettingsFolder, "addresses.json"), false);
			builder.Configuration.AddJsonFile(Path.Combine(commonSettingsFolder, "jwtsettings.json"), false);

			var databaseMicroserviceAddress = builder.Configuration.GetValue<string>("DatabaseMicroserviceAddress")!;

			// Add services to the container.
			builder.Services.AddGrpc();
			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			builder.Services.AddGrpcClient<Currency.CurrencyService.CurrencyServiceClient>(options => options.Address = new Uri(databaseMicroserviceAddress));

			builder.Services.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = JwtFromCookieAuthenticationHandler.AuthenticationSchemeName;
					options.DefaultChallengeScheme = JwtFromCookieAuthenticationHandler.AuthenticationSchemeName;
				})
				.AddScheme<AuthenticationSchemeOptions, JwtFromCookieAuthenticationHandler>(JwtFromCookieAuthenticationHandler.AuthenticationSchemeName, null);

			builder.Services.AddSingleton<IJwtService, JwtService>();

			var app = builder.Build();

			app.MapControllers();

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseAuthentication();
			app.UseAuthorization();

			app.Run();
		}
	}
}