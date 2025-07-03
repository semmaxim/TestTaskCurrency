using DatabaseMicroservice.Services;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMicroservice
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddGrpc();
			builder.Services.AddDbContext<TestTaskDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

			var app = builder.Build();

			MigrateDatabase(app);

			// Configure the HTTP request pipeline.
			app.MapGrpcService<CurrencyService>();
			app.MapGrpcService<UsersService>();
			app.Run();
		}

		private static void MigrateDatabase(WebApplication webApplication)
		{
			using var scope = webApplication.Services.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<TestTaskDbContext>();
			context.Database.Migrate();
		}
	}
}