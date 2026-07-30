namespace NsStore.Application.Features.Branches;

public record BranchDto(
    long Id,
    string Code,
    string Name,
    string? Address,
    string? Phone,
    bool IsActive);

public record BranchRequest(
    string Code,
    string Name,
    string? Address,
    string? Phone);

public record UpdateBranchStatusRequest(bool IsActive);
