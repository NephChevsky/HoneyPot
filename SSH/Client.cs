using Serilog;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;

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

			NetworkStream stream = new(_socket);
			StreamReader reader = new(stream, Encoding.ASCII);
			StreamWriter writer = new(stream, Encoding.ASCII);

			// Step 1: Send SSH protocol version string to the client
			if (reader.Peek() == 0) // Check if we haven't already sent a version string
			{
				string versionString = "SSH-2.0-Open SSH for _Windows_ 9.5\r\n"; // Server's version string
				writer.Write(versionString);
				writer.Flush();
			}

			// Step 2: Wait for the client's response to the protocol version
			if (reader.Peek() > 0)
			{
				string clientVersion = reader.ReadLine();
				Log.Information("Received client version: {ClientVersion}", clientVersion);
			}
		}
	}
}
