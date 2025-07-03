using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace ApiGateway;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Configuration.AddJsonFile(Path.Combine(builder.Environment.ContentRootPath, "routessettings.json"));

		builder.Services.AddOcelot(builder.Configuration);

		var app = builder.Build();

		await app.UseOcelot();

		await app.RunAsync();
	}
}