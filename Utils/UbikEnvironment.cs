namespace UbikMmo.Authenticator;

public static class UbikEnvironment {

	private static readonly PropertiesReader props = new(Environment.GetEnvironmentVariable("env") ?? "env.properties");

	/// <summary>
	/// Get an environment variable. Priority order is Environment > env.properties 
	/// </summary>
	/// <param name="key">The key of the environment variable.</param>
	/// <returns>null if neither C# enviornment nor env.properties contains this variable.</returns>
	public static string? GetString(string key) {
		return Environment.GetEnvironmentVariable(key) ?? props.Get(key);
	}

	public static Dictionary<string, string> GetDict(string prefix) {
		Dictionary<string, string> dict = new();
		var keys = props.Keys.FindAll(k => k.StartsWith(prefix));
		foreach(var key in keys) {
			dict[key] = props.Get(key) ?? "";
		}
		return dict;
	}

	#region Propery reader
	private class PropertiesReader {

		private readonly Dictionary<string, string> data = new();
		public PropertiesReader(string file) {
			foreach(string line in File.ReadAllLines(file)) {
				if((!string.IsNullOrEmpty(line)) &&
					(!line.StartsWith(";")) &&
					(!line.StartsWith("#")) &&
					(!line.StartsWith("'")) &&
					(line.Contains('='))) {
					int index = line.IndexOf('=');
					string key = line[..index].Trim();
					string value = line[(index + 1)..].Trim();

					if((value.StartsWith("\"") && value.EndsWith("\"")) ||
						(value.StartsWith("'") && value.EndsWith("'"))) {
						value = value[1..^1];
					}
					try {
						//ignore dublicates
						data.Add(key, value);
					} catch { }
				}
			}
		}
		public string? Get(string key) {
			if(data.ContainsKey(key))
				return data[key];
			return null;
		}

		public List<string> Keys => new(data.Keys);
	}
	#endregion
}
