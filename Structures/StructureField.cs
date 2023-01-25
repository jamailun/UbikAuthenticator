namespace UbikMmo.Authenticator.Structures;
public class StructureField {
	public string Name { get; }
	public FieldType Type { get; }
	public bool Required { get; }
	public bool Unique { get; }

	public StructureField(string name, string value) {
		this.Name = name;
		if(name.Length == 0 || name.Contains(' ') || Utils.HasNonASCIIChars(name))
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
		return "{" + Name + " " + Type.ToString() + (Required ? " REQUIRED" : "") + "}";
	}
}