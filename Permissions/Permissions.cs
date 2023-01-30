using UbikMmo.Authenticator;

namespace UbikAuthenticator.Authorization {
	
	public class PermissionsGiver {

		public const string CONFIG_PERM_SERVER = "SECRET.server";
		public const string CONFIG_PERM_ADMIN = "SECRET.admin";
		
		private readonly Dictionary<string, PermissionLevel> _permissions = new();

		private static PermissionsGiver? _Instance;
		public static PermissionsGiver Instance {
			get {
				_Instance ??= new PermissionsGiver();
				return _Instance;
			}
		}

		private PermissionsGiver() {
			string keyServer = UbikEnvironment.GetString(CONFIG_PERM_SERVER) ?? throw new Exception("No environment variable for " + CONFIG_PERM_SERVER);
			string keyAdmin = UbikEnvironment.GetString(CONFIG_PERM_ADMIN) ?? throw new Exception("No environment variable for " + CONFIG_PERM_ADMIN);
			_permissions[keyServer] = PermissionLevel.ServerAccess;
			_permissions[keyAdmin] = PermissionLevel.AdminAccess;
		}

		public static PermissionLevel GetPermission(HttpRequest request) {
			string token = request.Headers.Authorization.Count > 0 ? request.Headers.Authorization.Aggregate((s1, s2) => s1 + " " + s2) : "";
			return GetPermission(token);
		}

		public static PermissionLevel GetPermission(string token) {
			return Instance._permissions.GetValueOrDefault(token, PermissionLevel.None);
		}

	}

	public enum PermissionLevel : int {
		None = 0,
		ServerAccess = 8,
		AdminAccess = 16
	}

	public static class PermissionLevelExtension {
		public static bool IsAtLeast(this PermissionLevel perm, PermissionLevel other) {
			return perm >= other;
		}
	}
}
