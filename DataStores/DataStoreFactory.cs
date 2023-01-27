namespace UbikMmo.Authenticator.AuthLinks;

public static class DataStoreFactory {

	private static IDataStore? _iAuth;
	public static IDataStore IAuth {
		get {
			_iAuth ??= Factory();
			return _iAuth;
		}
	}

	private static IDataStore Factory() {
		string? authName = Environment.GetEnvironmentVariable("STORE");
		if(authName == null) {
			return MessageThenDefault("No \"STORE\" variable set.");
		}

		Console.WriteLine("Building AuthLink '" + authName + "'.");
		return authName switch {
			"redis" => new RedisDataStore(),
			"sqlite" => new SQLiteDataStore(),
			"fake" => new FakeDataStore(),
			_ => MessageThenDefault("Unknown STORE value: \"" + authName+"\".")
		};
	}

	private static IDataStore MessageThenDefault(string message) {
		Console.WriteLine("WARNING: " + message + " Using fake storage.");
		return new FakeDataStore();
	}

}
