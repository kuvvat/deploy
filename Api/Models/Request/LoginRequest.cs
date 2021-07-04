namespace Api.Models.Request
{
    /// <summary>
    /// Login request
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string? Password { get; set; }
    }
}
