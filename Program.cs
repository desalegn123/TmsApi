using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Tms.Api.Filters;
using Tms.Api.Services;
using TmsApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions,
    TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TmsDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else if (app.Environment.IsProduction())
{
    app.UseExceptionHandler();
}

app.MapGet("/api/error", () =>
{
    throw new Exception("Simulated database failure for ProblemDetails testing");
});

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});
if (app.Environment.IsDevelopment())
{
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
await TmsApi.Persistence.DataSeeder.SeedAsync(context);
}

app.Run();
