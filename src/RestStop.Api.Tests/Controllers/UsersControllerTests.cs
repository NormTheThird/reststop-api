using Microsoft.AspNetCore.Mvc;

namespace RestStop.Api.Tests.Controllers;

public class UsersControllerTests
{
    private static AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }

    private static User Seed(AppDbContext db, string email = "user@test.com",
        string? phone = null, string? username = null,
        UserRole role = UserRole.User, bool isActive = true)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Phone = phone,
            Username = username,
            Role = role,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_NoSearch_ReturnsAllUsers()
    {
        using var db = CreateDb();
        Seed(db, "a@test.com");
        Seed(db, "b@test.com");
        var sut = new UsersController(db);

        var result = await sut.GetAll();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value!;
        ((int)body.GetType().GetProperty("total")!.GetValue(body)!).Should().Be(2);
    }

    [Fact]
    public async Task GetAll_SearchByEmail_FiltersResults()
    {
        using var db = CreateDb();
        Seed(db, "alpha@test.com");
        Seed(db, "beta@test.com");
        var sut = new UsersController(db);

        var result = await sut.GetAll(search: "alpha");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value!;
        ((int)body.GetType().GetProperty("total")!.GetValue(body)!).Should().Be(1);
    }

    [Fact]
    public async Task GetAll_SearchByPhone_FiltersResults()
    {
        using var db = CreateDb();
        Seed(db, "a@test.com", phone: "+12125550001");
        Seed(db, "b@test.com", phone: "+12125550002");
        var sut = new UsersController(db);

        var result = await sut.GetAll(search: "0001");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ((int)ok.Value!.GetType().GetProperty("total")!.GetValue(ok.Value)!).Should().Be(1);
    }

    [Fact]
    public async Task GetAll_SearchByUsername_FiltersResults()
    {
        using var db = CreateDb();
        Seed(db, "a@test.com", username: "truckerjoe");
        Seed(db, "b@test.com", username: "citydriver");
        var sut = new UsersController(db);

        var result = await sut.GetAll(search: "trucker");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ((int)ok.Value!.GetType().GetProperty("total")!.GetValue(ok.Value)!).Should().Be(1);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingUser_ReturnsDto()
    {
        using var db = CreateDb();
        var user = Seed(db, "found@test.com");
        var sut = new UsersController(db);

        var result = await sut.GetById(user.Id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<AdminUserDto>()
            .Which.Email.Should().Be("found@test.com");
    }

    [Fact]
    public async Task GetById_MissingUser_Returns404()
    {
        using var db = CreateDb();
        var sut = new UsersController(db);

        var result = await sut.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        using var db = CreateDb();
        var sut = new UsersController(db);

        var result = await sut.Create(new CreateUserRequest("new@test.com", "Password1!", null, null, null));

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(201);
        db.Users.Should().ContainSingle(u => u.Email == "new@test.com");
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns409()
    {
        using var db = CreateDb();
        Seed(db, "taken@test.com");
        var sut = new UsersController(db);

        var result = await sut.Create(new CreateUserRequest("taken@test.com", "Password1!", null, null, null));

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_InvalidRole_Returns400()
    {
        using var db = CreateDb();
        var sut = new UsersController(db);

        var result = await sut.Create(new CreateUserRequest("x@test.com", "Password1!", null, "GodMode", null));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_MissingUser_Returns404()
    {
        using var db = CreateDb();
        var sut = new UsersController(db);

        var result = await sut.Update(Guid.NewGuid(), new UpdateUserRequest(null, null, null, null, null, null));

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_Email_PersistsChange()
    {
        using var db = CreateDb();
        var user = Seed(db, "old@test.com");
        var sut = new UsersController(db);

        await sut.Update(user.Id, new UpdateUserRequest(null, "new@test.com", null, null, null, null));

        db.Users.Find(user.Id)!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task Update_Phone_PersistsChange()
    {
        using var db = CreateDb();
        var user = Seed(db, "a@test.com");
        var sut = new UsersController(db);

        await sut.Update(user.Id, new UpdateUserRequest(null, null, "+12125550099", null, null, null));

        db.Users.Find(user.Id)!.Phone.Should().Be("+12125550099");
    }

    [Fact]
    public async Task Update_Role_PersistsChange()
    {
        using var db = CreateDb();
        var user = Seed(db, "a@test.com");
        var sut = new UsersController(db);

        await sut.Update(user.Id, new UpdateUserRequest(null, null, null, "Admin", null, null));

        db.Users.Find(user.Id)!.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Update_InvalidRole_Returns400()
    {
        using var db = CreateDb();
        var user = Seed(db, "a@test.com");
        var sut = new UsersController(db);

        var result = await sut.Update(user.Id, new UpdateUserRequest(null, null, null, "Overlord", null, null));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_TrustWeight_PersistsChange()
    {
        using var db = CreateDb();
        var user = Seed(db, "a@test.com");
        var sut = new UsersController(db);

        await sut.Update(user.Id, new UpdateUserRequest(null, null, null, null, null, 2.5));

        db.Users.Find(user.Id)!.TrustWeight.Should().Be(2.5);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingUser_RemovesAndReturns204()
    {
        using var db = CreateDb();
        var user = Seed(db, "gone@test.com");
        var sut = new UsersController(db);

        var result = await sut.Delete(user.Id);

        result.Should().BeOfType<NoContentResult>();
        db.Users.Find(user.Id).Should().BeNull();
    }

    [Fact]
    public async Task Delete_MissingUser_Returns404()
    {
        using var db = CreateDb();
        var sut = new UsersController(db);

        var result = await sut.Delete(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── Deactivate ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Deactivate_ActiveUser_SetsInactiveAndRevokesTokens()
    {
        using var db = CreateDb();
        var user = Seed(db, "active@test.com");
        db.RefreshTokens.Add(new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = "tok1", Revoked = false, ExpiresAt = DateTime.UtcNow.AddDays(7) });
        db.SaveChanges();
        var sut = new UsersController(db);

        var result = await sut.Deactivate(user.Id);

        result.Should().BeOfType<NoContentResult>();
        db.Users.Find(user.Id)!.IsActive.Should().BeFalse();
        db.RefreshTokens.Where(t => t.UserId == user.Id).All(t => t.Revoked).Should().BeTrue();
    }

    [Fact]
    public async Task Deactivate_MissingUser_Returns404()
    {
        using var db = CreateDb();
        var sut = new UsersController(db);

        var result = await sut.Deactivate(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── Reactivate ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Reactivate_InactiveUser_SetsActive()
    {
        using var db = CreateDb();
        var user = Seed(db, "inactive@test.com", isActive: false);
        var sut = new UsersController(db);

        var result = await sut.Reactivate(user.Id);

        result.Should().BeOfType<NoContentResult>();
        db.Users.Find(user.Id)!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Reactivate_MissingUser_Returns404()
    {
        using var db = CreateDb();
        var sut = new UsersController(db);

        var result = await sut.Reactivate(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── RevokeSessions ────────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeSessions_RevokesAllActiveTokens()
    {
        using var db = CreateDb();
        var user = Seed(db);
        db.RefreshTokens.AddRange(
            new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = "t1", Revoked = false, ExpiresAt = DateTime.UtcNow.AddDays(1) },
            new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = "t2", Revoked = false, ExpiresAt = DateTime.UtcNow.AddDays(1) });
        db.SaveChanges();
        var sut = new UsersController(db);

        var result = await sut.RevokeSessions(user.Id);

        result.Should().BeOfType<NoContentResult>();
        db.RefreshTokens.Where(t => t.UserId == user.Id).All(t => t.Revoked).Should().BeTrue();
    }

    [Fact]
    public async Task RevokeSessions_MissingUser_Returns404()
    {
        using var db = CreateDb();
        var sut = new UsersController(db);

        var result = await sut.RevokeSessions(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── ResetPassword ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_UpdatesPasswordHash()
    {
        using var db = CreateDb();
        var user = Seed(db);
        var sut = new UsersController(db);

        var result = await sut.ResetPassword(user.Id, new ResetPasswordRequest("NewPass123!"));

        result.Should().BeOfType<NoContentResult>();
        var updated = db.Users.Find(user.Id)!;
        BCrypt.Net.BCrypt.Verify("NewPass123!", updated.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_MissingUser_Returns404()
    {
        using var db = CreateDb();
        var sut = new UsersController(db);

        var result = await sut.ResetPassword(Guid.NewGuid(), new ResetPasswordRequest("x"));

        result.Should().BeOfType<NotFoundResult>();
    }
}
