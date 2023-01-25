using UbikMmo.Authenticator.Services;
using Microsoft.AspNetCore.Mvc;

namespace UbikMmo.Authenticator.Controllers; 

[ApiController]
public class ServersRegistrationController : ControllerBase {

	[HttpGet]
	[Route("/servers/list")]
	public IEnumerable<string> GetServers() {
		return ServersManager.Instance.GetServersNames();
    }

	[HttpPost]
	[Route("/servers/register/{key}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<string> RegisterServer(string key, Server.ServerDTO serverDto) {
		Console.WriteLine("Received " + serverDto);
		// Check data is complete
		if(serverDto.ServerName == null || serverDto.ServerUrl == null)
			return BadRequest("Invalid form : " + serverDto);

		// Check secret is valid
		if (!ServersManager.Instance.IsSecretKeyValid(key))
			return BadRequest("Invalid secret key value.");

		// Try to register the server
		string? token = ServersManager.Instance.RegisterServer(serverDto);
		if(token != null)
			return token;
		return BadRequest("Server already registered.");
	}

}
