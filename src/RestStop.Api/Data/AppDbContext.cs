namespace RestStop.Api.Data;

/// <summary>
/// Entity Framework Core database context for the RestStop application.
/// Configures all entities, relationships, and PostGIS geometry support.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initialises a new instance of <see cref="AppDbContext"/>.
    /// </summary>
    /// <param name="options">The options used to configure this context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Gets or sets the Users table.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Gets or sets the Locations table.</summary>
    public DbSet<Location> Locations => Set<Location>();

    /// <summary>Gets or sets the Restrooms table.</summary>
    public DbSet<Restroom> Restrooms => Set<Restroom>();

    /// <summary>Gets or sets the Reviews table.</summary>
    public DbSet<Review> Reviews => Set<Review>();

    /// <summary>Gets or sets the OtpCodes table.</summary>
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

    /// <summary>Gets or sets the RefreshTokens table.</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>Gets or sets the FlexPassParkers table.</summary>
    public DbSet<FlexPassParker> FlexPassParkers => Set<FlexPassParker>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
            entity.HasIndex(e => e.Phone).IsUnique().HasFilter("\"Phone\" IS NOT NULL");
            entity.HasIndex(e => e.GoogleId).IsUnique().HasFilter("\"GoogleId\" IS NOT NULL");
            entity.HasIndex(e => e.AppleId).IsUnique().HasFilter("\"AppleId\" IS NOT NULL");
            entity.Property(e => e.Role).HasConversion<string>();
            entity.Property(e => e.UserType).HasConversion<string>();
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Coordinates).HasColumnType("geometry(Point, 4326)");
            entity.HasIndex(e => e.Coordinates).HasMethod("GIST");
            entity.Property(e => e.PlaceType).HasConversion<string>();
        });

        modelBuilder.Entity<Restroom>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Location)
                .WithMany(l => l.Restrooms)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasIndex(e => new { e.LocationId, e.Type }).IsUnique();
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Restroom)
                .WithMany(r => r.Reviews)
                .HasForeignKey(e => e.RestroomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.RestroomId, e.CreatedAt });
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Recipient);
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Token).IsUnique();
        });

        modelBuilder.Entity<FlexPassParker>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Location)
                .WithMany()
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.PassNumber).IsUnique();
            entity.HasIndex(e => e.SentToMyKastl);
        });
    }
}
