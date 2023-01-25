namespace UbikMmo.Authenticator.Structures;

public class LoginRequest {
	public string Username { get; }
	public string Password { get; }

	public LoginRequest(string username, string password) {
		this.Username = username;
		this.Password = password;
	}

	public override string ToString() {
		return "LoginRequest{" + Username + "/" + Password + "}";
	}
}

public class RegisterRequest : LoginRequest {

	public Dictionary<StructureField, string> Fields { get; }

	public RegisterRequest(string u, string p, Dictionary<StructureField, string> values) : base(u, p) {
		Fields = values;
	}

	public override string ToString() {
		return "RegisterRequest{" + Username + "/" + Password  + ", [" + string.Join(',', Fields) + "]}";
	}
}