using System.Security.Cryptography;
using System.Text;

namespace UsersMicroservice.Services;

public class PasswordService : IPasswordService
{
	public void GenerateCredentials(string password, out string hash, out string salt)
	{
		using var hashAlgorithm = new HMACSHA512();
		salt = Convert.ToBase64String(hashAlgorithm.Key);
		var passwordBytes = Encoding.UTF8.GetBytes(password);
		var hashBytes = hashAlgorithm.ComputeHash(passwordBytes);
		hash = Convert.ToBase64String(hashBytes);
	}

	public bool ValidateCredentials(string password, string hash, string salt)
	{
		var saltBytes = Convert.FromBase64String(salt);
		using var hashAlgorithm = new HMACSHA512(saltBytes);
		var hashBytes = Convert.FromBase64String(hash);
		var passwordBytes = Encoding.UTF8.GetBytes(password);
		var computesHashBytes = hashAlgorithm.ComputeHash(passwordBytes);
		return hashBytes.SequenceEqual(computesHashBytes);
	}
}