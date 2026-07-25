using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

/// <summary>
/// Single table with a <see cref="ClientType"/> discriminator: an individual or a company.
/// For a company, <see cref="Name"/> holds the legal/business name.
/// </summary>
public class Client : AuditableEntity
{
    public ClientType Type { get; set; }
    public string Name { get; set; } = null!;
    public string? LastName { get; set; }
    public string? MotherLastName { get; set; }

    /// <summary>National ID (Carnet de Identidad) — individuals.</summary>
    public string? Ci { get; set; }

    /// <summary>Tax ID — both client types.</summary>
    public string? Nit { get; set; }

    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ContactName { get; set; }

    public string FullName => Type == ClientType.Company
        ? Name
        : string.Join(' ', new[] { Name, LastName, MotherLastName }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
}
