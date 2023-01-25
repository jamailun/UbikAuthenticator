using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace UbikMmo.Authenticator.Structures;

public enum FieldType {
	Unknown = 0,
	// Mandatory
	Username = 1,
	Password,
	// Typed
	String = 10,
	Integer,
	Float,
	Bool,
	// Special
	Date,
	Json,
	Email
}

public static class FieldTypeExtensions {
	public static FieldType ParseFieldType(string value) {
		return value switch {
			// Special
			"@USERNAME" => FieldType.Username,
			"@PASSWORD" => FieldType.Password,
			// Real types
			"string" or "str" => FieldType.String,
			"integer" or "int" => FieldType.Integer,
			"float" => FieldType.Float,
			"boolean" or "bool" => FieldType.Bool,
			// Fake types
			"date" => FieldType.Date,
			"json" => FieldType.Json,
			"email" => FieldType.Email,
			_ => FieldType.Unknown
		};
	}
	public static bool IsValid(this FieldType type, string value) {
		switch(type) {
			case FieldType.Username:
			case FieldType.Password:
			case FieldType.String:
				return true;
			case FieldType.Integer:
				return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
			case FieldType.Float:
				return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
			case FieldType.Bool:
				return bool.TryParse(value, out _);
			case FieldType.Email:
				string regex = Environment.GetEnvironmentVariable("EMAIL_REGEX") ?? "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$";
				return Regex.Match(value, regex, RegexOptions.IgnoreCase).Success;
			case FieldType.Json:
				try {
					JsonValue.Parse(value);
					return true;
				} catch {
					return false;
				}
			case FieldType.Date:
				return DateTime.TryParse(value, out _);
		}
		Console.WriteLine("Unknown type : '" + type + "'.");
		return false;
	}
	public static string ToSqliteType(this FieldType type) {
		return type switch {
			FieldType.String 
				or FieldType.Json => "TEXT",
			FieldType.Integer => "INTEGER",
			FieldType.Float => "REAL",
			FieldType.Bool => "BOOLEAN",
			FieldType.Email => "VARCHAR(64)",
			FieldType.Date => "DATE",
			_ => throw new Exception("Unexpected type here: '" + type + "'."),
		};
	}

	public static string DefaultValue(this FieldType type) {
		return type switch {
			FieldType.Integer
				or FieldType.Float => "0",
			FieldType.Bool => "FALSE",
			FieldType.Date => "\"1971-01-01\"",
			_ => "\"\"",
		};
	}
}