using Amazon.Runtime;
using RestStop.Api.Infrastructure.Vault;

namespace RestStop.Api.Extensions;

/// <summary>
/// Extension methods for <see cref="WebApplicationBuilder"/> that configure infrastructure,
/// authentication, and cross-cutting concerns.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>Loads secrets from Vault into the configuration pipeline. Must be called first.</summary>
    public static void AddVaultConfiguration(this WebApplicationBuilder builder)
        => VaultConfigurationExtensions.AddVaultConfiguration(builder);

    /// <summary>Registers the EF Core database context with PostgreSQL and PostGIS.</summary>
    public static void ConfigureDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("Default"),
                o => o.UseNetTopologySuite()));
    }

    /// <summary>Registers AWS SES and SNS singleton clients using credentials from Vault.</summary>
    public static void ConfigureAwsClients(this WebApplicationBuilder builder)
    {
        var region = Amazon.RegionEndpoint.GetBySystemName(
            builder.Configuration["Aws:Region"] ?? "us-east-1");

        var sesCredentials = new BasicAWSCredentials(
            builder.Configuration["Aws:SesAccessKeyId"]
                ?? throw new InvalidOperationException("Aws:SesAccessKeyId is not configured."),
            builder.Configuration["Aws:SesSecretAccessKey"]
                ?? throw new InvalidOperationException("Aws:SesSecretAccessKey is not configured."));

        var snsCredentials = new BasicAWSCredentials(
            builder.Configuration["Aws:SnsAccessKeyId"]
                ?? throw new InvalidOperationException("Aws:SnsAccessKeyId is not configured."),
            builder.Configuration["Aws:SnsSecretAccessKey"]
                ?? throw new InvalidOperationException("Aws:SnsSecretAccessKey is not configured."));

        builder.Services.AddSingleton<IAmazonSimpleEmailService>(
            _ => new AmazonSimpleEmailServiceClient(sesCredentials, region));

        builder.Services.AddSingleton<IAmazonSimpleNotificationService>(
            _ => new AmazonSimpleNotificationServiceClient(snsCredentials, region));
    }

    /// <summary>Configures JWT Bearer authentication from <c>Jwt:*</c> config keys.</summary>
    public static void ConfigureJwtAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSecret = builder.Configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "role",
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JWT");
                        logger.LogError(ctx.Exception,
                            "JWT authentication failed: {Message}", ctx.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });
    }

    /// <summary>Adds CORS using origins from <c>Cors:AllowedOrigins</c> config.</summary>
    public static void ConfigureCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy.WithOrigins(
                        builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                            ?? ["http://localhost:5173"])
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
    }

    /// <summary>Configures the Swagger UI middleware on the <see cref="WebApplication"/>.</summary>
    public static void UseSwaggerConfig(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestStop API v1");
            c.RoutePrefix = "swagger";
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });
    }
}
