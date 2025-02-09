using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyPot.DB
{
	public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HoneyPotDbContext>
	{
		public HoneyPotDbContext CreateDbContext(string[] args)
		{
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.AddUserSecrets<HoneyPotDbContext>()
				.Build();

			var builder = new DbContextOptionsBuilder<HoneyPotDbContext>();
			builder.UseSqlServer(configuration["SQLConnectionString"]);
			return new HoneyPotDbContext(builder.Options);
		}
	}
}
