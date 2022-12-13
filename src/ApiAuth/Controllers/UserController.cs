using ApiAuth.Data.Repositories;
using ApiAuth.Models;
using ApiAuth.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Interfaces;
using Shared.Settings;
using Shared.Utils;

namespace ApiAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IOptions<AppSettings> _appSettings;

        public UserController(IUserRepository userRepository, IEmailService emailService, IOptions<AppSettings> appSettings)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _appSettings = appSettings;
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
            // Como enriquecemos o contexto da requisição com o usuário, no AuthorizationFilter, podemos recuperar o usuário de duas formas:
            // 1) Através do User.Identity.Name
            // 2) Através do HttpContext.Items (Informações adicionais que podem ser adicionadas ao contexto da requisição ao utilizar um filtro de autorização)

            // 1) Através do User.Identity.Name
            var user = await GetCurrentUserByUserIdentity();

            // 2) Através do HttpContext.Items
            // var user = await GetCurrentUserByHttpContext();        

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


        private Task<User?> GetCurrentUserByHttpContext()
        {
            var user = HttpContext.Items["user"] as User ?? throw new Exception("User not found in HttpContext.Items");
            return Task.FromResult(user) as Task<User?>;
        }


        private async Task<User?> GetCurrentUserByUserIdentity()
        {
            var username = User.Identity?.Name ?? throw new Exception("User.Identity.Name is null");
            var user = await _userRepository.GetByUsername(username);
            return user;
        }
    }
}