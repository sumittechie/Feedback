using Api.Configurations;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api.Mangers
{
    public class AuthenticateManager
    {

        public Tokens GetValidRefreshToken(string token, Users identityUser)
        {
            if (identityUser == null)
            {
                return null;
            }

            var existingToken = identityUser.Tokens.FirstOrDefault(x => x.Token == token);
            return IsRefreshTokenValid(existingToken) ? existingToken : null;
        }
        

        public async Task<Users> ValidateUser(LoginCredentialVm credentials, UserManager<Users> userManager)
        {
            var identityUser = await userManager.FindByEmailAsync(credentials.Email);
            if (identityUser != null)
            {
                var result = userManager.PasswordHasher.VerifyHashedPassword(identityUser, identityUser.PasswordHash, credentials.Password);
                return result == PasswordVerificationResult.Failed ? null : identityUser;
            }

            return null;
        }



        public string GenerateAccessToken(Users identityUser, JWTBearerTokenSettings jwtBearerTokenSettings)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtBearerTokenSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, identityUser.UserName.ToString()),
                    new Claim(ClaimTypes.Email, identityUser.Email),
                    new Claim(ClaimTypes.Role, identityUser.IsAdmin ? "Admin": "User")
                }),
                Expires = DateTime.Now.AddSeconds(jwtBearerTokenSettings.ExpiryTimeInSeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = jwtBearerTokenSettings.Audience,
                Issuer = jwtBearerTokenSettings.Issuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public Tokens GenerateRefreshToken(string ipAddress, string userId, int refreshTokenExpiryInDays)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new Tokens
                {
                    Token = Convert.ToBase64String(randomBytes),
                    ExpiryOn = DateTime.UtcNow.AddDays(refreshTokenExpiryInDays),
                    CreatedOn = DateTime.UtcNow,
                    CreatedByIp = ipAddress,
                    UserId = userId
                };
            }
        }

        public bool IsRefreshTokenValid(Tokens existingToken)
        {
            // Is token already revoked, then return false
            if (existingToken.RevokedByIp != null && existingToken.RevokedOn != DateTime.MinValue)
            {
                return false;
            }

            // Token already expired, then return false
            if (existingToken.ExpiryOn <= DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }
    }
}
