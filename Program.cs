using net_backend;

var builder = WebApplication.CreateBuilder(args);
builder.AddServices();

var app = builder.Build();
await app.Configure();

app.Run();
