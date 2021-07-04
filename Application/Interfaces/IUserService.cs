using System.Threading.Tasks;
using Core.Models;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<User> AddWithReturnAsync(User user);

        Task<User> AuthenticateAsync(string username, string password);

        Task DecrementCreditAsync(int id);

        Task<User> GetByIdAsync(int userId);

        Task<bool> IsExistAsync(string phoneNumber, string email);
    }
}