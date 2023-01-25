using StackExchange.Redis;
using System.Text.Json;
using UbikMmo.Authenticator.Structures;

namespace UbikMmo.Authenticator.AuthLinks;

//TODO redo everything here

public class RedisAuthLink : IAuthLink {

	private readonly ConnectionMultiplexer _redis;

	public RedisAuthLink() {
		string? redisEndpoints = Environment.GetEnvironmentVariable("IAUTH.redis.endpoints");
		string? redisUser = Environment.GetEnvironmentVariable("IAUTH.redis.user");
		string? redisPassword = Environment.GetEnvironmentVariable("IAUTH.redis.password");
		if(redisEndpoints == null || redisUser == null || redisPassword == null) {
			throw new ArgumentException("Could not get environment variable 'IAUTH.redis.{endpoint/user/password}'.");
		}

		// Configuration
		ConfigurationOptions config = new() {
			Password = redisPassword,
			User = redisUser
		};
		foreach(var ep in redisEndpoints.Split(";"))
			config.EndPoints.Add(ep);

		// Connect
		_redis = ConnectionMultiplexer.Connect(config);

		// Debug
		Console.WriteLine("Redis connected: " + _redis.GetDatabase().Ping());
	}

	public async Task<Result<string>> LogAccount(LoginRequest request) {
		string password = Utils.HashString(request.Password);

		var db = _redis.GetDatabase();
		string key = "account:" + (request.Username ?? "") + ":" + password;

		// Test existence
		bool exists = await db.KeyExistsAsync(key);
		if(!exists) {
			return Result<string>.Error("Username or password incorrect.");
		}

		// Read entries
		var entries = await db.HashGetAllAsync(key);
		RedisMap map = new(entries);

		// Return UUID
		return Result<string>.Success(map["uuid"]);
	}

	public async Task<Result<string>> RegisterAccount(RegisterRequest request) {
		string password = Utils.HashString(request.Password);
		string key = "account:" + request.Username + ":" + password;

		RedisMap map = new();
		map["uuid"] = Utils.RandomString(32);
		map["username"] = request.Username;

		await _redis.GetDatabase().HashSetAsync(key, map.ToArray());

		return Result<string>.Success(map["uuid"]);
	}

}

internal class RedisMap {

	private readonly Dictionary<RedisValue, RedisValue> map = new();
	
	public RedisMap() {}
	
	public RedisMap(HashEntry[] entries) {
		foreach(var entry in entries)
			map[entry.Name] = entry.Value;
	}

	public string this[string key] {
		get {
			return map[key].ToString() ?? "";
		}
		set {
			map[key] = value;
		}
	}

	public HashEntry[] ToArray() {
		var array = new HashEntry[map.Count];
		int i = 0;
		foreach(var entry in map) {
			array[i++] = new HashEntry(entry.Key, entry.Value);
		}
		return array;
	}
}
