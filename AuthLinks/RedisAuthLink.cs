using StackExchange.Redis;
using System.Collections.Generic;
using UbikMmo.Authenticator.Structures;

namespace UbikMmo.Authenticator.AuthLinks;

public class RedisAuthLink : IAuthLink {

	private const string PREFIX = "accounts:";
	private const string PREFIX_UNIQUE = PREFIX+"_unique_:";

	private readonly ConnectionMultiplexer _redis;

	public RedisAuthLink() {
		string? redisEndpoints = Environment.GetEnvironmentVariable("STORE.redis.endpoints");
		string? redisUser = Environment.GetEnvironmentVariable("STORE.redis.user");
		string? redisPassword = Environment.GetEnvironmentVariable("STORE.redis.password");
		if(redisEndpoints == null || redisUser == null || redisPassword == null) {
			throw new ArgumentException("Could not get environment variable 'STORE.redis.{endpoints/user/password}'.");
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
		Console.WriteLine("Redis connected. Ping = " + _redis.GetDatabase().Ping().Milliseconds + "ms");
	}

	public async Task<Result<string>> LogAccount(LoginRequest request) {
		var db = _redis.GetDatabase();
		string keyData = GetDataKey(request);

		// Test existence
		bool exists = await db.KeyExistsAsync(keyData);
		if(!exists)
			return Result<string>.NotFoundError("Username or password incorrect.");

		// Read entries
		var entries = await db.HashGetAllAsync(keyData);
		RedisMap map = new(entries);

		// Return UUID
		return Result<string>.Success(map[IAuthLink.UUID]);
	}

	public async Task<Result<string>> RegisterAccount(RegisterRequest request) {
		// Duplicate test
		if(UniqueExists(AccountDataStructure.Structure.UsernameField, request.Username))
			return Result<string>.DuplicateError(AccountDataStructure.Structure.UsernameField);
		foreach(var field in AccountDataStructure.Structure.UniqueFields) {
			if(UniqueExists(field.Name, request.Fields[field]))
				return Result<string>.DuplicateError(field.Name);
		}
		// UUID creation and password hash
		string uuid = GenerateUUID();
		string keyData = GetDataKey(request);

		// Content
		RedisMap map = new();
		map[AccountDataStructure.Structure.UsernameField] = request.Username;
		map[IAuthLink.UUID] = uuid;
		foreach(var kv in request.Fields) {
			map[kv.Key.Name] = kv.Value;
		}

		// Calls
		await RegisterUniquesAsync(uuid, request);

		await _redis.GetDatabase().HashSetAsync(keyData, map.ToArray());
		await _redis.GetDatabase().KeyPersistAsync(keyData);

		return Result<string>.Success(uuid);
	}

	public async Task DeleteAccount(string uuid) {
		var batch = _redis.GetDatabase().CreateBatch();
		await batch.KeyDeleteAsync(uuid);
		foreach(var key in await GetKeysOfStringValue(uuid))
			await batch.KeyDeleteAsync(key);
		batch.Execute();
	}

	private async Task<List<RedisKey>> GetKeysOfStringValue(string value) {
		HashSet<RedisKey> keys = new();
		foreach(var ep in _redis.GetEndPoints())
			keys.UnionWith(_redis.GetServer(ep).Keys());
		keys.RemoveWhere(k => !k.ToString().Contains(PREFIX_UNIQUE));

		List<RedisKey> list = new();
		var db = _redis.GetDatabase();
		foreach(var key in keys) {
			var str = await db.StringGetAsync(key);
			if(value.Equals(str))
				list.Add(key);
		}
		return list;
	}

	public async Task<Result<List<Dictionary<string, string>>>> ListAccounts() {
		HashSet<RedisKey> keys = new();
		foreach(var ep in _redis.GetEndPoints())
			keys.UnionWith(_redis.GetServer(ep).Keys());
		keys.RemoveWhere(k => {
			string[] tokens = k.ToString().Split(":");
			if(tokens.Length == 2)
				return tokens[1].Equals("__unique__");
			return true;
		});

		List<Dictionary<string, string>> list = new();
		foreach(var key in keys) {
			Dictionary<string, string> values = new();
			foreach(var kv in await _redis.GetDatabase().HashGetAllAsync(key)) {
				if(! kv.Name.Equals(AccountDataStructure.Structure.PasswordField))
					values[kv.Name.ToString()] = kv.Value.ToString();
			}
			list.Add(values);
		}

		return Result<List<Dictionary<string, string>>>.Success(list);
	}

	#region util methods
	private static string GetDataKey(LoginRequest request) {
		return PREFIX + Utils.HashString(request.Username) + "--" + Utils.HashString(request.Password);
	}

	private async Task<bool> RegisterUniquesAsync(string uuid, RegisterRequest request) {
		var transaction = _redis.GetDatabase().CreateTransaction();
		if(transaction == null)
			return false;

		foreach(var kv in request.Fields) {
			if(kv.Key.Unique) {
				string key = PREFIX_UNIQUE + kv.Key.Name + ":" + kv.Value;
				_ = transaction.StringSetAsync(key, uuid);
				_ = transaction.KeyPersistAsync(key);
			}
		}
		var usernameKey = PREFIX_UNIQUE + AccountDataStructure.Structure.UsernameField + ":" + request.Username;
		_ = transaction.StringSetAsync(usernameKey, uuid);
		_ = transaction.KeyPersistAsync(usernameKey);

		return await transaction.ExecuteAsync();
	}

	private bool UniqueExists(string key, string value) {
		return _redis.GetDatabase().KeyExists(PREFIX_UNIQUE + key + ":" + value);
	}

	private string GenerateUUID() {
		string uuid;
		do {
			uuid = Utils.RandomString(64);
		} while(_redis.GetDatabase().KeyExists(PREFIX+uuid));
		return uuid;
	}
	#endregion
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
