using ExtensionMarket.Models;

namespace ExtensionMarket.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(string username, string email, string password, List<UserRole> roles);
    Task<User> UpdateUserAsync(Guid id, string username, string email);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> AddUserRoleAsync(Guid id, UserRole role);
    Task<bool> RemoveUserRoleAsync(Guid id, UserRole role);
    Task<bool> ValidateUserCredentialsAsync(string email, string password);
}