using UbikAuthenticator.Authorization;
using UbikMmo.Authenticator.AuthLinks;
using UbikMmo.Authenticator.Structures;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if(string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Production"))
	builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(80));

var app = builder.Build();

// Develoment mode
if (app.Environment.IsDevelopment()) {
	Console.WriteLine("Development mode enabled.");

	// Swagger
	app.UseSwagger();
    app.UseSwaggerUI();

	// Check permissions
	Console.WriteLine("Loading permissions...");
	Console.WriteLine(PermissionsGiver.Instance != null);

	// Check account structure
	Console.WriteLine("Loading account structure...");
	Console.WriteLine(AccountDataStructure.Structure);

	// Check authentication link
	Console.WriteLine("Loading authentication link...");
	Console.WriteLine(DataStoreFactory.IAuth != null);
}

//app.UseHttpsRedirection();
//app.UseAuthorization();

app.MapControllers();

app.Run();
// */