namespace Api.Models.Request
{
    /// <summary>
    /// Register Model
    /// </summary>
    public class RegisterRequest
    {
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
        /// Password
        /// </summary>
        public string? Password { get; set; }
    }
}
