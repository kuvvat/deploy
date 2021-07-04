using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Configuration;
using Api.Helpers;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Api.Models.Request;
using Api.Models.Response;
using Core;
using Core.Models;

namespace Api.Controllers
{
    /// <summary>
    /// User
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppSettings? _appSettings;
        private readonly ITechnicalLogger _logger;
        private readonly string? _ipAddress;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userService">User service</param>
        /// <param name="appSettings">App settings</param>
        /// <param name="httpContextAccessor">HTTP context</param>
        /// <param name="logger">Logger</param>
        public UserController(IUserService userService, IOptions<AppSettings> appSettings, IHttpContextAccessor httpContextAccessor, ITechnicalLogger logger)
        {
            if (appSettings == null)
            {
                throw new Exception("The system could not find the appSettings");
            }

            _userService = userService;
            _appSettings = appSettings.Value;
            _logger = logger;
            _ipAddress= httpContextAccessor.CurrentIpAddress();
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="registerRequest">Register request Model</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterRequest? registerRequest)
        {
            if (registerRequest == null)
            {
                return BadRequest(new { message = "The input object cannot be null or empty" });
            }

            if (string.IsNullOrWhiteSpace(registerRequest.FirstName) || string.IsNullOrWhiteSpace(registerRequest.LastName))
            {
                return BadRequest(new { message = "First name and last name are mandatory" });
            }

            if (string.IsNullOrWhiteSpace(registerRequest.Email))
            {
                return BadRequest(new { message = "Email cannot be null or empty" });
            }

            if (string.IsNullOrWhiteSpace(registerRequest.PhoneNumber))
            {
                return BadRequest(new { message = "Phone number cannot be null or empty" });
            }

            if (string.IsNullOrWhiteSpace(registerRequest.Password))
            {
                return BadRequest(new { message = "Password cannot be null or empty" });
            }

            var isExisting = await _userService.IsExistAsync(registerRequest.PhoneNumber, registerRequest.Email);
            if (isExisting)
            {
                _logger.Warning("User already exist",
                    new KeyValuePair<string, object>(nameof(registerRequest.Email), registerRequest.Email),
                    new KeyValuePair<string, object>(nameof(registerRequest.PhoneNumber), registerRequest.PhoneNumber),
                    new KeyValuePair<string, object>("ip-address", _ipAddress ?? string.Empty));

                return BadRequest(new { message = "A user with the same phone number or email already exist" });
            }

            var user = new User
            {
                Guid = Guid.NewGuid(),
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.PhoneNumber,
                Password = registerRequest.Password,
                RegistrationDate = DateTime.Now
            };

            var createdUser = await _userService.AddWithReturnAsync(user);
            var token = TokenManager.GenerateToken(_appSettings?.Secret ?? string.Empty, createdUser.Id);

            _logger.Information("User registered",
                new KeyValuePair<string, object>(nameof(registerRequest.Email), registerRequest.Email),
                new KeyValuePair<string, object>(nameof(registerRequest.FirstName), registerRequest.FirstName),
                new KeyValuePair<string, object>(nameof(registerRequest.LastName), registerRequest.LastName),
                new KeyValuePair<string, object>("ip-address", _ipAddress ?? string.Empty));

            var userResponse = GetUserResponse(createdUser, token);
            return Ok(userResponse);
        }

        /// <summary>
        /// Authenticate a user
        /// </summary>
        /// <param name="loginRequest">Login request Model</param>
        [AllowAnonymous]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest? loginRequest)
        {
            if (loginRequest == null)
            {
                return BadRequest(new { message = "The input object cannot be null or empty" });
            }

            if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest(new { message = "Email and password are mandatory" });
            }

            var user = await _userService.AuthenticateAsync(loginRequest.Email, loginRequest.Password);
            if (user == null)
            {
                _logger.Warning("Combination of email and password is incorrect",
                    new KeyValuePair<string, object>(nameof(loginRequest.Email), loginRequest.Email),
                    new KeyValuePair<string, object>("ip-address", _ipAddress ?? string.Empty));

                return BadRequest(new { message = "The combination of email and password is incorrect" });
            }

            var token = TokenManager.GenerateToken(_appSettings?.Secret ?? string.Empty, user.Id);

            _logger.Information("User logged-in",
                new KeyValuePair<string, object>(nameof(loginRequest.Email), loginRequest.Email),
                new KeyValuePair<string, object>("ip-address", _ipAddress ?? string.Empty));

            var userResponse = GetUserResponse(user, token);
            return Ok(userResponse);
        }

        private static UserResponse GetUserResponse(User user, string token)
        {
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Token = token,
                TokenExpiration = DateTime.Now.AddMonths(6)
            };
        }
    }
}