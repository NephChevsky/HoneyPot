using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

			NetworkStream stream = new NetworkStream(_socket, false);

			// Step 1: Send SSH Banner (Pretending to be a real SSH server)
			string banner = "SSH-2.0-Open SSH for _Windows_ 9.5\r\n";
			byte[] bannerBytes = Encoding.ASCII.GetBytes(banner);
			stream.Write(bannerBytes, 0, bannerBytes.Length);
			stream.Flush();

			// Step 2: Read Client's SSH Version
			byte[] buffer = new byte[1024];
			int bytesRead = stream.Read(buffer, 0, buffer.Length);

			if (bytesRead > 0)
			{
				string clientVersion = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
				Log.Information($"Client SSH Version: {clientVersion}");
			}

			// Step 3: Capture Key Exchange Initialization (SSH_MSG_KEXINIT)
			while (IsConnected)
			{
				try
				{
					bytesRead = stream.Read(buffer, 0, buffer.Length);
					if (bytesRead == 0) break; // Client disconnected

					

				}
				catch (Exception ex)
				{
					Console.WriteLine($"[ERROR] {ex.Message}");
					break;
				}
			}
		}
	}
}
