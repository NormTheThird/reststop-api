var builder = WebApplication.CreateBuilder(args);
builder.AddVaultConfiguration();
builder.ConfigureDatabase();
builder.ConfigureAwsClients();
builder.ConfigureJwtAuthentication();
builder.Services.AddAuthorization();
builder.ConfigureCors();
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddApiVersioningSupport();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

if (Environment.GetEnvironmentVariable("RUN_MIGRATIONS") == "true")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    return;
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();
app.UseMiddleware<MinimumVersionMiddleware>();

app.UseSwaggerConfig();

app.UseHttpsRedirection();
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
