namespace LibraryManagementApi.Application.Books;

public record BookInventoryDto(Guid BranchId, string BranchName, int TotalCopies, int AvailableCopies);
