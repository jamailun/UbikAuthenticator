namespace UbikMmo.Authenticator.Structures;
public class StructureField {
	public string Name { get; }
	public FieldType Type { get; }
	public bool Required { get; }

	public StructureField(string name, string value) {
		this.Name = name;
		foreach(var token in value.Split(" ")) {
			if(token.Equals("required")) {
				Required = true;
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