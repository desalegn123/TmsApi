using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TmsDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TmsDbContext context,
        TokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;
    }

    // ============================================================
    // POST /api/auth/login
    // ============================================================

    [EnableRateLimiting("AuthLimiter")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(423, new
            {
                detail = "Account locked due to multiple failed login attempts."
            });
        }

        var validPassword = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);

        // Generate access token
        var accessToken = _tokenService.GenerateJwt(
            user,
            roles);

        // Create initial refresh token
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token
        });
    }

    // ============================================================
    // POST /api/auth/refresh
    // ============================================================

    public record RefreshRequest(string RefreshToken);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid refresh token."
            });
        }

        // ========================================================
        // Token theft detection
        // ========================================================

        if (storedToken.IsUsed)
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            return Unauthorized(new
            {
                detail =
                    "Token theft detected. All user sessions revoked."
            });
        }

        // ========================================================
        // Check expiration / revocation
        // ========================================================

        if (storedToken.IsRevoked ||
            storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail = "Refresh token expired or revoked."
            });
        }

        // ========================================================
        // Mark old refresh token as used
        // ========================================================

        storedToken.IsUsed = true;

        // ========================================================
        // Create new refresh token
        // ========================================================

        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);

        await _context.SaveChangesAsync();

        // ========================================================
        // Generate new access token
        // ========================================================

        var user = await _userManager.FindByIdAsync(
            storedToken.UserId);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "User not found."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken = _tokenService.GenerateJwt(
            user,
            roles);

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }
}