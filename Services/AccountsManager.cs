namespace UbikMmo.Authenticator.Services; 

public class AccountsManager {

	// TOKEN -> Account
	private readonly Dictionary<string, Account> accounts = new();

	public static AccountsManager Instance => _instance;
	private readonly static AccountsManager _instance = new();

	public string GetOrCreateTokenForAccount(string uuid) {
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

	public void Remove(string uuid) {
		string? token = null;
		foreach(var account in accounts.Values) {
			if(account.Uuid.Equals(uuid)) {
				token = account.Token;
				break;
			}
		}
		if(token != null)
			accounts.Remove(token);
	}

}
