using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Core;
using Core.Models;
using Infrastructure.Interfaces;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITechnicalLogger _technicalLogger;

        public UserService(IUserRepository userRepository, ITechnicalLogger technicalLogger)
        {
            _userRepository = userRepository;
            _technicalLogger = technicalLogger;
        }

        public async Task<User> AddWithReturnAsync(User user)
        {
            CreatePasswordHash(user.Password, out var passwordHash, out var passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            var createdUser = await _userRepository.AddWithReturnAsync(user);

            _technicalLogger.Information("User created", new KeyValuePair<string, object>(nameof(user.Id), user.Id));

            return createdUser;
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _technicalLogger.Warning("User not found", new KeyValuePair<string, object>(nameof(email), email));
                return null;
            }

            return VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt) ? user : null;
        }

        public async Task DecrementCreditAsync(int id)
        {
            var dbUser = await _userRepository.GetByIdAsync(id);
            dbUser.Credits -= 1;

            await _userRepository.UpdateAsync(dbUser);
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            var result = await _userRepository.GetByIdAsync(userId);
            return result;
        }

        public Task<bool> IsExistAsync(string phoneNumber, string email)
        {
            _technicalLogger.Information("Checking if user exist", new KeyValuePair<string, object>(nameof(email), email));

            return _userRepository.IsExistAsync(phoneNumber, email);
        }

        #region Helpers

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
            }
            if (storedHash.Length != 64)
            {
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", nameof(storedHash));
            }
            if (storedSalt.Length != 128)
            {
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", nameof(storedHash));
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (var i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                    {
                        _technicalLogger.Warning("Passwords didn't not match");
                        return false;
                    }
                }
            }

            _technicalLogger.Information("Passwords are a match");

            return true;
        }

        #endregion
    }
}
