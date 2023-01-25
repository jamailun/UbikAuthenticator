namespace UbikMmo.Authenticator;

public class Result<T> {

	public static Result<T> Success(T value) => new(true, null, value);
	public static Result<T> Error(string error) => new(false, error, default);

	public bool IsSuccess { get; }
	public string? ErrorContent { get; }

	private readonly T? _successValue;
	public T SuccessValue {
		get {
			return _successValue ?? throw new Exception("Cannot access result if result is a failure in " + this);
		}
	}

	private Result(bool success, string? error, T? value) {
		this.IsSuccess = success;
		this.ErrorContent = error;
		this._successValue = value;
	}

	public override string ToString() {
		if(IsSuccess)
			return "Result{Success, " + SuccessValue + "}";
		return "Result{Failure : \"" + ErrorContent + "\"}";
	}
}
