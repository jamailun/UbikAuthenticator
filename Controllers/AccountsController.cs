using Microsoft.AspNetCore.Mvc;
using UbikMmo.Authenticator.Services;
using UbikMmo.Authenticator.AuthLinks;
using UbikMmo.Authenticator.Structures;

namespace UbikMmo.Authenticator.Controllers; 

[ApiController]
public class AccountsController : ControllerBase {

	[HttpPost]
	[Route("/accounts/login/{serverName}")]
	public async Task<IActionResult> Login([FromBody][ModelBinder(BinderType = typeof(ExtractJson))] string json, string serverName) {
		// Parse request
		Result<LoginRequest> requestResult = AccountDataStructure.Structure.TryParseLoginRequest(json);
		if(!requestResult.IsSuccess)
			return BadRequest(requestResult.ErrorContent);

		// Player authentication
		Result<string> result = await AuthLinkFactory.IAuth.LogAccount(requestResult.SuccessValue);
		if(!result.IsSuccess) {
			return BadRequest(result.ErrorContent);
		}
		string uuid = result.SuccessValue ?? throw new Exception("Internal error.");
		string token = AccountsManager.Instance.GetOrCreateTokenForPlayer(uuid);


		// Server check : only after authentication
		var server = ServersManager.Instance.GetServer(serverName);
		if(server == null)
			return BadRequest("Unknown Server '" + serverName + "'.");

		return Ok(new Account.PlayerLoggedIn(token, server.Url));
	}

	[HttpPost]
	[Route("/accounts/register")]
	public async Task<IActionResult> Register([FromBody][ModelBinder(BinderType = typeof(ExtractJson))] string json) {
		// Parse request
		Result<RegisterRequest> requestResult = AccountDataStructure.Structure.TryParseRegisterRequest(json);
		if(!requestResult.IsSuccess)
			return BadRequest(requestResult.ErrorContent);

		Result<string> result = await AuthLinkFactory.IAuth.RegisterAccount(requestResult.SuccessValue);
		if(result.IsSuccess) {
			string uuid = result.SuccessValue ?? throw new Exception("Internal error.");
			AccountsManager.Instance.GetOrCreateTokenForPlayer(uuid);
			return Ok("Account created successfully.");
		}

		return BadRequest(result.ErrorContent);
	}

	[HttpPost]
	[Route("/accounts/check/{key}/{token}")]
	public IActionResult TestPlayer(string key, string token){
		if(!ServersManager.Instance.IsSecretKeyValid(key))
			return BadRequest("Bad secret key for server.");
		string? uuid = AccountsManager.Instance.GetUuidFromToken(token);
		if(uuid == null)
			return NotFound("Token not valid.");
		return Ok(uuid);
	}

	[HttpGet]
	[Route("/accounts/debug_list")]
	public IEnumerable<Account> Test() {
		return AccountsManager.Instance.Debug_GetLoggedAccounts();
	}

}
