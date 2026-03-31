using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PMO.API.DTOs;
using PMO.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PMO.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser>  _users;
    private readonly IConfiguration       _config;

    public AuthController(UserManager<AppUser> users, IConfiguration config)
    {
        _users  = users;
        _config = config;
    }

    // POST /api/auth/token/
    [HttpPost("token")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _users.FindByNameAsync(req.Username)
                ?? await _users.FindByEmailAsync(req.Username);

        if (user == null || !await _users.CheckPasswordAsync(user, req.Password))
            return Unauthorized(new { detail = "Invalid credentials" });

        if (!user.IsActive)
            return Unauthorized(new { detail = "?????? ??? ?????" });

        var tokens = GenerateTokens(user);
        return Ok(new TokenResponse(
            tokens.access, tokens.refresh,
            new UserDto(user.Id, user.UserName!, user.Email!,
                        user.FullNameAr, user.FullNameEn,
                        user.Department, user.JobTitle)));
    }

    // POST /api/auth/token/refresh/
    [HttpPost("token/refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest req)
    {
        try
        {
            var principal = ValidateRefreshToken(req.Refresh);
            var userId    = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user      = _users.FindByIdAsync(userId!).Result;
            if (user == null) return Unauthorized();

            var tokens = GenerateTokens(user);
            return Ok(new { access = tokens.access });
        }
        catch
        {
            return Unauthorized(new { detail = "Invalid refresh token" });
        }
    }

    // POST /api/auth/register/
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var user = new AppUser
        {
            UserName    = req.Username,
            Email       = req.Email,
            FullNameAr  = req.FullNameAr,
            FullNameEn  = req.FullNameEn,
            Department  = req.Department,
            JobTitle    = req.JobTitle,
        };

        var result = await _users.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        var tokens = GenerateTokens(user);
        return Created("", new TokenResponse(
            tokens.access, tokens.refresh,
            new UserDto(user.Id, user.UserName!, user.Email!,
                        user.FullNameAr, user.FullNameEn,
                        user.Department, user.JobTitle)));
    }

    // ?? Helpers ???????????????????????????????????????????
    private (string access, string refresh) GenerateTokens(AppUser user)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims  = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,            user.UserName!),
            new Claim(ClaimTypes.Email,           user.Email ?? ""),
        };

        var accessToken = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        var refreshToken = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(accessToken),
                new JwtSecurityTokenHandler().WriteToken(refreshToken));
    }

    private ClaimsPrincipal ValidateRefreshToken(string token)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = _config["Jwt:Issuer"],
            ValidAudience            = _config["Jwt:Audience"],
            IssuerSigningKey         = key,
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
    }
}
