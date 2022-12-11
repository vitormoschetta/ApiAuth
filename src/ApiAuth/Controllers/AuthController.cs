using ApiAuth.Data.Repositories;
using ApiAuth.Requests;
using ApiAuth.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
using Shared.Services;
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

        public AuthController(IJwtService jwtServices, IUserRepository userRepository)
        {
            _jwtServices = jwtServices;
            _userRepository = userRepository;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userRepository.GetByUsername(request.Username);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var passwordHash = HashManager.GenerateHash(request.Password, user.Salt);

            if (user.Password != passwordHash)
                return BadRequest(new { message = "Username or password is incorrect" });

            var token = await _jwtServices.GenerateJwtToken(user.Username, user.Role, user.Email);

            return Ok(new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                Email = user.Email,
                Token = token
            });
        }
    }
}