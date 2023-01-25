namespace UbikMmo.Authenticator.AuthLinks;

public interface IAuthLink {

	public Task<Result<string>> RegisterAccount(string json);

	public Task<Result<string>> LogAccount(string json);

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
