namespace UbikMmo.Authenticator.Services; 

public class PlayersManager {

	private readonly Dictionary<string, PlayerAccount> players = new();

	public static PlayersManager Instance => _instance;
	private readonly static PlayersManager _instance = new();

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

	public IEnumerable<PlayerAccount> Debug_GetLoggedPlayers() {
		return players.Values;
	}

}
