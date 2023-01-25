using System.Text.Json.Nodes;

namespace UbikMmo.Authenticator.Structures;

public class AccountDataStructure {

	public string UsernameField { get; }
	public string PasswordField { get; }
	public List<StructureField> Fields { get; }
	public List<StructureField> UniqueFields { get; }

	private static AccountDataStructure? _Instance;
	public static AccountDataStructure Structure {
		get {
			_Instance ??= new AccountDataStructure();
			return _Instance;
		}
	}

	private AccountDataStructure() {
		var fields = Parse(Utils.LoadStructuresFile());
		UsernameField = fields.Find(f => f.Type == FieldType.Username)?.Name ?? throw new Exception();
		PasswordField = fields.Find(f => f.Type == FieldType.Password)?.Name ?? throw new Exception();
		Fields = fields.FindAll(f => f.Type != FieldType.Username && f.Type != FieldType.Password);
		// on le calcule que une fois
		UniqueFields = Fields.FindAll(f => f.Unique);
	}

	// Must contain @USERNAME and @PASSWORD only
	// Note: it must be dynamic because the names of the fields can change!
	public Result<LoginRequest> TryParseLoginRequest(string json) {
		try {
			// Json parsing and validity
			JsonNode? dataNode = JsonObject.Parse(json);
			if(dataNode == null || dataNode.GetType() != typeof(JsonObject))
				return Result<LoginRequest>.Error("Invalid Json");

			// found values
			string? username = null, password = null;
			foreach(var kv in (JsonObject) dataNode) {
				if(kv.Value == null)
					return Result<LoginRequest>.Error($"Invalid value for key '{kv.Key}'.");
				string value = kv.Value.ToString();
				if(kv.Key == UsernameField) {
					username = value;
				} else if(kv.Key == PasswordField) {
					password = value;
				} else {
					return Result<LoginRequest>.Error("Unknown field: '" + kv.Key + "'.");
				}
			}
			// Verify counts
			if(username == null)
				return Result<LoginRequest>.Error("Need a '" + UsernameField + "' field.");
			if(password == null)
				return Result<LoginRequest>.Error("Need a '" + PasswordField + "' field.");
			return Result<LoginRequest>.Success(new(username, password));
		} catch(Exception e) {
			return Result<LoginRequest>.Error(e.Message);
		}
	}

	// Must contain @USERNAME, @PASSWORD and all required fields
	public Result<RegisterRequest> TryParseRegisterRequest(string json) {
		try {
			// Json parsing and validity
			JsonNode? dataNode = JsonObject.Parse(json);
			if(dataNode == null || dataNode.GetType() != typeof(JsonObject))
				return Result<RegisterRequest>.Error("Invalid Json");

			// found values
			string? username = null, password = null;
			Dictionary<StructureField, string> values = new();
			// Validity test
			List<string> requiredFields = new(Fields.FindAll(f => f.Required).Select(f => f.Name));
			foreach(var kv in (JsonObject)dataNode) {
				if(kv.Value == null)
					return Result<RegisterRequest>.Error($"Invalid value for key '{kv.Key}'.");
				string value = kv.Value.ToString();
				if(kv.Key == UsernameField) {
					username = value;
				} else if(kv.Key == PasswordField) {
					password = value;
				} else {
					// Field exist ?
					StructureField? field = Fields.Find(f => f.Name.Equals(kv.Key));
					if(field == null)
						return Result<RegisterRequest>.Error("Unknown field: '"+kv.Key+"'.");
					// Type validity
					if(!field.Type.IsValid(value))
						return Result<RegisterRequest>.Error("Field '" + kv.Key + "' of value '" + value + "' doesn't correspond to type " + field.Type);
					// Required ?
					requiredFields.Remove(kv.Key);

					// add it
					values[field] = value;
				}
			}
			// Verify counts
			if(username == null)
				return Result<RegisterRequest>.Error("Need a '" + UsernameField + "' field.");
			if(password == null)
				return Result<RegisterRequest>.Error("Need a '" + PasswordField + "' field.");
			if(requiredFields.Count > 0)
				return Result<RegisterRequest>.Error($"Missing required field{(requiredFields.Count > 1 ? "s":"")} : [{string.Join(",", requiredFields)}].");
			return Result<RegisterRequest>.Success(new(username, password, values));
		} catch(Exception e) {
			return Result<RegisterRequest>.Error(e.Message);
		}
	}

	private static List<StructureField> Parse(JsonObject root) {
		JsonNode? dataNode = root["account_data"];
		if(dataNode == null)
			throw new Exception("structures file must contain 'account_data' entry.");
		if(dataNode.GetType() != typeof(JsonObject))
			throw new Exception("structures file must contain 'account_data' entry as an object, not an array.");
		JsonObject data = (JsonObject) dataNode;

		List<StructureField> list = new();
		foreach(var kv in data) {
			JsonNode? node = kv.Value;
			if(node == null)
				throw new Exception("structures file contains null value: 'account_data."+kv.Key+"'.");
			list.Add(new(kv.Key, node.ToString()));
		}

		int countUser = list.FindAll(sf => sf.Type == FieldType.Username).Count;
		int countpwd = list.FindAll(sf => sf.Type == FieldType.Password).Count;
		if(countUser != 1 || countpwd != 1)
			throw new Exception("ERROR: 'account_data' of structures file must contains exactly one @USERNAME and @PASSWORD.");

		return list;
	}

	public override string ToString() {
		return "AccountStructure{@USERNAME, @PASSWORD, " + string.Join(", ", Fields) + "}"; 
	}
}

