using System.Security.Cryptography;
using System.Text.Json.Nodes;

namespace UbikMmo.Authenticator;

public static class Utils {

	#region Salt and hash
	private static string? _salt;
	public static string Salt {
		get {
			_salt ??= UbikEnvironment.GetString("SALT") ?? "";
			return _salt;
		}
	}
	public static string HashString(string? text, bool alphaNumOnly = true) {
		if(String.IsNullOrEmpty(text))
			return String.Empty;

		// Uses SHA256 to create the hash
		using var sha = SHA256.Create();

		// Convert the string to a byte array first, to be processed
		byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text + Salt);
		byte[] hashBytes = sha.ComputeHash(textBytes);

		// Convert back to a string, removing the '-' that BitConverter adds
		string hash = BitConverter.ToString(hashBytes);
		return alphaNumOnly ? hash.Replace("-", String.Empty) : hash;
	}
	#endregion

	#region Random
	private readonly static Random random = new();
	public static string RandomString(int length) {
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}
	#endregion

	// get the root of structures.json
	public static JsonObject LoadStructuresFile() {
		string json = File.ReadAllText("Properties/structures.json");

		JsonNode? node = JsonObject.Parse(json);
		if(node == null)
			throw new Exception("structures file do not contain anything.");
		if(node.GetType() != typeof(JsonObject))
			throw new Exception("structures file must contain an object, not an array.");

		return (JsonObject) node;
	}

}
