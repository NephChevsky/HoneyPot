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

		readonly Dictionary<byte[], VirtualShell> _shells = [];

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
			e.Disconnected += Event_Disconnected;
		}

		private void Event_ServiceRegistered(object sender, SshService e)
		{
			var session = (Session)sender;
			Log.Information("Session {SessionId} requesting {ServiceName}", Convert.ToHexString(session.SessionId), e.GetType().Name);

			if (e is UserAuthService)
			{
				var service = (UserAuthService)e;
				service.UserAuth += Event_UserAuth;
			}
			else if (e is ConnectionService service)
			{
				service.CommandOpened += Event_CommandOpened;
				/*service.EnvReceived += service_EnvReceived;
				service.PtyReceived += service_PtyReceived;
				service.TcpForwardRequest += service_TcpForwardRequest;
				service.WindowChange += Service_WindowChange;*/
			}
		}

		private void Event_Disconnected(object sender, EventArgs e)
		{
			_shells.Remove(((Session)sender).SessionId);
		}

		private void Event_UserAuth(object sender, UserAuthArgs e)
		{
			Log.Information("Client {KeyAlgorithm} fingerprint: {Fingerprint}.", e.KeyAlgorithm, e.Fingerprint);

			e.Result = true;
		}

		private void Event_CommandOpened(object sender, CommandRequestedArgs e)
		{
			Log.Information("Channel {ServerChannelId} runs {ShellType}: \"{CommandText}\", client key SHA256:{Fingerprint}", e.Channel.ServerChannelId, e.ShellType, e.CommandText, e.AttachedUserAuthArgs.Fingerprint);

			if (e.ShellType == "shell")
			{
				e.Agreed = true;

				if (!_shells.TryGetValue(e.AttachedUserAuthArgs.Session.SessionId, out VirtualShell shell))
				{
					shell = new();
					_shells.Add(e.AttachedUserAuthArgs.Session.SessionId, shell);

					e.Channel.DataReceived += (ss, ee) => shell.OnData(ee);
					e.Channel.CloseReceived += (ss, ee) => shell.OnClose();
					shell.DataReceived += (ss, ee) => e.Channel.SendData(ee);
					shell.CloseReceived += (ss, ee) => e.Channel.SendClose();

					shell.Start();
				}
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
