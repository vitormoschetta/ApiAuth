using System.Security.Claims;
using ApiAuth.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiAuth.Filters
{
    public class AuthFilter : IAsyncAuthorizationFilter
    {
        private readonly UserRepository _userRepository;

        public AuthFilter(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // verifica se a rota é pública (AllowAnonymous) e não precisa validar o token
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymousAttributes = endpoint?.Metadata.GetOrderedMetadata<AllowAnonymousAttribute>() ?? Array.Empty<AllowAnonymousAttribute>();
            if (allowAnonymousAttributes.Any())
            {
                await Task.CompletedTask;
                return;
            }

            // se chegou aqui, a rota não é pública, então precisa validar o token (se o usuário está autenticado)
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // verificar se o usuário tem permissão para acessar a rota
            endpoint = context.HttpContext.GetEndpoint();
            var authorizeAttributes = endpoint?.Metadata.GetOrderedMetadata<AuthorizeAttribute>() ?? Array.Empty<AuthorizeAttribute>();

            if (authorizeAttributes.Any())
            {
                var roles = authorizeAttributes.SelectMany(x => x.Roles?.Split(",").Select(y => y.Trim()) ?? Array.Empty<string>()).ToList();
                if (roles.Any())
                {
                    var userRoles = context.HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
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