using DatabaseMicroservice.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseMicroservice;

public class TestTaskDbContext : DbContext
{
	public TestTaskDbContext()
	{
	}

	public TestTaskDbContext(DbContextOptions<TestTaskDbContext> options) : base(options)
	{
	}

	public DbSet<DatabaseMicroservice.Entities.Currency> Currency { get; set; }
	public DbSet<User> Users { get; set; }

}