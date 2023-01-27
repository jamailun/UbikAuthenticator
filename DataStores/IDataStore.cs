using UbikMmo.Authenticator.Structures;

namespace UbikMmo.Authenticator.AuthLinks;

/// <summary>
/// A object linking UbikAuthenticator with a storage system.
/// </summary>
public interface IDataStore {

	public const string UUID = "__uuid__";

	/// <summary>
	/// Register an account.
	/// </summary>
	/// <param name="request">The register with account data</param>
	/// <returns>the UUID of the account</returns>
	public Task<Result<string>> RegisterAccount(RegisterRequest request);

	/// <summary>
	/// Login to an account. The server doesn't matter here.
	/// </summary>
	/// <param name="json">The login request, with username and password.</param>
	/// <returns>an access token for the account</returns>
	public Task<Result<string>> LogAccount(LoginRequest json);

	public Task DeleteAccount(string uuid);

	public Task<Result<List<Dictionary<string, string>>>> ListAccounts();

}