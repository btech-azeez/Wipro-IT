using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<(User? User, string? Error)> RegisterAsync(RegisterViewModel model);
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(int userId);
        Task<List<User>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ToggleUserStatusAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        public AuthService(AppDbContext context) => _context = context;

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<(User? User, string? Error)> RegisterAsync(RegisterViewModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return (null, "Username already exists.");

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return (null, "Email already registered.");

            var user = new User
            {
                Username     = model.Username,
                FullName     = model.FullName,
                Email        = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role         = WiproRoles.Employee,
                Department   = model.Department,
                IsActive     = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return (user, null);
        }

        public Task<User?> GetUserByIdAsync(int userId)
            => _context.Users.FindAsync(userId).AsTask()!;

        public async Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.RefreshToken       = refreshToken;
                user.RefreshTokenExpiry = expiry;
                await _context.SaveChangesAsync();
            }
        }

        public Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
            => _context.Users.FirstOrDefaultAsync(u =>
                u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        public async Task RevokeRefreshTokenAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.RefreshToken       = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();
            }
        }

        public Task<List<User>> GetAllUsersAsync()
            => _context.Users.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToListAsync();

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
