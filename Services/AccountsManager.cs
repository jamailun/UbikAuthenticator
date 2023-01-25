namespace UbikMmo.Authenticator.Services; 

public class AccountsManager {

	private readonly Dictionary<string, Account> players = new();

	public static AccountsManager Instance => _instance;
	private readonly static AccountsManager _instance = new();

	public string GetOrCreateTokenForPlayer(string uuid) {
		// If already exist, just return token
		foreach(var a in players.Values) {
			if(a.Uuid.Equals(uuid)) {
				return a.Token;
			}
		}
		// Create token and account
		string token = Utils.RandomString(64);
		players[token] = new(uuid, token);

		return token;
	}

	public string? GetUuidFromToken(string? token) {
		if(token == null || ! players.ContainsKey(token))
			return null;
		return players[token].Uuid;
	}

	public IEnumerable<Account> Debug_GetLoggedPlayers() {
		return players.Values;
	}

}
