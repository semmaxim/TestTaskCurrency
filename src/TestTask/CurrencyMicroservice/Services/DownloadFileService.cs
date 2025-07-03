namespace CurrencyUpdateMicroservice.Services;

public class DownloadFileService : IDownloadFileService
{
	public async Task<string> GetTextFile(string url, CancellationToken cancellationToken)
	{
		using var client = new HttpClient();
		return await client.GetStringAsync(url, cancellationToken);
	}
}