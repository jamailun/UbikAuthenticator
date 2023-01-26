using System;
using System.Data.SQLite;
using System.Text.Json;
using UbikMmo.Authenticator.Structures;

namespace UbikMmo.Authenticator.AuthLinks; 

public class SQLiteAuthLink : IAuthLink, IDisposable {

	private readonly SQLiteConnection sql;

	public SQLiteAuthLink() {
		string? dbUri = Environment.GetEnvironmentVariable("STORE.sqlite.path");
		if(dbUri == null) {
			Console.WriteLine("WARNING: No environement variable for 'STORE.sqlite.path'.");
			dbUri = "./sqlite.db";
		}
		// Create and open the SQL connection.
		sql = new SQLiteConnection(@"URI=file:" + dbUri);
		sql.Open();
		// Initialize
		Initialize();
	}

	public async Task<Result<string>> LogAccount(LoginRequest request) {
		string password = Utils.HashString(request.Password);

		using var cmd = new SQLiteCommand(sql);
		cmd.CommandText = @"SELECT "+IAuthLink.UUID+" FROM Accounts WHERE username=@USER and passwordHash=@PWD;";
		cmd.Parameters.AddWithValue("@USER", request.Username ?? throw new Exception("Email CANNOT be null."));
		cmd.Parameters.AddWithValue("@PWD", password);
		await cmd.PrepareAsync();

		string? uuid = (await cmd.ExecuteScalarAsync())?.ToString();
		if(uuid == null)
			return Result<string>.Error("Username or password incorrect.");
		return Result<string>.Success(uuid);
	}

	public async Task<Result<string>> RegisterAccount(RegisterRequest request) {
		try {
			// Check duplicates
			if(ExistsInTable("username", request.Username))
				return Result<string>.Error("This username is already taken.");
			foreach(var field in AccountDataStructure.Structure.UniqueFields) {
				if(ExistsInTable(field.Name, request.Fields[field]))
					return Result<string>.Error("Duplicate " + field.Name + " value : '" + request.Fields[field]+"'.");
			}

			var uuid = GenerateUUID();
			using var cmd = new SQLiteCommand(sql);

			string tableSignature = IAuthLink.UUID + ", username, passwordHash";
			string tableValues = "@UUID, @USERNAME, @PWD";
			cmd.Parameters.AddWithValue("@UUID", uuid);
			cmd.Parameters.AddWithValue("@USERNAME", request.Username);
			cmd.Parameters.AddWithValue("@PWD", Utils.HashString(request.Password));

			foreach(var kv in request.Fields) {
				var field = kv.Key;
				string parameter = "@val_" + field.Name;
				// append
				tableSignature += ", " + field.Name;
				tableValues += ", " + parameter;
				// parametrize
				cmd.Parameters.AddWithValue(parameter, kv.Value);
			}
			
			// Set statement
			cmd.CommandText = $"INSERT INTO Accounts({tableSignature}) VALUES({tableValues});";
			await cmd.PrepareAsync();

			await cmd.ExecuteNonQueryAsync();

			return Result<string>.Success(uuid);
		} catch(Exception e) {
			return Result<string>.Error(e.Message);
		}
	}

	public async Task DeleteAccount(string uuid) {
		using var cmd = new SQLiteCommand(sql);

		cmd.CommandText = $"DELETE FROM Accounts WHERE {IAuthLink.UUID}=@UUID;";
		cmd.Parameters.AddWithValue("@UUID", uuid);
		await cmd.PrepareAsync();

		await cmd.ExecuteNonQueryAsync();
	}

	public async Task<Result<List<Dictionary<string, string>>>> ListAccounts() {
		List<Dictionary<string, string>> list = new();

		using var cmd = new SQLiteCommand(sql);
		cmd.CommandText = $"SELECT * FROM Accounts WHERE 1";

		using var reader = await cmd.ExecuteReaderAsync();

		while(reader.Read()) {
			Dictionary<string, string> values = new() {
				[AccountDataStructure.Structure.UsernameField] = reader[AccountDataStructure.Structure.UsernameField]?.ToString() ?? ""
			};
			foreach(var f in AccountDataStructure.Structure.Fields) {
				values[f.Name] = f.Type.Reformat(reader[f.Name]?.ToString());
			}
			list.Add(values);
		}

		return Result<List<Dictionary<string, string>>>.Success(list);
	}

	#region utils methods
	private string GenerateUUID() {
		string uuid;
		do {
			uuid = Utils.RandomString(32);
		} while(ExistsInTable(IAuthLink.UUID, uuid));
		return uuid;
	}

	private bool ExistsInTable(string element, string value) {
		using var cmd = new SQLiteCommand(sql);
		cmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM Accounts WHERE {element}=\"{value}\" LIMIT 1);";
		return (long) cmd.ExecuteScalar() == 1;
	}
	#endregion

	#region Initialization
	private void Initialize() {
		using var cmd = new SQLiteCommand(sql);
		cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Accounts (
			  " + IAuthLink.UUID + @" VARCHAR(64) PRIMARY KEY,
			  username VARCHAR(64) NOT NULL UNIQUE,
			  passwordHash VARCHAR(256) NOT NULL";
		foreach(var field in AccountDataStructure.Structure.Fields) {
			cmd.CommandText += ", " + field.Name + " " + field.Type.ToSqliteType() + " NOT NULL DEFAULT " + field.Type.DefaultValue() + (field.Unique ? " UNIQUE":"");
		}
		cmd.CommandText += ");";

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
