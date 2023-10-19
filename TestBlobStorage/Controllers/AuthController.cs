using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TestBlobStorage.Data;
using TestBlobStorage.Models;
using TestBlobStorage.Models.Dtos;
using TestBlobStorage.Services;

namespace TestBlobStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly CosmosDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(CosmosDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Role,nameof(User))
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("JWT:Secret").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: creds,
                expires: DateTime.Now.AddHours(8)
             );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody]UserDto request)
        {
            try
            {
                User user = new()
                {
                    Name = request.Name,
                    Age = request.Age,
                    Username= request.Username,
                    Id = Guid.NewGuid(),
                    Surname = request.Surname,
                    ProfilePhoto = "Not set yet"
                };

                CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordSalt = passwordSalt;
                user.PasswordHash = passwordHash;

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.ToString());
                Console.WriteLine(ex.Message);
                throw;
            }
            
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Username == request.Username);

            if (user is null) return NotFound($"{request.Username} does not exist");

            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt)) return BadRequest("Password doesn't match Admin");

            var token = CreateToken(user);

            return Ok(token);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }

    }
}
