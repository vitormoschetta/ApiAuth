using System.Security.Claims;
using ApiAuth.Data.Repositories;
using ApiAuth.Models;
using ApiAuth.Requests;
using ApiAuth.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Interfaces;
using Shared.Settings;
using Shared.Utils;

namespace ApiAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtServices;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IOptions<AppSettings> _appSettings;

        public AuthController(IJwtService jwtServices, IUserRepository userRepository, IEmailService emailService, IOptions<AppSettings> appSettings)
        {
            _jwtServices = jwtServices;
            _userRepository = userRepository;
            _emailService = emailService;
            _appSettings = appSettings;
        }


        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register([FromBody] CreateUserRequest request)
        {
            var userExists = await _userRepository.GetByUsername(request.Username);
            if (userExists != null)
                return BadRequest("Username already exists");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = request.Role,
                RefreshToken = await _jwtServices.GenerateRefreshToken()
            };

            user.Password = HashManager.GenerateHash(request.Password, user.Salt);

            await _userRepository.Create(user);

            if (_appSettings.Value.SmtpConfig.Enabled)
            {
                var body = $@"
                    <h1>Welcome to our platform</h1>
                    <p>Username: {user.Username}</p>
                    <p>Email: {user.Email}</p>

                    <p>Click <a href='{_appSettings.Value.BaseAddress}/api/Auth/EmailVerification/{user.Id}'>here</a> to validate your email</p>";

                await _emailService.SendEmail(user.Email, "Welcome", body);
            }

            return Ok(user);
        }


        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userRepository.GetByUsername(request.Username);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var passwordHash = HashManager.GenerateHash(request.Password, user.Salt);

            if (user.Password != passwordHash)
                return BadRequest(new { message = "Username or password is incorrect" });

            var claims = GetClaims(user);
            var token = await _jwtServices.GenerateJwtToken(claims);

            return Ok(new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                Email = user.Email,
                RefreshToken = user.RefreshToken,
                Token = token
            });
        }


        [HttpPost("RefreshToken")]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var principal = await _jwtServices.GetPrincipalFromExpiredToken(request.Token);
            var username = principal.Identity?.Name ?? throw new SecurityTokenException("Invalid token");

            var user = await _userRepository.GetByUsername(username);
            if (user == null)
                return Unauthorized(new { message = "Invalid token: user not found" });

            if (user.RefreshToken != request.RefreshToken)
                return Unauthorized(new { message = "Invalid refresh token" });

            var claims = GetClaims(user);
            var token = await _jwtServices.GenerateJwtToken(claims);

            var refreshToken = await _jwtServices.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            await _userRepository.Update(user);

            return Ok(new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                Email = user.Email,
                Token = token,
                RefreshToken = refreshToken
            });
        }


        [HttpGet("EmailVerification/{id}")]
        public async Task<ContentResult> Confirm(Guid id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = $"<div style='text-align:center; margin-top:3em; color:#808080'><h1>Usuario nao ncontrado</h1></div>"
                };

            user.IsEmailVerified = true;
            await _userRepository.Update(user);

            return new ContentResult
            {
                ContentType = "text/html",
                Content = $"<div style='text-align:center; margin-top:3em; color:#808080'><h1>Email verificado com sucesso</h1></div>"
            };
        }


        private Claim[] GetClaims(User user)
        {
            return new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("iss", _appSettings.Value.JwtConfig.Issuer),
                new Claim("aud", _appSettings.Value.JwtConfig.Audience),
                new Claim("document", "123456789") // custom claim,
            };
        }
    }
}