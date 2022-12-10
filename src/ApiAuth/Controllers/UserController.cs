using ApiAuth.Data.Repositories;
using ApiAuth.Models;
using ApiAuth.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Utils;

namespace ApiAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            return Ok(await _userRepository.Get());
        }


        [HttpGet("{name}")]
        [Authorize(Roles = "user")]
        public async Task<ActionResult<User>> GetByName(string name)
        {
            var user = await _userRepository.GetByUsername(name);
            if (user == null)
                return NotFound();

            return Ok(user);
        }


        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            var username = User.Identity?.Name ?? throw new Exception("User.Identity.Name is null");
            var user = await _userRepository.GetByUsername(username);
            if (user == null)
                return NotFound();

            return Ok(user);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Create([FromBody] CreateUserRequest request)
        {
            var userExists = await _userRepository.GetByUsername(request.Username);
            if (userExists != null)
                return BadRequest("Username already exists");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = request.Role
            };

            user.Password = HashManager.GenerateHash(request.Password, user.Salt);

            await _userRepository.Create(user);

            return Ok(user);
        }
    }
}