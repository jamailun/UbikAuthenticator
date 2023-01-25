using UbikMmo.Authenticator.Structures;

namespace UbikMmo.Authenticator.AuthLinks; 

public class FakeAuthLink : IAuthLink {

	private readonly List<SavedPlayer> _accounts = new();

	public Task<Result<string>> RegisterAccount(RegisterRequest request) {
		
		if(_accounts.Any(a => a.username.Equals(request.Username)))
			return new(() => Result<string>.Error("This username is already used."));

		var account = new SavedPlayer(request);
		_accounts.Add(account);

		return new(() => Result<string>.Success(account.uuid));
	}

	public Task<Result<string>> LogAccount(LoginRequest request) {
		SavedPlayer? saved = _accounts.Find(a => a.username.Equals(request.Username) && a.password.Equals(request.Password));
		if(saved == null)
			return new(() => Result<string>.Error("Account does not exist. username or password incorrect."));

		return new(() => Result<string>.Success(saved.uuid));
	}

	private class SavedPlayer {
		public string uuid;
		public string username;
		public string password;
		public SavedPlayer(RegisterRequest register) {
			this.username = register.Username ?? "";
			this.password = register.Password ?? "";
			this.uuid = "UUID_"+username;
		}
	}

}
