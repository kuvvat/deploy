using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Core.Models;
using Infrastructure.Context;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationContext context, ITechnicalLogger technicalLogger) : base(context, technicalLogger)
        {

        }

        public Task<User> GetByEmailAsync(string email)
        {
            _technicalLogger.Information("Get user by Email", new KeyValuePair<string, object>(nameof(email), email));

            return DbSet.AsNoTracking().FirstOrDefaultAsync(u => string.Equals(u.Email, email));
        }

        public Task<bool> IsExistAsync(string phoneNumber, string email)
        {
            _technicalLogger.Information("Check if user exist by phone number or email",
                new KeyValuePair<string, object>(nameof(phoneNumber), phoneNumber),
                new KeyValuePair<string, object>(nameof(email), email));

            return DbSet.AsNoTracking().AnyAsync(item => string.Equals(item.PhoneNumber, phoneNumber) || string.Equals(item.Email, email));
        }
    }
}
