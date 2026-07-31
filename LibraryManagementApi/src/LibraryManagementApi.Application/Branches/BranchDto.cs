namespace LibraryManagementApi.Application.Branches;

public record BranchDto(Guid Id, string Name, string Address, string? ContactNumber, string? Email, bool IsActive);
