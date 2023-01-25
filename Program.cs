var reg = UbikMmo.Authenticator.Structures.AccountDataStructure.Structure.TryParseRegisterRequest(
	"{\"username\":\"michel_xx\", \"password\":\"abcd\", \"email\":\"monmail.ouaf@mail.com\", \"firstname\":\"michel\", \"birthdate\":\"2020-10-10\", \"is_nice\":true,\"ratio\":32.2}"
);
Console.WriteLine(reg);

var log = UbikMmo.Authenticator.Structures.AccountDataStructure.Structure.TryParseLoginRequest(
	"{\"username\":\"michel_xx\", \"password\":\"abcd\"}"
);
Console.WriteLine(log);

/*var builder = WebApplication.CreateBuilder(args);

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
	UbikMmo.Authenticator.Utils.DebugStartup();
}

//app.UseHttpsRedirection();
//app.UseAuthorization();

app.MapControllers();

app.Run();
// */