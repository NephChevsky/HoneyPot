using HoneyPot.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoneyPot.DB
{
	public class HoneyPotDbContext : DbContext
	{
		public DbSet<User> Users { get; set; }
		public DbSet<Command> Commands { get; set; }

		public HoneyPotDbContext(DbContextOptions<HoneyPotDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>(e =>
			{
				e.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.IsRequired();

				e.Property(e => e.RemoteIdentifier)
					.IsRequired();

				e.Property(e => e.Login)
					.IsRequired();
				
				e.Property(e => e.Password)
					.IsRequired();

				e.Property(e => e.CreationDateTime)
					.IsRequired();

				e.Property(e => e.LastLoginDateTime)
					.IsRequired();
			});

			modelBuilder.Entity<Command>(e =>
			{
				e.Property(e => e.Id)
					.ValueGeneratedOnAdd()
					.IsRequired();

				e.Property(e => e.FullCommand)
					.IsRequired();

				e.Property(e => e.Verb);

				e.Property(e => e.Args);

				e.Property(e => e.Owner)
					.IsRequired();

				e.Property(e => e.Succeeded);

				e.Property(e => e.ExecutionDateTime)
					.IsRequired();
			});
		}
	}
}
