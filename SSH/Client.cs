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
		}
	}
}
