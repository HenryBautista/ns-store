using NsStore.Application.Features.Users;

namespace NsStore.Application.Features.Auth;

public record LoginRequest(string Username, string Password);

/// <summary>
/// The refresh token is returned separately so the API can put it in an httpOnly cookie —
/// it is never part of the JSON body.
/// </summary>
public record AuthResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    UserDto User,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

public record LoginResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAt, UserDto User);
