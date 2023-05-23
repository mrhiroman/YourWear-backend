using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using YourWear_backend.Entities;
using YourWear_backend.Models;

namespace YourWear_backend.Controllers;

[ApiController]
[Route("/api/login")]
public class UserController: Controller
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHashingService _passwordHashingService;

    public UserController(AppDbContext dbContext, PasswordHashingService passwordHashingService)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
    }
 
    [HttpPost]
    public async Task<IActionResult> Token([FromBody] LoginUserModel model)
    {
        var identity = await GetIdentity(model.Email, model.Password); 
        if (identity == null) 
        { 
            return BadRequest(new { errorText = "Invalid username or password." }); 
        }
        
        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER, 
            audience: AuthOptions.AUDIENCE, 
            notBefore: now, 
            claims: identity.Claims, 
            expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)), 
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
 
        var response = new 
        {
            access_token = encodedJwt, 
            username = identity.Name
        };
 
        return Json(response);
    }
 
    private async Task<ClaimsIdentity> GetIdentity(string email, string password)
    {
        var hashedPassword = _passwordHashingService.GetSHA256(password);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email && x.Password == hashedPassword); 
        if (user != null)
        {
            var claims = new List<Claim> 
            {
                new Claim(ClaimTypes.Email, user.Email), 
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            }; 
            ClaimsIdentity claimsIdentity = 
                new ClaimsIdentity(claims, "Token", ClaimTypes.Name, 
                    ClaimTypes.Role);
            return claimsIdentity;
        }

        return null;
    }
    
    private async Task<ClaimsIdentity> GetIdentity(User user)
    {
        if (user != null)
        {
            var claims = new List<Claim> 
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email), 
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            }; 
            ClaimsIdentity claimsIdentity = 
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, 
                    ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }

        return null;
    }

    [HttpPost("/api/register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserModel model)
    {
        if (ValidateRegister(model))
        {
            var hashedPassword = _passwordHashingService.GetSHA256(model.Password);
            await _dbContext.Users.AddAsync(new User
            {
                Email = model.Email,
                Name = model.Name,
                Password = hashedPassword,
                Role = "user"
            });
        }

        await _dbContext.SaveChangesAsync();
        return Json(new { model.Name, model.Password });
    }

    private bool ValidateRegister(RegisterUserModel model)
    {
        return (model.Email != String.Empty && model.Email.Length > 5 &&
                model.Name != String.Empty && model.Name.Length > 7 &&
                model.Password != string.Empty && model.Password.Length >= 8);
    }

    [Authorize]
    [HttpGet("/api/refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
        if (user != null)
        {
            var identity = await GetIdentity(user);
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER, 
                audience: AuthOptions.AUDIENCE, 
                notBefore: now, 
                claims: identity.Claims, 
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)), 
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
 
            var response = new 
            {
                access_token = encodedJwt, 
                username = identity.Name
            };
            return Json(response);
        }
        
        return BadRequest("Failed to Identify a User.");
    }
    
}