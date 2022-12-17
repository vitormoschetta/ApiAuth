using ApiAuth.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiAuth.Filters
{
    /// <summary>
    /// Filtro de autorização.
    /// Esse filtro não é necessário, pois o próprio framework com a biblioteca Microsoft.AspNetCore.Authentication.JwtBearer já faz a validação do token.
    /// Adicionamos ele apenas para enriquecer o contexto da requisição com informações do usuário.
    /// </summary>
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserRepository _userRepository;

        public AuthorizationFilter(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Verifica se a rota é pública (AllowAnonymous). Neste caso, não precisamos enriquecer o contexto.
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymousAttributes = endpoint?.Metadata.GetOrderedMetadata<AllowAnonymousAttribute>() ?? Array.Empty<AllowAnonymousAttribute>();
            if (allowAnonymousAttributes.Any())
            {
                await Task.CompletedTask;
                return;
            }

            // No controller já conseguimos identificar o usuário que está fazendo a requisição, através do User.Identity.Name.
            // Porém, podemos também consultar a base de dados e enriquecer as informações do usuário no contexto da requisição. Ex:
            var userName = context.HttpContext.User.Identity?.Name ?? throw new Exception("User.Identity is null");
            var user = await _userRepository.GetByUsername(userName);
            context.HttpContext.Items.Add("user", user);

            await Task.CompletedTask;
        }
    }
}