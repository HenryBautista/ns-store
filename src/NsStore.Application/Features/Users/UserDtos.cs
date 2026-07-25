using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Users;

public record UserDto(
    long Id,
    string Username,
    string FirstName,
    string LastName,
    string? MotherLastName,
    string FullName,
    UserRole Role,
    bool IsActive);

public record CreateUserRequest(
    string Username,
    string Password,
    string FirstName,
    string LastName,
    string? MotherLastName,
    UserRole? Role);

public record UpdateUserRequest(
    string Username,
    string FirstName,
    string LastName,
    string? MotherLastName,
    string? Password);

public record UpdateUserStatusRequest(bool IsActive);

public record UpdateUserRoleRequest(UserRole Role);

public static class UserMapping
{
    public static UserDto ToDto(this User user) => new(
        user.Id,
        user.Username,
        user.FirstName,
        user.LastName,
        user.MotherLastName,
        user.FullName,
        user.Role,
        user.IsActive);
}
