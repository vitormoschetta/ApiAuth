using System.Security.Claims;
using ApiAuth.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiAuth.Filters
{
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly UserRepository _userRepository;

        public AuthorizationFilter(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Verifica se a rota é pública (AllowAnonymous). Neste caso, não precisa validar o token.
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymousAttributes = endpoint?.Metadata.GetOrderedMetadata<AllowAnonymousAttribute>();
            if (allowAnonymousAttributes != null)
            {
                await Task.CompletedTask;
                return;
            }

            // Se a rota não for pública, verifica se o usuário está autenticado.
            var user = context.HttpContext.User;
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Verifica se o usuário tem permissão para acessar a rota.
            var authorizeAttributes = endpoint?.Metadata.GetOrderedMetadata<AuthorizeAttribute>();
            if (authorizeAttributes != null)
            {
                var roles = authorizeAttributes.SelectMany(x => x.Roles?.Split(",").Select(y => y.Trim()) ?? Array.Empty<string>()).ToList();
                if (roles.Any())
                {
                    var userRoles = user.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
                    if (!userRoles.Any(x => roles.Contains(x)))
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
            }

            // aqui poderíamos fazer outras validações, como por exemplo, se o usuário está ativo ou não
            // poderíamos também enriquecer o usuário com outras informações, como por exemplo, o perfil dele e passar para o controller. Ex: context.HttpContext.Items.Add("user", user)            

            await Task.CompletedTask;
        }
    }
}