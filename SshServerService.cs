using FxSsh;
using FxSsh.Services;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Net;

namespace HoneyPot
{
	internal class SshServerService : IHostedService
	{
		private SshServer _sshServer;

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_sshServer = new(new StartingInfo(IPAddress.Parse("127.0.0.1"), 51551, "SSH-2.0-OpenSSH_for_Windows_9.5"));
			_sshServer.AddHostKey("rsa-sha2-256", KeyGenerator.GenerateRsaKeyPem(2048));
			_sshServer.AddHostKey("rsa-sha2-512", KeyGenerator.GenerateRsaKeyPem(2048));
			_sshServer.ConnectionAccepted += Event_ConnectionAccepted;
			_sshServer.Start();

			Log.Information("SSH Server has started");

			return Task.CompletedTask;
		}

		private void Event_ConnectionAccepted(object sender, Session e)
		{
			Log.Information("Connection accepted");

			e.ServiceRegistered += Event_ServiceRegistered;
		}

		private void Event_ServiceRegistered(object sender, SshService e)
		{
			var session = (Session)sender;
			Console.WriteLine("Session {0} requesting {1}.", BitConverter.ToString(session.SessionId).Replace("-", ""), e.GetType().Name);

			if (e is UserAuthService)
			{
				var service = (UserAuthService)e;
				service.UserAuth += Event_UserAuth;
			}
			else if (e is ConnectionService)
			{
				var service = (ConnectionService)e;
				service.CommandOpened += Event_CommandOpened;
				/*service.EnvReceived += service_EnvReceived;
				service.PtyReceived += service_PtyReceived;
				service.TcpForwardRequest += service_TcpForwardRequest;
				service.WindowChange += Service_WindowChange;*/
			}
		}

		private void Event_UserAuth(object sender, UserAuthArgs e)
		{
			Console.WriteLine("Client {0} fingerprint: {1}.", e.KeyAlgorithm, e.Fingerprint);

			e.Result = true;
		}

		private void Event_CommandOpened(object sender, CommandRequestedArgs e)
		{
			Console.WriteLine($"Channel {e.Channel.ServerChannelId} runs {e.ShellType}: \"{e.CommandText}\", client key SHA256:{e.AttachedUserAuthArgs.Fingerprint}.");

			if (e.ShellType == "shell")
			{
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_sshServer.Stop();

			Log.Information("SSH Server has stopped");

			return Task.CompletedTask;
		}
	}
}
