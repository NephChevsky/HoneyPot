using FxSsh;
using FxSsh.Services;
using HoneyPot.DB;
using HoneyPot.DB.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Net;

namespace HoneyPot
{
	internal class SshServerService : IHostedService
	{
		private SshServer _sshServer;

		private HoneyPotDbContext _db;
		private IConfiguration _configuration;

		readonly Dictionary<byte[], VirtualShell> _shells = [];

		public SshServerService(HoneyPotDbContext dbcontext, IConfiguration configuration)
		{
			_db = dbcontext;
			_configuration = configuration;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_sshServer = new(new StartingInfo(IPAddress.Parse("127.0.0.1"), 51551, "SSH-2.0-OpenSSH_for_Windows_9.5"));
			_sshServer.AddHostKey("rsa-sha2-256", _configuration["RSA2048PrivateKey"]);
			_sshServer.AddHostKey("rsa-sha2-512", _configuration["RSA2048PrivateKey"]);
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
			Log.Information("{RemoteEndPoint} connected with credentials: \"{UserName}\" - \"{Password}\"", e.Session.RemoteEndPoint.ToString(), e.Username, e.Password);

			DateTime now = DateTime.UtcNow;
			string remoteIdentifier = e.Session.RemoteEndPoint.ToString().Split(':')[0];
			User user = _db.Users.Where(x => x.RemoteIdentifier == remoteIdentifier && x.Login == e.Username && x.Password == e.Password).FirstOrDefault();
			if (user == null)
			{
				user = new User(remoteIdentifier, e.Username, e.Password)
				{
					CreationDateTime = now
				};
				_db.Users.Add(user);
			}
			user.LastLoginDateTime = now;

			_db.SaveChanges();

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
					User user = _db.Users.Where(x => x.RemoteIdentifier == e.AttachedUserAuthArgs.Session.RemoteEndPoint.ToString().Split(':', StringSplitOptions.None)[0] && x.Login == e.AttachedUserAuthArgs.Username && x.Password == e.AttachedUserAuthArgs.Password).First();
					shell = new(_db, user);
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
