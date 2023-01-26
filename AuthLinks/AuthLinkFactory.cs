namespace UbikMmo.Authenticator.AuthLinks;

public static class AuthLinkFactory {

	private static IAuthLink? _iAuth;
	public static IAuthLink IAuth {
		get {
			_iAuth ??= Factory();
			return _iAuth;
		}
	}

	private static IAuthLink Factory() {
		string? authName = Environment.GetEnvironmentVariable("STORE");
		if(authName == null) {
			return MessageThenDefault("No \"STORE\" variable set.");
		}

		Console.WriteLine("Building AuthLink '" + authName + "'.");
		return authName switch {
			"redis" => new RedisAuthLink(),
			"sqlite" => new SQLiteAuthLink(),
			"fake" => new FakeAuthLink(),
			_ => MessageThenDefault("Unknown STORE value: \"" + authName+"\".")
		};
	}

	private static IAuthLink MessageThenDefault(string message) {
		Console.WriteLine("WARNING: " + message + " Using fake storage.");
		return new FakeAuthLink();
	}

}
