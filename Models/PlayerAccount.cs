using System;

namespace UbikMmo.Authenticator;

public class PlayerAccount {

    public string Uuid { get; }

    public string Token { get; }

    public PlayerAccount(string uuid, string token) {
		Uuid = uuid;
		Token = token;
	}

	public override string ToString() {
        return "PlayerAccount{" + Uuid + "}";
	}

	public class PlayerLoggedIn {
		public string Token { get; }
		public string ServerAddress { get; }

		public PlayerLoggedIn(string token, string address) {
			Token = token;
			ServerAddress = address;
		}
	}

}
