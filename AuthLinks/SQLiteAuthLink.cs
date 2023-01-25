using System.Data.SQLite;
using System.Text.Json;

namespace UbikMmo.Authenticator.AuthLinks; 

public class SQLiteAuthLink : IAuthLink, IDisposable {

	private readonly SQLiteConnection sql;

	public SQLiteAuthLink() {
		string? dbUri = Environment.GetEnvironmentVariable("IAUTH.sqlite.path");
		if(dbUri == null) {
			Console.WriteLine("WARNING: No environement variable for 'IAUTH.sqlite.path'.");
			dbUri = "./sqlite.db";
		}
		// Create and open the SQL connection.
		sql = new SQLiteConnection(@"URI=file:" + dbUri);
		sql.Open();
		// Initialize
		Initialize();
	}

	public async Task<Result<string>> LogAccount(string json) {
		BasicLogInRequest? request = JsonSerializer.Deserialize<BasicLogInRequest>(json);
		if(request == null)
			return Result<string>.Error("Invalid JSON for the request.");

		string password = Utils.HashString(request.password);

		using var cmd = new SQLiteCommand(sql);
		cmd.CommandText = @"SELECT uuid FROM Accounts WHERE email=@EMAIL and passwordHash=@PWD;";
		cmd.Parameters.AddWithValue("@EMAIL", request.email ?? throw new Exception("Email CANNOT be null."));
		cmd.Parameters.AddWithValue("@PWD", password);
		await cmd.PrepareAsync();

		string? uuid = (await cmd.ExecuteScalarAsync())?.ToString();
		if(uuid == null)
			return Result<string>.Error("Email or password incorrect.");
		return Result<string>.Success(uuid);
	}

	public async Task<Result<string>> RegisterAccount(string json) {
		BasicRegisterRequest? request = JsonSerializer.Deserialize<BasicRegisterRequest>(json);
		if(request == null)
			return Result<string>.Error("Invalid JSON for the request.");

		if(ExistsInTable("username", request.username ?? throw new Exception("Username CANNOT be null.")))
			return Result<string>.Error("This username is already taken.");
		if(ExistsInTable("email", request.email ?? throw new Exception("Email CANNOT be null.")))
			return Result<string>.Error("This email is already linked to an account.");

		var uuid = GenerateUUID();
		using var cmd = new SQLiteCommand(sql);
		cmd.CommandText = @"INSERT INTO Accounts(uuid, username, email, passwordHash) VALUES(@UUID, @USERNAME, @EMAIL, @PWD);";
		cmd.Parameters.AddWithValue("@UUID", uuid);
		cmd.Parameters.AddWithValue("@USERNAME", request.username);
		cmd.Parameters.AddWithValue("@EMAIL", request.email);
		cmd.Parameters.AddWithValue("@PWD", Utils.HashString(request.password ?? throw new Exception("password CANNOT be null.")));
		await cmd.PrepareAsync();
		await cmd.ExecuteNonQueryAsync();

		return Result<string>.Success(uuid);
	}

	private string GenerateUUID() {
		string uuid;
		do {
			uuid = Utils.RandomString(32);
		} while(ExistsInTable("uuid", uuid));
		return uuid;
	}

	private bool ExistsInTable(string element, string value) {
		using var cmd = new SQLiteCommand(sql);
		cmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM Accounts WHERE {element}=\"{value}\" LIMIT 1);";
		return (long) cmd.ExecuteScalar() == 1;
	}

	#region Initialization
	private void Initialize() {
		using var cmd = new SQLiteCommand(sql);
		cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Accounts (
			  uuid VARCHAR(32) PRIMARY KEY,
			  username VARCHAR(32) NOT NULL UNIQUE,
			  email VARCHAR(64) NOT NULL UNIQUE,
			  passwordHash VARCHAR(128) NOT NULL
			);";

		cmd.ExecuteNonQuery();
	}
	#endregion

	#region dispose
	public void Dispose() {
		sql.Dispose();
		GC.SuppressFinalize(this);
	}
	#endregion
}
