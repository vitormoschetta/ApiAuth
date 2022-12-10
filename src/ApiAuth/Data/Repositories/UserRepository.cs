using ApiAuth.Models;

namespace ApiAuth.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private List<User> _users = new List<User>();

        public Task Create(User user)
        {                    
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<User>> Get()
        {
            return Task.FromResult(_users.AsEnumerable());
        }

        public Task<User?> GetById(Guid id)
        {
            return Task.FromResult(_users.SingleOrDefault(x => x.Id == id));
        }

        public Task<User?> GetByUsername(string username)
        {
            return Task.FromResult(_users.SingleOrDefault(x => x.Username == username));
        }
    }
}