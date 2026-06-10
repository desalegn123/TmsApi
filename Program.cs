var builder = WebApplication.CreateBuilder(args);

// Services: add authentication / authorization services
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// TODO 1: Register routing in the pipeline where it belongs for your app.
app.UseRouting();

// TODO 2: Register authentication and authorization in the pipeline.
app.UseAuthentication();
app.UseAuthorization();

// TODO 3: Map GET /api/assessments/results and require authorization.
app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        Message = "Assessment results retrieved successfully."
    });
}).RequireAuthorization();

app.Run();