using System.ComponentModel.DataAnnotations;

namespace DatabaseMicroservice.Entities;

public class User
{
	public int Id { get; set; }

	[MaxLength(100)]
	public string Name { get; set; } = String.Empty;

	[MaxLength(100)]
	public string PasswordHash { get; set; } = String.Empty;

	[MaxLength(200)]
	public string PasswordSalt { get; set; } = String.Empty;

	public List<Currency> Currencies { get; set; } = [];
}