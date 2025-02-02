using System.Net.Sockets;

namespace HoneyPot.SSH
{
	internal class Client
	{
		private Socket _socket;

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
