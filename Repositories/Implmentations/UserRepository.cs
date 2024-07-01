using Master.API.Models;
using Master.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Master.API.Repositories.Implmentations
{
    public class UserRepository : IUserRepository
    {
        private readonly MasterDBContext _context;

        public UserRepository(MasterDBContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == username);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
