using System.ComponentModel.DataAnnotations;

namespace HoneyPot.DB.Models
{
	public class User
	{
		[Key]
		public Guid Id { get; set; }

		public string RemoteIdentifier { get; set; }

		public string Login { get; set; }

		public string Password { get; set; }

		public DateTime CreationDateTime { get; set; }

		public DateTime LastLoginDateTime { get; set; }

		public User(string remoteIdentifier, string login, string password)
		{
			RemoteIdentifier = remoteIdentifier;
			Login = login;
			Password = password;
		}
	}
}
