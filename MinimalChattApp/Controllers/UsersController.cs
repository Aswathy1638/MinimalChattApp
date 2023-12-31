﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using MinimalChattApp.Data;
using MinimalChattApp.Model;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Plugins;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace MinimalChattApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public UsersController(ChatDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
          if (_context.User == null)
          {
              return NotFound();
          }
            return await _context.User.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
          if (_context.User == null)
          {
              return NotFound();
          }
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        
        [HttpPost("/api/register")]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var email = _context.User.Any(u => u.Email == user.Email);

            if (email)
            {
                return Conflict(new { message = "Email exist" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid Credential" });
            }


            var HashedPassword = PasswordHash(user.Password);

            user.Password = HashedPassword;
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }
        [HttpPost("/api/login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            // Check if the request is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid credentials" });
            }

            // Find the user by email
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            // Check if the user exists
            if (user == null)
            {
                return Unauthorized(new { error = "Invalid email or password" });
            }

            // Verify the provided password against the hashed password
            if (BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
            {
                // Passwords match, user is authenticated

                // Generate JWT token
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    token,
                    profile = new
                    {
                        user.Id,
                        user.Name,
                        user.Email
                    }
                });
            }
            else
            {
                // Passwords do not match, login failed
                return Unauthorized(new { error = "Invalid email or password" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            string keyString = "ASD123ASRJHIOUICVGTUJIOLKL-JHGJHJHK-HJHGHGK";
           byte[] key = Encoding.ASCII.GetBytes(keyString);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            // Add any other claims you want to include in the token
        }),
                Expires = DateTime.UtcNow.AddHours(1), // Set the token expiration time (e.g., 1 hour)
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        [HttpGet("api/users")]
        [Authorize] // Require authentication to access this endpoint
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Get the user ID of the authenticated user making the request
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Retrieve the list of users from the database, excluding the current user
            var users = await _context.User.Where(u => u.Id != userId).ToListAsync();

            // Return the list of users in the response body
            return Ok(users.Select(u => new
            {
                id = u.Id,
                name = u.Name,
                email = u.Email
            }));
        }
        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.User == null)
            {
                return NotFound();
            }
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.User?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        public static string PasswordHash(string password)
        {
            // Generate a random salt and hash the password using bcrypt
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hashedPassword;
        }

    }
}
