using Serilog;
using System.Text;

namespace HoneyPot
{
	public class VirtualShell
	{
		public event EventHandler<byte[]> DataReceived;
		public event EventHandler<byte[]> CloseReceived;

		private string _currentCommand = string.Empty;
		private string _workingDir = @"C:\Users\Neph";

		private VirtualDisk _disk = new();

		public void Start()
		{
			ScrollDown(39);
			MoveCursorPosition(1, 1);
			DataReceived(this, Encoding.UTF8.GetBytes("Microsoft Windows [Version 10.0.19045.5371]\r\n"));
			DataReceived(this, Encoding.UTF8.GetBytes("(c) Microsoft Corporation. All rights reserved.\r\n"));
			DataReceived(this, Encoding.UTF8.GetBytes("\r\n"));
			DataReceived(this, Encoding.UTF8.GetBytes($"neph@NEPH-PC {_workingDir}>"));
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
						DataReceived(this, Encoding.UTF8.GetBytes($"neph@NEPH-PC {_workingDir}>"));
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
			Log.Information(cmd);
			cmd = cmd.Trim();
			List<string> splittedCmd = [.. cmd.Split(' ')];
			string verb = splittedCmd[0];
			List<string> parameters = splittedCmd.Skip(1).ToList();
			if (verb == "dir")
			{
				List<string> folders = _disk.GetFolders(_workingDir);
				List<string> files = _disk.GetFiles(_workingDir);
				List<string> listing = [];
				listing.AddRange(folders);
				listing.AddRange(files);
				listing = [.. listing.OrderBy(x => x)];
				DataReceived(this, Encoding.UTF8.GetBytes($" Volume in drive {_workingDir[0]} has no label.\r\n"));
				DataReceived(this, Encoding.UTF8.GetBytes($" Volume Serial Number is EA45-3E10\r\n\r\n"));
				DataReceived(this, Encoding.UTF8.GetBytes($" Directory of {_workingDir}\r\n\r\n"));
				DataReceived(this, Encoding.UTF8.GetBytes($"04/02/2025  22:41    <DIR>          .\r\n"));
				DataReceived(this, Encoding.UTF8.GetBytes($"04/02/2025  22:41    <DIR>          ..\r\n"));
				foreach (string folder in folders)
				{
					DataReceived(this, Encoding.UTF8.GetBytes($"04/02/2025  22:41    <DIR>          {folder}\r\n"));
				}
				DataReceived(this, Encoding.UTF8.GetBytes($"               {files.Count} File(s)              0 bytes\r\n"));
				DataReceived(this, Encoding.UTF8.GetBytes($"               {folders.Count} Dir(s)  479,379,808,256 bytes free\r\n\r\n"));
			}
			else if (verb == "cd")
			{
				if (_disk.FolderExists(_workingDir, parameters[0]))
				{
					if (parameters[0] == "..")
					{
						_workingDir = string.Join('\\', _workingDir.Split('\\').SkipLast(1));
					}
					else
					{
						_workingDir = Path.Combine(_workingDir, parameters[0]);
					}
					DataReceived(this, Encoding.UTF8.GetBytes("\r\n"));
				}
			}
			else if (verb == "mkdir")
			{
				if (parameters.Count == 0)
				{
					DataReceived(this, Encoding.UTF8.GetBytes("The syntax of the command is incorrect.\r\n"));
				}
				else
				{
					foreach (string parameter in parameters)
					{
						_disk.CreateFolder(_workingDir, parameter);
					}
					DataReceived(this, Encoding.UTF8.GetBytes("\r\n"));
				}
			}
			else if (verb == "exit")
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
