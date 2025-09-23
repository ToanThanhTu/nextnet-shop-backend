using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using net_backend.Data.Types;

namespace net_backend.Users
{
    /// <summary>
    /// Helper class for generating and managing JWT (JSON Web Token) authentication tokens.
    /// This class is responsible for creating secure tokens that authenticate users
    /// and authorize access to protected API endpoints.
    /// </summary>
    public class JwtTokenHelper
    {
        /// <summary>
        /// Generates a JWT authentication token for a given user.
        /// The token includes user claims (email and role) and expires after 12 hours.
        /// </summary>
        /// <param name="user">The authenticated user to generate a token for</param>
        /// <returns>A string representation of the JWT token</returns>
        /// <remarks>
        /// The generated token can be used by the frontend to authenticate API requests
        /// by including it in the Authorization header as "Bearer {token}".
        /// </remarks>
        public static string GenerateToken(User user)
        {
            // Create the symmetric security key used to sign the token
            // WARNING: This secret key should be moved to configuration/environment variables
            // and should be the same key used in ConfigureServices.cs for validation
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("my_super_secret_key_1234567890abcdef"));
            
            // Create signing credentials using HMAC SHA256 algorithm
            // This ensures the token can be verified and hasn't been tampered with
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Define the claims to include in the token
            // Claims are pieces of information about the user that can be extracted from the token
            var claims = new[]
            {
                // User's email address - used for identification
                new Claim(ClaimTypes.Email, user.Email),
                // User's role - used for authorization (e.g., "admin", "user")
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Create the JWT token with all necessary parameters
            var userToken = new JwtSecurityToken(
                issuer: "Trevor",                    // Who issued the token (should match validation config)
                audience: "Trevor",                  // Who the token is intended for (should match validation config)
                claims: claims,                      // User information included in the token
                expires: DateTime.Now.AddHours(12),  // Token expiration time (12 hours from now)
                signingCredentials: credentials      // Cryptographic signature for security
            );

            // Convert the token object to a string that can be transmitted to the client
            return new JwtSecurityTokenHandler().WriteToken(userToken);
        }
    }
}
