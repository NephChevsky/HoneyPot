namespace HoneyPot
{
	public class VirtualDisk
	{
		private readonly List<string> _folders = [
			@"C:\",
			@"C:\Users",
			@"C:\Users\Neph"
		];
		private readonly List<string> _files = [];

		public void CreateFolder(string dir, string name)
		{
			_folders.Add(Path.Combine(dir, name));
		}

		public List<string> GetFolders(string dir)
		{
			dir += @"\";
			return _folders.Where(x => x.StartsWith(dir, StringComparison.OrdinalIgnoreCase)).Select(x => x.Replace(dir, "", StringComparison.OrdinalIgnoreCase)).ToList();
		}

		public List<string> GetFiles(string dir)
		{
			dir += @"\";
			return _files.Where(x => x.StartsWith(dir, StringComparison.OrdinalIgnoreCase)).Select(x => x.Replace(dir, "", StringComparison.OrdinalIgnoreCase)).ToList();
		}

		public bool FolderExists(string dir, string name)
		{
			return _folders.Any(x => x.Equals(Path.GetFullPath(Path.Combine(dir, name)), StringComparison.OrdinalIgnoreCase));
		}
	}
}
