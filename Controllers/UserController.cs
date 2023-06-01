using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private readonly HttpClient _httpClient;

    public UserController(AppDbContext dbContext, PasswordHashingService passwordHashingService, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
        _httpClient = httpClient;
    }
 
    [HttpPost]
    [ProducesResponseType(typeof(AuthRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Token([FromBody] LoginUserModel model)
    {
        var identity = await GetIdentity(model.Email, model.Password); 
        if (identity == null || model.Password == "") 
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

        var refreshToken = await GetRefreshToken(await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email));
        
        var response = new AuthRequestResponse
        {
            AccessToken = encodedJwt,
            RefreshToken = refreshToken.TokenValue,
            Username = identity.Name
        };
 
        return Json(response);
    }
 
    private async Task<ClaimsIdentity> GetIdentity(string email, string password)
    {
        var hashedPassword = _passwordHashingService.GetSHA256(password);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email && x.Password == hashedPassword); 
        if (user != null)
        {
            var tokenClaims = new List<Claim> 
            {
                new Claim(ClaimTypes.Email, user.Email), 
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            ClaimsIdentity tokenClaimsIdentity =
                new ClaimsIdentity(tokenClaims, "Token", ClaimTypes.Name,
                    ClaimTypes.Role);

            return tokenClaimsIdentity;
        }

        return null;
    }

    private async Task<RefreshToken> GetRefreshToken(User user)
    {
        if (user != null)
        {
            var token = new RefreshToken
            {
                User = user,
                Created = DateTime.Now,
                Expires = DateTime.Now.AddDays(7),
                TokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            };
            _dbContext.RefreshTokens.Add(token);
            await _dbContext.SaveChangesAsync();
            return token;
        }

        return null;
    }
    private async Task<ClaimsIdentity> GetIdentity(User user)
    {
        if (user != null)
        {
            var tokenClaims = new List<Claim> 
            {
                new Claim(ClaimTypes.Email, user.Email), 
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };
            ClaimsIdentity claimsIdentity = 
                new ClaimsIdentity(tokenClaims, "Token", ClaimTypes.Name, 
                    ClaimTypes.Role);
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
                Role = "user",
                IsGoogle = false
            });
        }
        else
        {
            return BadRequest("Data cannot be validated.");
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
    
    [HttpPost("/api/refresh")]
    [ProducesResponseType(typeof(AuthRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] string token)
    {
        var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenValue == token);
        if (refreshToken == null || refreshToken.Expires < DateTime.Now)
        {
            return BadRequest("Token is not valid.");
        }
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == refreshToken.User.Email);
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

            refreshToken.Expires = DateTime.Now.Subtract(TimeSpan.FromMinutes(10));
            _dbContext.RefreshTokens.Update(refreshToken);
            await _dbContext.SaveChangesAsync();

            refreshToken = await GetRefreshToken(user);

            var response = new AuthRequestResponse
            {
                AccessToken = encodedJwt, 
                RefreshToken = refreshToken.TokenValue,
                Username = user.Name
            };
            return Json(response);
        }
        
        return BadRequest("User associated with this token not found.");
    }

    [Authorize]
    [HttpGet("/api/info")]
    [ProducesResponseType(typeof(UserInfoModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserInfo()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Name == User.Identity.Name);
        if (user != null)
        {
            return Json(new UserInfoModel
            {
                Name = user.Name,
                Email = user.Email,
                PublishedWears = user.PublishedWears.Select(pw => new WearModel
                {
                    Name = pw.Name,
                    Category = new CategoryModel {Name = pw.Category.Name},
                    ImageUrl = pw.ImageUrl,
                    Id = pw.Id,
                    CreatorName = user.Name,
                    CreatorId = user.Id
                }).ToList()
            });
        }
        
        return BadRequest("Failed to Identify a User.");
    }

    [HttpPost("/api/google_login")]
    [ProducesResponseType(typeof(AuthRequestResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GoogleLogin([FromBody] string token)
    {
        var googleUser = await GetCredentialsFromGoogle(token);
        if (googleUser != null)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == googleUser.Email && u.IsGoogle == true);
            if (user == null)
            {
                await _dbContext.Users.AddAsync(new User
                {
                    Email = googleUser.Email,
                    Name = googleUser.Username,
                    Password = _passwordHashingService.GetSHA256(googleUser.Username),
                    Role = "user",
                    IsGoogle = true,
                });
                await _dbContext.SaveChangesAsync();
                user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == googleUser.Email && u.IsGoogle == true);
            }
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
            await _dbContext.SaveChangesAsync();

            var refreshToken = await GetRefreshToken(user);

            var response = new AuthRequestResponse 
            {
                AccessToken = encodedJwt, 
                RefreshToken = refreshToken.TokenValue,
                Username = user.Name
            };
            return Json(response);
            
        }

        return BadRequest("Invalid google token");
    }

    private async Task<Credentials> GetCredentialsFromGoogle(string token)
    {
        var url = $"https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", "Bearer " + token);
        var uploadResponse = await _httpClient.SendAsync(request);

        if (uploadResponse.IsSuccessStatusCode)
        {
            var respData = await uploadResponse.Content.ReadAsStringAsync();
            var json = System.Text.Json.JsonDocument.Parse(respData);
            var email = json.RootElement.GetProperty("email").ToString();
            var name = json.RootElement.GetProperty("name").ToString();
            await _dbContext.SaveChangesAsync();
            return new Credentials(name, email);
        }

        return null;
    }

    record Credentials(string Username, string Email);

}