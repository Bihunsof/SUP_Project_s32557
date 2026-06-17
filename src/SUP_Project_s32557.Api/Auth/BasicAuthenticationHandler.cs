using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SUP_Project_s32557.Api.Data;

namespace SUP_Project_s32557.Api.Auth;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AppDbContext _db;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        AppDbContext db) : base(options, logger, encoder) => _db = db;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization header");

        try
        {
            var header = AuthenticationHeaderValue.Parse(Request.Headers.Authorization!);
            if (!"Basic".Equals(header.Scheme, StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("Invalid scheme");

            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter ?? "")).Split(':', 2);
            if (credentials.Length != 2) return AuthenticateResult.Fail("Invalid credentials");

            var login = credentials[0];
            var passwordHash = PasswordHasher.Hash(credentials[1]);
            var employee = await _db.Employees.SingleOrDefaultAsync(x => x.Login == login && x.PasswordHash == passwordHash);
            if (employee is null) return AuthenticateResult.Fail("Invalid login or password");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, employee.Login),
                new Claim(ClaimTypes.Role, employee.Role.ToString())
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization header");
        }
    }
}
