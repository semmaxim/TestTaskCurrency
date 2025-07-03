using System.Text;
using CurrencyUpdateMicroservice.Services;

namespace CurrencyUpdateMicroservice
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var builder = Host.CreateApplicationBuilder(args);

			var commonSettingsFolder = Path.Combine(builder.Environment.ContentRootPath, "..\\CommonSettings");
			builder.Configuration.AddJsonFile(Path.Combine(commonSettingsFolder, "addresses.json"), false);

			var databaseMicroserviceAddress = builder.Configuration.GetValue<string>("DatabaseMicroserviceAddress")!;

			builder.Services.AddGrpcClient<Currency.CurrencyService.CurrencyServiceClient>(options => options.Address = new Uri(databaseMicroserviceAddress));
			builder.Services.AddTransient<IDownloadFileService, DownloadFileService>();
			builder.Services.AddTransient<ICurrencyUpdateService, CurrencyUpdateService>();
			builder.Services.AddHostedService<CurrencyUpdaterWorker>();

			var host = builder.Build();

			host.Run();
		}
	}
}