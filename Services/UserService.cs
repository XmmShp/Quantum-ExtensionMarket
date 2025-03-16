using ExtensionMarket.Data;
using ExtensionMarket.Interfaces;
using ExtensionMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExtensionMarket.Services;

public class UserService(ApplicationDbContext context, IAuditLogService auditLogService, IPasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await context.Users.ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await context.Users
            .Include(u => u.CreatedExtensions)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(string username, string email, string password, List<UserRole> roles)
    {
        // Check if user with the same email or username already exists
        if (await context.Users.AnyAsync(u => u.Email == email))
        {
            throw new InvalidOperationException("Email is already in use.");
        }

        if (await context.Users.AnyAsync(u => u.Username == username))
        {
            throw new InvalidOperationException("Username is already taken.");
        }

        // Create user object
        var user = new User
        {
            Username = username,
            Email = email,
            Roles = roles
        };

        // Hash the password using IPasswordHasher
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        // Set creation date
        user.CreatedDate = DateTime.UtcNow;
        user.LastLogin = DateTime.UtcNow;

        // Add user to database
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Log user creation
        await auditLogService.CreateAuditLogAsync(
            "UserCreated",
            user.Id,
            $"User {user.Username} created with roles {string.Join(", ", user.Roles.Select(r => r.ToString()))}."
        );

        return user;
    }

    public async Task<User> UpdateUserAsync(Guid id, string username, string email)
    {
        var existingUser = await context.Users.FindAsync(id);
        if (existingUser == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        // Check if email is being changed and if it's already in use
        if (existingUser.Email != email &&
            await context.Users.AnyAsync(u => u.Email == email && u.Id != id))
        {
            throw new InvalidOperationException("Email is already in use.");
        }

        // Check if username is being changed and if it's already taken
        if (existingUser.Username != username &&
            await context.Users.AnyAsync(u => u.Username == username && u.Id != id))
        {
            throw new InvalidOperationException("Username is already taken.");
        }

        // Update user properties
        existingUser.Username = username;
        existingUser.Email = email;

        // Save changes
        context.Users.Update(existingUser);
        await context.SaveChangesAsync();

        // Log user update
        await auditLogService.CreateAuditLogAsync(
            "UserUpdated",
            existingUser.Id,
            $"User {existingUser.Username} updated."
        );

        return existingUser;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        // Log user deletion before actually deleting
        var username = user.Username;

        // Delete user
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        // Log user deletion
        await auditLogService.CreateAuditLogAsync(
            "UserDeleted",
            Guid.Empty, // System action since user is deleted
            $"User {username} with ID {id} deleted."
        );

        return true;
    }

    public async Task<bool> AddUserRoleAsync(Guid id, UserRole role)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        if (user.Roles.Contains(role))
        {
            return true; // Role already exists, no need to add
        }

        user.Roles.Add(role);
        await context.SaveChangesAsync();

        // Log role addition
        await auditLogService.CreateAuditLogAsync(
            "UserRoleAdded",
            user.Id,
            $"Role {role} added to user {user.Username}."
        );

        return true;
    }

    public async Task<bool> RemoveUserRoleAsync(Guid id, UserRole role)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        if (!user.Roles.Contains(role))
        {
            return true; // Role doesn't exist, no need to remove
        }

        user.Roles.Remove(role);
        await context.SaveChangesAsync();

        // Log role removal
        await auditLogService.CreateAuditLogAsync(
            "UserRoleRemoved",
            user.Id,
            $"Role {role} removed from user {user.Username}."
        );

        return true;
    }

    public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return false;
        }

        // Verify password using IPasswordHasher
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return false;
        }

        // Update last login time
        user.LastLogin = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Log successful login
        await auditLogService.CreateAuditLogAsync(
            "UserLogin",
            user.Id,
            $"User {user.Username} logged in."
        );

        return true;
    }
}