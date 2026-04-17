using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Digests;
using Registration.Persistence.entity;

namespace ResFunctions
{
    public class RegistrationFunctions
    {
        private readonly IConfiguration configuration;

        public RegistrationFunctions(IConfiguration Configuration)
        {
            configuration = Configuration;
        }
        public string GenerateSalt(int size = 32)
        {
            var saltBytes = new byte[size];
            RandomNumberGenerator.Fill(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        public string HashPasswordWithStribog(string password, string salt)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(salt)) throw new ArgumentNullException(nameof(salt));
            // Combine password and salt
            string saltedPassword = password + salt;

            // Convert the combined string into bytes
            byte[] passwordBytes = Encoding.UTF8.GetBytes(saltedPassword);

            // Here, you would call the Stribog hash function on the passwordBytes
            //  Stribog512 is a method that computes the hash
            byte[] hashBytes = Stribog512(passwordBytes);

            // Convert hash bytes to hex string for easier storage
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public static byte[] Stribog512(byte[] input)
        {
            // Create an instance of the Stribog (GOST 34.11-2012) 512-bit hash function
            Gost3411_2012_512Digest digest = new Gost3411_2012_512Digest();

            // Initialize and process the input
            digest.BlockUpdate(input, 0, input.Length);

            // Create a byte array to hold the hash result
            byte[] result = new byte[digest.GetDigestSize()];

            // Compute the hash
            digest.DoFinal(result, 0);

            return result;
        }
        public string GenerateJwtToken(RegistrationUser user)
        {
            try
            {
                var secretKey = configuration["JwtSettings:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new Exception("JWT Secret Key is missing or empty in configuration.");
                }

                Console.WriteLine($"Secret Key: {secretKey}");

                var key = Encoding.ASCII.GetBytes(secretKey);

                if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email) || user.role == null)
                {
                    throw new Exception("User claims are missing. Please check the user's data.");
                }

                Console.WriteLine($"Generating JWT for user: {user.UserName}, Email: {user.Email}, Role: {user.role}");

                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor //acts like a blueprint for creating the token
                {
                    Subject = new ClaimsIdentity(new[]
                    {

                      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                      new Claim(ClaimTypes.Name, user.UserName),
                      new Claim(ClaimTypes.Email, user.Email),
                      new Claim(ClaimTypes.Role, user.role.Name)
                    }),
                    Expires = DateTime.UtcNow.AddHours(2), // Set token expiration to 2 hours
                    Issuer = "InformationSecurity",
                    Audience = "InformationSecurityApi",
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                Console.WriteLine("JWT token generated successfully.");

                return tokenHandler.WriteToken(token); // Return the generated token
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating JWT token: {ex.Message}");
                throw;
            }
        }


    }
}