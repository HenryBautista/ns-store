using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Clients;

public class ClientService(IAppDbContext db, TimeProvider clock)
{
    public async Task<PagedResult<ClientDto>> ListAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.Clients.AsNoTracking().AsQueryable();
        if (request.TrimmedSearch is { } search)
        {
            var pattern = $"%{search.ToLower()}%";
            query = query.Where(c =>
                EF.Functions.Like(c.Name.ToLower(), pattern) ||
                (c.LastName != null && EF.Functions.Like(c.LastName.ToLower(), pattern)) ||
                (c.MotherLastName != null && EF.Functions.Like(c.MotherLastName.ToLower(), pattern)) ||
                (c.Ci != null && EF.Functions.Like(c.Ci.ToLower(), pattern)) ||
                (c.Nit != null && EF.Functions.Like(c.Nit.ToLower(), pattern)));
        }

        var page = await query
            .OrderBy(c => c.Name)
            .ThenBy(c => c.LastName)
            .ToPagedResultAsync(request, cancellationToken);

        return new PagedResult<ClientDto>(page.Items.Select(ToDto).ToList(), page.Page, page.PageSize, page.Total);
    }

    public async Task<ClientDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        ToDto(await FindAsync(id, cancellationToken));

    public async Task<ClientDto> CreateAsync(ClientRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCiAvailableAsync(request, null, cancellationToken);

        var client = new Client();
        Apply(client, request);

        db.Clients.Add(client);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(client);
    }

    public async Task<ClientDto> UpdateAsync(long id, ClientRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCiAvailableAsync(request, id, cancellationToken);

        var client = await FindAsync(id, cancellationToken);
        Apply(client, request);

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(client);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var client = await FindAsync(id, cancellationToken);
        client.DeletedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Client> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(Client), id);

    /// <summary>
    /// The CI identifies a person, so it may not repeat. Companies never store one, and the soft-delete
    /// query filter means a deleted client stops reserving its CI.
    /// </summary>
    private async Task EnsureCiAvailableAsync(ClientRequest request, long? excludeId, CancellationToken cancellationToken)
    {
        if (request.Type != ClientType.Individual || string.IsNullOrWhiteSpace(request.Ci))
        {
            return;
        }

        var ci = request.Ci.Trim().ToLower();
        var taken = await db.Clients
            .AnyAsync(c => c.Ci != null && c.Ci.ToLower() == ci && (excludeId == null || c.Id != excludeId), cancellationToken);

        if (taken)
        {
            throw new ConflictException(ErrorCodes.DuplicateCi, $"CI '{request.Ci.Trim()}' is already registered");
        }
    }

    /// <summary>Identity fields are exclusive per type, so the row stays coherent; the address is shared.</summary>
    private static void Apply(Client client, ClientRequest request)
    {
        client.Type = request.Type;
        client.Name = request.Name.Trim();
        client.Nit = request.Nit?.Trim();
        client.Phone = request.Phone?.Trim();
        client.Email = request.Email?.Trim();
        client.City = request.City?.Trim();
        client.Address = request.Address?.Trim();

        if (request.Type == ClientType.Individual)
        {
            client.LastName = request.LastName?.Trim();
            client.MotherLastName = request.MotherLastName?.Trim();
            client.Ci = request.Ci?.Trim();
            client.ContactName = null;
        }
        else
        {
            client.LastName = null;
            client.MotherLastName = null;
            client.Ci = null;
            client.ContactName = request.ContactName?.Trim();
        }
    }

    internal static ClientDto ToDto(Client c) => new(
        c.Id,
        c.Type,
        c.Name,
        c.LastName,
        c.MotherLastName,
        c.FullName,
        c.Ci,
        c.Nit,
        c.Phone,
        c.Email,
        c.City,
        c.Address,
        c.ContactName);
}
