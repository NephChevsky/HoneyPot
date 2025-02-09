using System.ComponentModel.DataAnnotations;

namespace HoneyPot.DB.Models
{
	public class Command
	{
		[Key]
		public Guid Id { get; set; }

		public string FullCommand { get; set; }

		public string Verb { get; set; }

		public string Args { get; set; }

		public Guid Owner { get; set; }

		public bool Succeeded { get; set; }

		public DateTime ExecutionDateTime { get; set; }
	}
}
