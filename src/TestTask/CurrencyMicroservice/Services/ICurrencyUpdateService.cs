namespace CurrencyUpdateMicroservice.Services;

public interface ICurrencyUpdateService
{
	Task UpdateCurrency(CancellationToken cancellationToken);
}