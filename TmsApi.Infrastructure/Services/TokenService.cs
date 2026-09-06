using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TmsApi.Infrastructure.Identity;

namespace TmsApi.Infrastructure.Services;
public class TokenService
{
private readonly IConfiguration _config;
public TokenService(IConfiguration config)
{
_config = config;
}
public string GenerateJwt(TmsUser user, IList<string> roles)
{
var claims = new List<Claim>
{
new Claim(ClaimTypes.NameIdentifier, user.Id),
new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
new Claim("FirstName", user.FirstName)
};
foreach (var role in roles)
{
claims.Add(new Claim(ClaimTypes.Role, role));
}
var key = new
SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
var creds = new SigningCredentials(key,
SecurityAlgorithms.HmacSha256);
var token = new JwtSecurityToken(
issuer: _config["Jwt:Issuer"],
audience: _config["Jwt:Audience"],
claims: claims,
expires:
DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"]!)),
signingCredentials: creds
);
return new JwtSecurityTokenHandler().WriteToken(token);
}
}

internal class JwtSecurityTokenHandler
{
    public JwtSecurityTokenHandler()
    {
    }

    internal string WriteToken(JwtSecurityToken token)
    {
        throw new NotImplementedException();
    }
}

internal class JwtSecurityToken
{
    private string? issuer;
    private string? audience;
    private List<Claim> claims;
    private DateTime expires;
    private SigningCredentials signingCredentials;

    public JwtSecurityToken(string? issuer, string? audience, List<Claim> claims, DateTime expires, SigningCredentials signingCredentials)
    {
        this.issuer = issuer;
        this.audience = audience;
        this.claims = claims;
        this.expires = expires;
        this.signingCredentials = signingCredentials;
    }
}