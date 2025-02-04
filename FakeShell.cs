using Serilog;
using System.Text;

namespace HoneyPot
{
	public class FakeShell
	{
		public event EventHandler<byte[]> DataReceived;
		public event EventHandler<byte[]> CloseReceived;

		private string _currentCommand = string.Empty;

		public void Start()
		{
			ScrollDown(39);
			MoveCursorPosition(1, 1);
			DataReceived(this, Encoding.UTF8.GetBytes("Microsoft Windows [Version 10.0.19045.5371]\r\n"));
			DataReceived(this, Encoding.UTF8.GetBytes("(c) Microsoft Corporation. All rights reserved.\r\n"));
			DataReceived(this, Encoding.UTF8.GetBytes("\r\n"));
			DataReceived(this, Encoding.UTF8.GetBytes("neph@NEPH-PC C:\\Users\\Neph>"));
		}

		public void OnData(byte[] data)
		{
			foreach (byte b in data)
			{
				if (b == 0x7f)
				{
					if (_currentCommand.Length > 0)
					{
						_currentCommand = _currentCommand[..^1];
						SendBackSpace();
					}
				}
				else if (b == 0x0d)
				{
					DataReceived(this, [b, 0x0a]);
					bool exit = RunCommand(_currentCommand);
					_currentCommand = string.Empty;
					if (!exit)
					{
						DataReceived(this, Encoding.UTF8.GetBytes("neph@NEPH-PC C:\\Users\\Neph>"));
					}
				}
				else
				{
					_currentCommand += Encoding.UTF8.GetString([b]);
					DataReceived(this, [b]);
				}
			}
		}

		private void ScrollDown(int n)
		{
			DataReceived(this, Encoding.UTF8.GetBytes($"\x1b[{n}S"));
		}

		private void MoveCursorPosition(int x, int y)
		{
			DataReceived(this, Encoding.UTF8.GetBytes($"\x1b[{x};{y}H"));
		}

		private void SendBackSpace()
		{
			DataReceived(this, [0x08, 0x20, 0x08]);
		}

		private bool RunCommand(string cmd)
		{
			cmd = cmd.Trim();
			if (cmd == "dir")
			{
				DataReceived(this, Encoding.UTF8.GetBytes("show file list\r\n"));
			}
			else if (cmd == "exit")
			{
				CloseReceived(this, [0x03]);
				return true;
			}
			else
			{
				DataReceived(this, Encoding.UTF8.GetBytes($"'{cmd.Split(' ')[0]}' is not recognized as an internal or external command,\r\noperable program or batch file.\r\n"));
			}

			return false;
		}

		public void OnClose()
		{
			DataReceived(this, [0x03]);
			Log.Information("Shell closed");
		}
	}
}
