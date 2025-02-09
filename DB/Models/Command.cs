using Serilog;
using System.ComponentModel.DataAnnotations;

namespace HoneyPot.DB.Models
{
	public class Command
	{
		[Key]
		public Guid Id { get; set; }

		public string FullCommand { get; set; }

		public string Verb { get; set; }

		public List<string> Args { get; set; }

		public Guid Owner { get; set; }

		public bool Succeeded { get; set; }

		public DateTime ExecutionDateTime { get; set; }

		private static Command Parse(string command)
		{
			Command cmd = new()
			{
				FullCommand = command
			};
			List<string> splittedCmd = [.. command.Trim().Split(' ')];
			cmd.Verb = splittedCmd[0];
			cmd.Args = splittedCmd.Skip(1).ToList();

			return cmd;
		}

		public static bool TryParse(string command, out Command cmd)
		{
			cmd = null;
			try
			{
				cmd = Parse(command);
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Failed to parse {Command}: {ExceptionMessage}\r\n{ExceptionStackTrace}", command, ex.Message, ex.StackTrace);
				return false;
			}
		}
	}
}
