using System;

namespace Api.Models.Response
{
    /// <summary>
    /// Login response
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Credits
        /// </summary>
        public int Credits { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Token expiration
        /// </summary>
        public DateTime TokenExpiration { get; set; }
    }
}
