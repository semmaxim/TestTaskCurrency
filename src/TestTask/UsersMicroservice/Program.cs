using JwtLibrary.Services;

namespace UsersMicroservice;

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
		builder.Services.AddGrpcClient<Users.UsersService.UsersServiceClient>(options => options.Address = new Uri(databaseMicroserviceAddress));

		builder.Services.AddSingleton<IJwtService, JwtService>();

		var app = builder.Build();

		app.MapControllers();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.Run();
	}
}
