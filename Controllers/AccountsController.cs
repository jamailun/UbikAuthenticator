using Microsoft.AspNetCore.Mvc;
using UbikMmo.Authenticator.Services;
using UbikMmo.Authenticator.AuthLinks;
using UbikMmo.Authenticator.Structures;
using UbikAuthenticator.Authorization;

namespace UbikMmo.Authenticator.Controllers; 

[ApiController]
public class AccountsController : ControllerBase {

	[HttpPost]
	[Route("/accounts/login/{serverName}")]
	// Permission == ALL
	public async Task<IActionResult> Login([FromBody][ModelBinder(BinderType = typeof(ExtractJson))] string json, string serverName) {
		// Parse request
		Result<LoginRequest> requestResult = AccountDataStructure.Structure.TryParseLoginRequest(json);
		if(!requestResult.IsSuccess)
			return BadRequest(requestResult.ErrorContent);

		// Player authentication
		Result<string> result = await DataStoreFactory.IAuth.LogAccount(requestResult.SuccessValue);
		if(!result.IsSuccess) {
			return BadRequest(result.ErrorContent);
		}
		string uuid = result.SuccessValue ?? throw new Exception("Internal error.");
		string token = AccountsManager.Instance.GetOrCreateTokenForAccount(uuid);


		// Server check : only after authentication
		var server = ServersManager.Instance.GetServer(serverName);
		if(server == null)
			return BadRequest("Unknown Server '" + serverName + "'.");

		return Ok(new Account.PlayerLoggedIn(token, server.Url));
	}

	[HttpPost]
	[Route("/accounts/register")]
	// Permission == ALL
	public async Task<IActionResult> Register([FromBody][ModelBinder(BinderType = typeof(ExtractJson))] string json) {
		// Parse request
		Result<RegisterRequest> requestResult = AccountDataStructure.Structure.TryParseRegisterRequest(json);
		if(!requestResult.IsSuccess)
			return BadRequest(requestResult.ErrorContent);

		Result<string> result = await DataStoreFactory.IAuth.RegisterAccount(requestResult.SuccessValue);
		if(result.IsSuccess) {
			string uuid = result.SuccessValue ?? throw new Exception("Internal error.");
			AccountsManager.Instance.GetOrCreateTokenForAccount(uuid);
			return Ok("Account created successfully.");
		}

		return BadRequest(result.ErrorContent);
	}

	[HttpDelete]
	[Route("/accounts/{uuid}")]
	// Permission == ADMIN
	public async Task<IActionResult> DeleteAccount(string uuid) {
		if(PermissionsGiver.GetPermission(Request) < PermissionLevel.AdminAccess)
			return Unauthorized("Not enough permissions");

		await DataStoreFactory.IAuth.DeleteAccount(uuid);
		AccountsManager.Instance.Remove(uuid);

		return Ok();
	}

	[HttpGet]
	[Route("/accounts")]
	// Permission == ADMIN
	public async Task<IActionResult> ListAllAccounts() {
		if(PermissionsGiver.GetPermission(Request) < PermissionLevel.AdminAccess)
			return Unauthorized("Not enough permissions");

		var result = await DataStoreFactory.IAuth.ListAccounts();

		if(!result.IsSuccess)
			return BadRequest(result.ErrorContent);

		return Ok(result.SuccessValue);
	}

	[HttpPost]
	[Route("/accounts/check/{account_token}")]
	// Permission == SERVER
	public IActionResult CheckIfAccountCanConnect(string account_token) {
		if(PermissionsGiver.GetPermission(Request) < PermissionLevel.ServerAccess)
			return BadRequest("Bad secret key for server.");
		string? uuid = AccountsManager.Instance.GetUuidFromToken(account_token);
		if(uuid == null)
			return NotFound("Token not valid.");
		return Ok(uuid);
	}

	[HttpGet]
	[Route("/accounts/debug_list")]
	//TODO remove
	public IEnumerable<Account> Test() {
		return AccountsManager.Instance.Debug_GetLoggedAccounts();
	}

}
