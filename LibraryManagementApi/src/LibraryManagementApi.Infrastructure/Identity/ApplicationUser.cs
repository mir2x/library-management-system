using Microsoft.AspNetCore.Identity;

namespace LibraryManagementApi.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public required string FullName { get; set; }
}
