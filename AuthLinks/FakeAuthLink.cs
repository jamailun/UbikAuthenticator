using System.Text.Json;

namespace UbikMmo.Authenticator.AuthLinks; 

public class FakeAuthLink : IAuthLink {

	private readonly List<SavedPlayer> _accounts = new();

	public Task<Result<string>> RegisterAccount(string json) {
		BasicRegisterRequest? request = JsonSerializer.Deserialize<BasicRegisterRequest>(json);
		if(request == null)
			return new(() => Result<string>.Error("Invalid JSON for the request."));

		if(_accounts.Any(a => a.email.Equals(request.email)))
			return new(() => Result<string>.Error("This email is already used."));

		var account = new SavedPlayer(request);
		_accounts.Add(account);

		return new(() => Result<string>.Success(account.uuid));
	}

	public Task<Result<string>> LogAccount(string json) {
		BasicLogInRequest? request = JsonSerializer.Deserialize<BasicLogInRequest>(json);
		if(request == null)
			return new(() => Result<string>.Error("Invalid JSON for the request."));

		SavedPlayer? saved = _accounts.Find(a => a.email.Equals(request.email) && a.password.Equals(request.password));
		if(saved == null)
			return new(() => Result<string>.Error("Account does not exist. Email or password incorrect."));

		return new(() => Result<string>.Success(saved.uuid));
	}

	private class SavedPlayer {
		public string uuid;
		public string username;
		public string email;
		public string password;
		public SavedPlayer(BasicRegisterRequest register) {
			this.username = register.username ?? "";
			this.email = register.email ?? "";
			this.password = register.password ?? "";
			this.uuid = "UUID_"+username;
		}
	}

}
