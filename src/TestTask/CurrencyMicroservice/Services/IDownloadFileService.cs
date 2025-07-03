namespace CurrencyUpdateMicroservice.Services;

public interface IDownloadFileService
{
	Task<string> GetTextFile(string url, CancellationToken cancellationToken);
}