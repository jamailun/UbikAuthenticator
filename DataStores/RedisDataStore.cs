using StackExchange.Redis;
using System.Collections.Generic;
using System.Security.Cryptography;
using UbikMmo.Authenticator.Structures;

namespace UbikMmo.Authenticator.AuthLinks;

public class RedisDataStore : IDataStore {

	private const string PREFIX = "accounts:";
	private const string PREFIX_UNIQUE = PREFIX+"unique:";
	private const string PREFIX_DATA = PREFIX+"data:";
	private const string PREFIX_USERNAME = PREFIX+"username:";

	private readonly ConnectionMultiplexer _redis;

	public RedisDataStore() {
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

		// Get UUID from username
		string? uuid = await db.StringGetAsync(PREFIX_USERNAME + request.Username);
		if(uuid == null)
			return Result<string>.NotFoundError("Username or password incorrect.");

		// Get data from UUID
		HashEntry[]? entries = await db.HashGetAllAsync(PREFIX_DATA + uuid);
		if(entries == null)
			return Result<string>.InternalError("No account. Internal data has been lost.");
		RedisMap map = new(entries);

		// Compare passwords in data
		string password = Utils.HashString(request.Password);
		if(!string.Equals(map[AccountDataStructure.Structure.PasswordField], password))
			return Result<string>.NotFoundError("Username or password incorrect.");

		// Return UUID
		return Result<string>.Success(uuid);
	}

	public async Task<Result<string>> RegisterAccount(RegisterRequest request) {
		var db = _redis.GetDatabase();
		// Duplicate test
		if(await db.KeyExistsAsync(PREFIX_USERNAME + request.Username))
			return Result<string>.DuplicateError(AccountDataStructure.Structure.UsernameField);
		foreach(var field in AccountDataStructure.Structure.UniqueFields) {
			if(UniqueExists(field.Name, request.Fields[field]))
				return Result<string>.DuplicateError(field.Name);
		}

		// UUID and password hash generation
		string uuid = GenerateUUID();
		string password = Utils.HashString(request.Password);

		// Content generation
		RedisMap map = new();
		map[AccountDataStructure.Structure.UsernameField] = request.Username;
		map[AccountDataStructure.Structure.PasswordField] = password;
		map[IDataStore.UUID] = uuid;
		foreach(var kv in request.Fields) {
			map[kv.Key.Name] = kv.Value;
		}

		// username -> UUID
		string keyUsername = PREFIX_USERNAME + request.Username;
		await db.StringSetAsync(keyUsername, uuid);
		await db.KeyPersistAsync(keyUsername);

		// UUID -> [data]
		string keyData = PREFIX_DATA + uuid;
		await db.HashSetAsync(keyData, map.ToArray());
		await db.KeyPersistAsync(keyData);

		// Unique fields
		await RegisterUniquesAsync(uuid, request);

		// Success
		return Result<string>.Success(uuid);
	}

	public async Task DeleteAccount(string uuid) {
		var db = _redis.GetDatabase();

		// Get data relative to the account
		var dataRedis = await db.HashGetAllAsync(PREFIX_DATA + uuid);
		if(dataRedis.Length == 0)
			return; // already deleted or doesn't exist
		RedisMap data = new(dataRedis);

		// Compute all keys to remove
		List<string> toDeleteKeys = new() {
			// uuid -> data
			PREFIX_DATA + uuid,
			// username -> uuid
			PREFIX_USERNAME + data[AccountDataStructure.Structure.UsernameField]
		};
		foreach(var field in AccountDataStructure.Structure.UniqueFields) {
			// uniques
			toDeleteKeys.Add(PREFIX_UNIQUE + field.Name + ":" + data[field.Name]);
		}

		// Delete keys
		foreach(var key in toDeleteKeys)
			await db.KeyDeleteAsync(key);
	}

	public async Task<Result<List<Dictionary<string, string>>>> ListAccounts() {
		HashSet<RedisKey> keys = new();
		foreach(var ep in _redis.GetEndPoints()) {
			var rawKeys = new HashSet<RedisKey>(_redis.GetServer(ep).Keys());
			rawKeys.RemoveWhere(rk => ! rk.ToString().StartsWith(PREFIX_DATA));
			keys.UnionWith(rawKeys);
		}

		List<Dictionary<string, string>> list = new();
		var db = _redis.GetDatabase();
		foreach(var key in keys) {
			Dictionary<string, string> values = new();
			foreach(var kv in await db.HashGetAllAsync(key)) {
				if(! kv.Name.Equals(AccountDataStructure.Structure.PasswordField))
					values[kv.Name.ToString()] = kv.Value.ToString();
			}
			list.Add(values);
		}

		return Result<List<Dictionary<string, string>>>.Success(list);
	}

	#region util methods
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
