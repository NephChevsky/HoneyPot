using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace HoneyPot.SSH
{
	internal class Server : IHostedService
	{
		private TcpListener _tcpListener;
		private Task _runner;

		private List<Client> _clients = [];

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 51551);
			_tcpListener.Start();

			_runner = Task.Run(async () =>
			{
				await Poll(cancellationToken);
			}, cancellationToken);

			return Task.CompletedTask;
		}

		public async Task Poll(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				while (_tcpListener.Pending())
				{
					Socket socket = await _tcpListener.AcceptSocketAsync();
					_clients.Add(new Client(socket));
					Log.Information("Connection from: {ClientRemoteEndPoint}", socket.RemoteEndPoint);
				}

				_clients.ForEach(c => c.Poll());

				_clients.RemoveAll(c => c.IsConnected == false);
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
