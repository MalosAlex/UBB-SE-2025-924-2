using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLayer.Models;
using BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BusinessLayer.Models.Login;
using Microsoft.AspNetCore.Authorization;

namespace SteamWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ISessionService sessionService;
        private readonly IConfiguration configuration;

        public AuthController(IUserService userService, ISessionService sessionService, IConfiguration configuration)
        {
            this.userService = userService;
            this.sessionService = sessionService;
            this.configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Authenticate the user
            var user = userService.Login(request.EmailOrUsername, request.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            // Get the session details from the singleton
            var sessionId = UserSession.Instance.CurrentSessionId.Value;

            // Create UserWithSessionDetails
            var userWithSessionDetails = new UserWithSessionDetails
            {
                SessionId = sessionId,
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Developer = user.IsDeveloper,
                UserCreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                CreatedAt = UserSession.Instance.CreatedAt,
                ExpiresAt = UserSession.Instance.ExpiresAt
            };

            // Generate JWT token
            var token = GenerateJwtToken(user, sessionId);

            // Return the login response with token and session details
            return Ok(new LoginResponse
            {
                User = user,
                Token = token,
                UserWithSessionDetails = userWithSessionDetails
            });
        }

        private string GenerateJwtToken(User user, Guid sessionId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration["Jwt:Key"] ?? "YourTemporarySecretKeyHereMustBe32Chars"));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username),
                new Claim("sessionId", sessionId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"] ?? "SteamWebApi",
                audience: configuration["Jwt:Audience"] ?? "SteamProfile",
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}