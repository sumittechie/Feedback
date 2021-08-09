using Api.Configurations;
using Api.Mangers;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("Auth")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly JWTBearerTokenSettings _jwtBearerTokenSettings;
        private readonly UserManager<Users> _userManager;
        private readonly FeedbackDbContext _dbContext;
        private readonly AuthenticateManager _manager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticateController(IOptions<JWTBearerTokenSettings> jwtTokenOtions, UserManager<Users> userManager, FeedbackDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _jwtBearerTokenSettings = jwtTokenOtions.Value;
            _userManager = userManager;
            _dbContext = context;
            _manager = new AuthenticateManager();
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserDetailsVm userDetails)
        {
            if (!ModelState.IsValid || userDetails == null)
            {
                return new BadRequestObjectResult(new { Message = "User Registration Failed" });
            }

            var identityUser = new Users()
            {
                UserName = userDetails.Email,
                Email = userDetails.Email,
                Name = userDetails.Name,
                PhoneNumber = userDetails.Mobile,
                IsAdmin = false
            };
            var result = await _userManager.CreateAsync(identityUser, userDetails.Password);
            if (!result.Succeeded)
            {
                var dictionary = new ModelStateDictionary();
                foreach (IdentityError error in result.Errors)
                {
                    dictionary.AddModelError(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "User Registration Failed", Errors = dictionary });
            }

            return Ok(new { Message = "User Reigstration Successful" });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginCredentialVm credentials)
        {
            Users identityUser;

            if (!ModelState.IsValid
                || credentials == null
                || (identityUser = await _manager.ValidateUser(credentials, _userManager)) == null)
            {
                return Ok(new ApiResponse { Error = true, Message = "Invalid email or password" });
            }

            var token = GenerateTokens(identityUser);

            var data = new
            {
                Token = token,
                role = identityUser.IsAdmin ? "admin" : "user",
            };

            return Ok(new ApiResponse { Error = false, Data = data });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RefreshToken")]
        public IActionResult RefreshToken()
        {
           
            var token = HttpContext.Request.Cookies["refreshToken"];
            var identityUser = _dbContext.Users.Include(x => x.Tokens)
                .FirstOrDefault(x => x.Tokens.Any(y => y.Token == token && y.UserId == x.Id));

            // Get existing refresh token if it is valid and revoke it
            var existingRefreshToken = _manager.GetValidRefreshToken(token, identityUser);
            if (existingRefreshToken == null)
            {
                return new BadRequestObjectResult(new { Message = "Failed" });
            }

            existingRefreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress.ToString();
            existingRefreshToken.RevokedOn = DateTime.UtcNow;

            // Generate new tokens
            var newToken = GenerateTokens(identityUser);
            return Ok(new { Token = newToken, Message = "Success" });
        }

        [HttpPost]
        [Route("RevokeToken")]
        public IActionResult RevokeToken(string token)
        {
            // If user found, then revoke
            if (RevokeRefreshToken(token))
            {
                return Ok(new { Message = "Success" });
            }

            // Otherwise, return error
            return new BadRequestObjectResult(new { Message = "Failed" });
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            // Revoke Refresh Token 
            RevokeRefreshToken();
            return Ok(new { Token = "", Message = "Logged Out" });
        }

        private bool RevokeRefreshToken(string token = null)
        {
            token = token == null ? HttpContext.Request.Cookies["refreshToken"] : token;
            var identityUser = _dbContext.Users.Include(x => x.Tokens)
                .FirstOrDefault(x => x.Tokens.Any(y => y.Token == token && y.UserId == x.Id));
            if (identityUser == null)
            {
                return false;
            }

            // Revoke Refresh token
            var existingToken = identityUser.Tokens.FirstOrDefault(x => x.Token == token);
            existingToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress.ToString();
            existingToken.RevokedOn = DateTime.UtcNow;
            _dbContext.Update(identityUser);
            _dbContext.SaveChanges();
            return true;
        }

        private string GenerateTokens(Users identityUser)
        {
            // Generate access token
            string accessToken = _manager.GenerateAccessToken(identityUser, _jwtBearerTokenSettings);

            // Generate refresh token and set it to cookie
            var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            var refreshToken = _manager.GenerateRefreshToken(ipAddress, identityUser.Id, _jwtBearerTokenSettings.RefreshTokenExpiryInDays);

            // Set Refresh Token Cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
            //Response.Cookies.Append();

            // Save refresh token to database
            if (identityUser.Tokens == null)
            {
                identityUser.Tokens = new List<Tokens>();
            }

            identityUser.Tokens.Add(refreshToken);
            _dbContext.Update(identityUser);
            _dbContext.SaveChanges();
            return accessToken;
        }

    }
}
