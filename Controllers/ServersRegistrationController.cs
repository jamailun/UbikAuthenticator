using UbikMmo.Authenticator.Services;
using Microsoft.AspNetCore.Mvc;
using UbikAuthenticator.Authorization;

namespace UbikMmo.Authenticator.Controllers; 

[ApiController]
public class ServersRegistrationController : ControllerBase {

	[HttpGet]
	[Route("/servers/list")]
	public IEnumerable<string> GetServers() {
		return ServersManager.Instance.GetServersNames();
    }

	[HttpPost]
	[Route("/servers/register/")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public ActionResult<string> RegisterServer(Server.ServerDTO serverDto) {
		if(PermissionsGiver.GetPermission(Request) < PermissionLevel.ServerAccess)
			return Unauthorized(new ApiError("401", "unsufficient permissions value."));

		Console.WriteLine("Received " + serverDto);
		// Check data is complete
		if(serverDto.ServerName == null || serverDto.ServerUrl == null)
			return BadRequest(new ApiErrorField("400", serverDto.ServerName == null ? "ServerName" : "ServerUrl", "Invalid form : " + serverDto));

		// Try to register the server
		string? token = ServersManager.Instance.RegisterServer(serverDto);
		if(token != null)
			return token;
		return BadRequest(new ApiErrorField("400", "ServerName", "Server already registered."));
	}

}
