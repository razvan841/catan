var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Text("Hello World", "text/plain"));

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "healthy" });
});

app.Run();