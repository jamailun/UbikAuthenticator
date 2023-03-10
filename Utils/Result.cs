namespace UbikMmo.Authenticator;

public class Result<T> {

	public static Result<T> Success(T value) => new(true, null, value);
	public static Result<T> Error(string error) => new(false, error, default);

	public static Result<T> DuplicateError(string field) => new("400", field, "Duplicate value for field '" + field + "'.");
	public static Result<T> NotFoundError(string msg) => new("404", null, msg);
	public static Result<T> InternalError(string msg) => new("500", null, msg);

	public bool IsSuccess { get; }
	public string? ErrorContent { get; }

	private ApiError? _apiError;
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

	private Result(string errorCode, string? errorField, string errorContent) {
		this.IsSuccess = false;
		this.ErrorContent = errorContent;
		if(errorField == null)
			this._apiError = new ApiError(errorCode, errorContent);
		else
			this._apiError = new ApiErrorField(errorCode, errorField, errorContent);
	}

	internal ApiError ToApiError() {
		_apiError ??= new ApiError("400", ErrorContent ?? "");
		return _apiError;
	}

	public override string ToString() {
		if(IsSuccess)
			return "Result{Success, " + SuccessValue + "}";
		return "Result{Failure : \"" + ErrorContent + "\"}";
	}
}

internal class ApiError {
	public string code { get; }
	public string details { get; }

	public ApiError(string code, string details) {
		this.code = code;
		this.details = details;
	}
}
internal class ApiErrorField : ApiError {
	public string field { get; }

	public ApiErrorField(string code, string field, string details) : base(code, details) {
		this.field = field;
	}
}