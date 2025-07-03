using System.Xml.Linq;
using Currency;

namespace CurrencyUpdateMicroservice.Services;

public class CurrencyUpdateService(
	CurrencyService.CurrencyServiceClient currencyClient,
	IConfiguration configuration,
	IDownloadFileService downloadFileService)
	: ICurrencyUpdateService
{
	private readonly string UpdateUri = configuration.GetValue<string>("CurrencyUpdateService:UpdateUrl")!;

	public async Task UpdateCurrency(CancellationToken cancellationToken)
	{
		var file = await downloadFileService.GetTextFile(UpdateUri, cancellationToken);
		var xml = XElement.Parse(file);

		var request = new SetCurrencyRatesRequest();
		request.Rates.AddRange(xml.Elements("Valute")
			.Select(e => new CurrencyRate
			{
				Code = e.Element("CharCode")!.Value,
				Name = e.Element("Name")!.Value,
				Rate = e.Element("Value")!.Value
			}));
		var response = await currencyClient.SetCurrencyRatesAsync(request, cancellationToken: cancellationToken);
		if (!response.Success)
			throw new Exception("Не удалось передать список валют на микросервис БД.");
	}
}