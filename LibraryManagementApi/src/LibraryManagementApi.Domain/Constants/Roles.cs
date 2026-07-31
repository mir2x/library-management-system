namespace LibraryManagementApi.Domain.Constants;

public static class Roles
{
    public const string Admin = nameof(Admin);
    public const string Librarian = nameof(Librarian);
    public const string Member = nameof(Member);

    public static readonly IReadOnlyList<string> All = [Admin, Librarian, Member];
}
