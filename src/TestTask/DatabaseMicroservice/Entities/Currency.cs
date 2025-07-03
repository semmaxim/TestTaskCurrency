using System.ComponentModel.DataAnnotations;

namespace DatabaseMicroservice.Entities;

public class Currency
{
	public int Id { get; set; }

	[MaxLength(3)]
	public string Code { get; set; } = String.Empty;

	[MaxLength(100)]
	public string Name { get; set; } = String.Empty;

	public decimal Rate { get; set; }
}