using CurrencyUpdateMicroservice.Services;

namespace CurrencyUpdateMicroservice;

public class CurrencyUpdaterWorker(IConfiguration configuration, ILogger<CurrencyUpdaterWorker> logger, ICurrencyUpdateService currencyUpdateService) : BackgroundService
{
	private const string UpdateDelaySettingsPath = "CurrencyUpdaterWorker:UpdateDelay";

	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Сервис запущен");

		if (!TimeSpan.TryParse(configuration.GetValue<string>(UpdateDelaySettingsPath), out var updateDelay))
		{
			logger.LogError($"Не удалось получить параметр {UpdateDelaySettingsPath}");
		}
		else
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(updateDelay, cancellationToken);

				try
				{
					await currencyUpdateService.UpdateCurrency(cancellationToken);
					logger.LogInformation("Валюты обновлены");
				}
				catch (Exception e)
				{
					logger.LogError(e, "Произошла ошибка при запуске обновления валют.");
				}
			}
		}
	}
}