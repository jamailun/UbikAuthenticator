using UbikMmo.Authenticator.AuthLinks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
    Console.WriteLine("Develoment mode enabled.");
}

// Check account structure
Console.WriteLine(UbikMmo.Authenticator.Structures.AccountDataStructure.Structure);
// Redis debug
if(AuthLinkFactory.IAuth.GetType() == typeof(RedisAuthLink))
    ((RedisAuthLink) AuthLinkFactory.IAuth).DebugClear(false);

//app.UseHttpsRedirection();
//app.UseAuthorization();

app.MapControllers();

app.Run();
// */