namespace UsersMicroservice.Services;

public interface IPasswordService
{
	void GenerateCredentials(string password, out string hash, out string salt);

	bool ValidateCredentials(string password, string hash, string salt);
}