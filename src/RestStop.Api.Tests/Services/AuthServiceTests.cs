namespace RestStop.Api.Tests.Services;

/// <summary>Unit tests for <see cref="AuthService"/>.</summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IOtpService> _otp = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IEmailSender> _email = new();
    private readonly Mock<ISmsSender> _sms = new();

    private AuthService CreateSut() =>
        new(_users.Object, _otp.Object, _tokens.Object, _email.Object, _sms.Object);

    /// <summary>
    /// SendOtpAsync should route to the email sender when the recipient is an email address.
    /// </summary>
    [Fact]
    public async Task SendOtpAsync_EmailRecipient_SendsViaEmail()
    {
        _otp.Setup(o => o.GenerateAsync("user@example.com")).ReturnsAsync("123456");

        await CreateSut().SendOtpAsync("user@example.com");

        _email.Verify(e => e.SendOtpAsync("user@example.com", "123456"), Times.Once);
        _sms.Verify(s => s.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// SendOtpAsync should route to the SMS sender when the recipient starts with '+'.
    /// </summary>
    [Fact]
    public async Task SendOtpAsync_PhoneRecipient_SendsViaSms()
    {
        _otp.Setup(o => o.GenerateAsync("+12125551234")).ReturnsAsync("654321");

        await CreateSut().SendOtpAsync("+12125551234");

        _sms.Verify(s => s.SendOtpAsync("+12125551234", "654321"), Times.Once);
        _email.Verify(e => e.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// VerifyOtpAsync should throw UnauthorizedAccessException when the code is invalid.
    /// </summary>
    [Fact]
    public async Task VerifyOtpAsync_InvalidCode_ThrowsUnauthorized()
    {
        _otp.Setup(o => o.ValidateAsync("user@example.com", "000000")).ReturnsAsync(false);

        var sut = CreateSut();
        var act = () => sut.VerifyOtpAsync(new VerifyCodeRequest("user@example.com", "000000"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// VerifyOtpAsync should create a new user record when no matching user exists.
    /// </summary>
    [Fact]
    public async Task VerifyOtpAsync_NewUser_CreatesUserRecord()
    {
        var newUser = new User { Email = "new@example.com" };
        _otp.Setup(o => o.ValidateAsync("new@example.com", "123456")).ReturnsAsync(true);
        _users.Setup(u => u.GetByEmailAsync("new@example.com")).ReturnsAsync((User?)null);
        _users.Setup(u => u.CreateAsync(It.IsAny<User>())).ReturnsAsync(newUser);
        _tokens.Setup(t => t.GenerateAccessToken(newUser)).Returns("access-token");
        _tokens.Setup(t => t.GenerateRefreshTokenAsync(newUser.Id)).ReturnsAsync("refresh-token");

        var result = await CreateSut().VerifyOtpAsync(
            new VerifyCodeRequest("new@example.com", "123456"));

        result.AccessToken.Should().Be("access-token");
        _users.Verify(u => u.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    /// <summary>
    /// LoginAsync should throw UnauthorizedAccessException when the password does not match.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };

        _users.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        var sut = CreateSut();
        var act = () => sut.LoginAsync(new LoginRequest("test@example.com", "wrongpassword"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// RegisterAsync should throw InvalidOperationException when the email is already registered.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        _users.Setup(u => u.GetByEmailAsync("taken@example.com"))
            .ReturnsAsync(new User { Email = "taken@example.com" });

        var sut = CreateSut();
        var act = () => sut.RegisterAsync(
            new RegisterRequest("username", "taken@example.com", "Password123!"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}
