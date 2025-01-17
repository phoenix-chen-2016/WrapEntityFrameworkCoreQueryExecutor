namespace WrapEntityFrameworkCoreQueryExecutor;

using Microsoft.EntityFrameworkCore;

internal class SandboxDbContext(DbContextOptions<SandboxDbContext> options)
	: DbContext(options)
{
	public DbSet<Table1> Table1 { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Table1>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Name).IsRequired();
		});
	}
}