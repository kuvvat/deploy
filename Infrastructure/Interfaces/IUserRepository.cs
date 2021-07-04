using System.Threading.Tasks;
using Core.Models;

namespace Infrastructure.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);

        Task<bool> IsExistAsync(string phoneNumber, string email);
    }
}
