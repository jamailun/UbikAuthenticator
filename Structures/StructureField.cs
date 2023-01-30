using System.Text.RegularExpressions;
using UbikMmo.Authenticator.AuthLinks;

namespace UbikMmo.Authenticator.Structures;


public class StructureField {

	private const string FIELD_NAME_REGEX = @"^[A-Za-z_\-]{1,32}$";

	public string Name { get; }
	public FieldType Type { get; }
	public bool Required { get; }
	public bool Unique { get; }

	public StructureField(string name, string value) {
		this.Name = name;
		if(! Regex.IsMatch(name, FIELD_NAME_REGEX) || IDataStore.UUID.Equals(name))
			throw new Exception("Illegal name value: \"" + name + "\".");
		foreach(string token in value.Split(" ")) {
			if("required".Equals(token, StringComparison.OrdinalIgnoreCase)) {
				Required = true;
			} else if("unique".Equals(token, StringComparison.OrdinalIgnoreCase)) {
				Unique = true;
			} else {
				Type = FieldTypeExtensions.ParseFieldType(token);
			}
		}
		if(Type == FieldType.Unknown)
			throw new Exception("Unknown type in structure for name '" + name + "' : '" + value + "'.");
	}

	public override string ToString() {
		return "{'" + Name + "' " + Type.ToString() + (Required ? " REQUIRED" : "") + (Unique ? " UNIQUE" : "") + "}";
	}
}