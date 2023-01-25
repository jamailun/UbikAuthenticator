using UbikMmo.Authenticator.Structures;

namespace UbikMmo.Authenticator.AuthLinks;

public interface IAuthLink {

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

}

[Serializable]
public class BasicLogInRequest {
	public string? email { get; set; }
	public string? password { get; set; }
}

[Serializable]
public class BasicRegisterRequest {
	public string? username { get; set; }
	public string? email { get; set; }
	public string? password { get; set; }
}
