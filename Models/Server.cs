

namespace UbikMmo.Authenticator;

public class Server {

    public string Name { get; }

    public string Url { get; }

    public string? Token { get; }

    public Server(ServerDTO dto) {
        Name = dto.ServerName ?? throw new Exception("ServerName CANNOT be null.");
        Url = dto.ServerUrl ?? throw new Exception("ServerUrl CANNOT be null.");
        Token = Utils.HashString(Name);
	}

	public override string ToString() {
        return "{" + Name + " - " + Url + "}";
	}

    public class ServerDTO {
		public string? ServerName { get; set; }
		public string? ServerUrl { get; set; }

		public override string ToString() {
			return "ServerDTO{'" + ServerName + "' - '" + ServerUrl + "'}";
		}

	}

}
