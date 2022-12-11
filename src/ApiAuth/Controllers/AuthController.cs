using ApiAuth.Data.Repositories;
using ApiAuth.Requests;
using ApiAuth.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
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

        [HttpPost("Login")]
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
    }
}