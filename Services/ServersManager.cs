namespace UbikMmo.Authenticator.Services;

public class ServersManager {

	private readonly Dictionary<string, Server> servers = new();

	public static ServersManager Instance => _instance;
	private readonly static ServersManager _instance = new();

	public string? RegisterServer(Server.ServerDTO serverDto) {
		if(servers.ContainsKey(serverDto.ServerName ?? "NULL")) {
			Console.WriteLine("Could not register server " + serverDto.ServerName + ": already exists.");
			return null;
		}
		Server server = new(serverDto);
		servers[server.Name] = server;
		Console.WriteLine("New server : "+ server);

		return server.Token;
	}

	public Server? GetServer(string? serverName) {
		if(serverName == null || !servers.ContainsKey(serverName))
			return null;
		return servers[serverName];
	}

	public IEnumerable<string> GetServersNames() {
		return servers.Values.Select(s => s.Name);
	}

}
