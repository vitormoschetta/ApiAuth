using ApiAuth.Models;

namespace ApiAuth.Data.Repositories
{
    public interface IUserRepository
    {
        Task Create(User user);
        Task<IEnumerable<User>> Get();
        Task<User?> GetById(Guid id);
        Task<User?> GetByUsername(string username);
    }
}