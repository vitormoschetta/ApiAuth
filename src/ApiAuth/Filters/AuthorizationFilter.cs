using System.Security.Claims;
using ApiAuth.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiAuth.Filters
{
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserRepository _userRepository;

        public AuthorizationFilter(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Verifica se a rota é pública (AllowAnonymous). Neste caso, não precisa validar o token.
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymousAttributes = endpoint?.Metadata.GetOrderedMetadata<AllowAnonymousAttribute>() ?? Array.Empty<AllowAnonymousAttribute>();
            if (allowAnonymousAttributes.Any())
            {
                await Task.CompletedTask;
                return;
            }

            // Se a rota não for pública, verifica se o usuário está autenticado.
            var contextUser = context.HttpContext.User;
            if (contextUser.Identity == null || !contextUser.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Verifica se o usuário tem permissão para acessar a rota.
            var authorizeAttributes = endpoint?.Metadata.GetOrderedMetadata<AuthorizeAttribute>() ?? Array.Empty<AuthorizeAttribute>();
            if (authorizeAttributes.Any())
            {
                var roles = authorizeAttributes.SelectMany(x => x.Roles?.Split(",").Select(y => y.Trim()) ?? Array.Empty<string>()).ToList();
                if (roles.Any())
                {
                    var userRoles = contextUser.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
                    if (!userRoles.Any(x => roles.Contains(x)))
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
            }

            // No controller já conseguimos identificar o usuário que está fazendo a requisição, através do User.Identity.Name.
            // Porém, podemos também consultar a base de dados e enriquecer as informações do usuário no contexto da requisição. Ex:
            var user = await _userRepository.GetByUsername(contextUser.Identity?.Name ?? throw new Exception("User.Identity.Name is null"));

            if (user != null)
            {
                context.HttpContext.Items.Add("user", user);
            }          

            await Task.CompletedTask;
        }
    }
}