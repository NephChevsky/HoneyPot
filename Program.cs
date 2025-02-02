using HoneyPot.SSH;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;

namespace HoneyPot
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Log.Logger = CreateLogger();

			HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
			builder.Services.AddHostedService<Server>();

			var host = builder.Build();
			host.Run();
		}

		private static Logger CreateLogger()
		{
			LoggerConfiguration config = new LoggerConfiguration()
				.MinimumLevel.Information()
				.WriteTo.Console()
				.WriteTo.File("current.log");

			return config.CreateLogger();
		}
	}
}
