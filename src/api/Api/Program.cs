using CivicConnect.Data;

var builder = WebApplication.CreateBuilder(args);

// Register data access
builder.Services.AddDataAccess(builder.Configuration);

var app = builder.Build();

app.Run();
