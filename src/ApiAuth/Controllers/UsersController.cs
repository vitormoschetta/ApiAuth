using ApiAuth.Data.Repositories;
using ApiAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
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


        private Task<User?> GetCurrentUserByHttpContext()
        {
            var user = HttpContext.Items["user"] as User ?? throw new Exception("User not found in HttpContext.Items");
            return Task.FromResult(user) as Task<User?>;
        }


        private async Task<User?> GetCurrentUserByUserIdentity()
        {
            var username = User.Identity?.Name ?? throw new Exception("User.Identity.Name is null");

            // pegar valor de um claim específico
            var document = User.Claims.FirstOrDefault(x => x.Type == "document")?.Value;
            if (document != null)
                Console.WriteLine($"Document: {document}");

            return await _userRepository.GetByUsername(username);
        }
    }
}