using Serilog;
using System.Net.Sockets;
using System.Text;

namespace HoneyPot.SSH
{
	internal class Client
	{
		private Socket _socket;
		private NetworkStream _networkStream;
		private StreamWriter _streamWriter;
		private StreamReader _streamReader;

		private bool _protocolVersionExchangeCompleted;

		public bool IsConnected
		{
			get
			{
				return _socket != null;
			}
		}

		public Client(Socket socket)
		{
			_socket = socket;

			_networkStream = new(_socket);
			_streamWriter = new(_networkStream, Encoding.ASCII);
			_streamReader = new(_networkStream, Encoding.ASCII);
		}

		public void Poll()
		{
			if (!IsConnected)
			{
				return;
			}

			if (!_protocolVersionExchangeCompleted)
			{
				// Step 1: Send SSH protocol version string to the client
				string versionString = "SSH-2.0-Open SSH for _Windows_ 9.5\r\n"; // Server's version string
				_streamWriter.Write(versionString);
				_streamWriter.Flush();

				// Step 2: Wait for the client's response to the protocol version
				string clientVersion = _streamReader.ReadLine();
				_protocolVersionExchangeCompleted = true;
				Log.Information("Received client version: {ClientVersion}", clientVersion);
				return;
			}
		}
	}
}
