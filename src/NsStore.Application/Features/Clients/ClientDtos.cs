using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Clients;

public record ClientDto(
    long Id,
    ClientType Type,
    string Name,
    string? LastName,
    string? MotherLastName,
    string FullName,
    string? Ci,
    string? Nit,
    string? Phone,
    string? Email,
    string? City,
    string? Address,
    string? ContactName);

public record ClientRequest(
    ClientType Type,
    string Name,
    string? LastName,
    string? MotherLastName,
    string? Ci,
    string? Nit,
    string? Phone,
    string? Email,
    string? City,
    string? Address,
    string? ContactName);
