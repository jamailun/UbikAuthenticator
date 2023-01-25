namespace UbikMmo.Authenticator.Services; 

public class AccountsManager {

	private readonly Dictionary<string, Account> accounts = new();

	public static AccountsManager Instance => _instance;
	private readonly static AccountsManager _instance = new();

	public string GetOrCreateTokenForPlayer(string uuid) {
		// If already exist, just return token
		foreach(var a in accounts.Values) {
			if(a.Uuid.Equals(uuid)) {
				return a.Token;
			}
		}
		// Create token and account
		string token = Utils.RandomString(64);
		accounts[token] = new(uuid, token);

		return token;
	}

	public string? GetUuidFromToken(string? token) {
		if(token == null || ! accounts.ContainsKey(token))
			return null;
		return accounts[token].Uuid;
	}

	public IEnumerable<Account> Debug_GetLoggedAccounts() {
		return accounts.Values;
	}

}
