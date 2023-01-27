using UbikMmo.Authenticator.Structures;

namespace UbikMmo.Authenticator.AuthLinks; 

public class FakeDataStore : IDataStore {

	private readonly List<SavedAccount> _accounts = new();

	public Task<Result<string>> RegisterAccount(RegisterRequest request) {
		
		if(_accounts.Any(a => a.username.Equals(request.Username)))
			return new(() => Result<string>.Error("This username is already used."));

		var account = new SavedAccount(request);
		_accounts.Add(account);

		return new(() => Result<string>.Success(account.uuid));
	}

	public Task<Result<string>> LogAccount(LoginRequest request) {
		SavedAccount? saved = _accounts.Find(a => a.username.Equals(request.Username) && a.password.Equals(request.Password));
		if(saved == null)
			return new(() => Result<string>.Error("Account does not exist. username or password incorrect."));

		return new(() => Result<string>.Success(saved.uuid));
	}

	public Task DeleteAccount(string s) {
		var account = _accounts.Find(s => s.uuid.Equals(s));
		if(account != null)
			_accounts.Remove(account);
		return new(() => { });
	}

	public Task<Result<List<Dictionary<string, string>>>> ListAccounts() {
		List<Dictionary<string, string>> list = new();
		foreach(var sa in _accounts) {
			list.Add(new() {
				["uuid"] = sa.uuid,
				["username"] = sa.username,
				["password"] = sa.password
			});
		}
		return new(() => Result<List<Dictionary<string, string>>>.Success(list));
	}

	private class SavedAccount {
		public string uuid;
		public string username;
		public string password;
		public SavedAccount(RegisterRequest register) {
			this.username = register.Username ?? "";
			this.password = register.Password ?? "";
			this.uuid = "UUID_"+username;
		}
	}

}
