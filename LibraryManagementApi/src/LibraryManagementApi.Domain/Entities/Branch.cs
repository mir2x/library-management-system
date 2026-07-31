using LibraryManagementApi.Domain.Common;

namespace LibraryManagementApi.Domain.Entities;

public class Branch : BaseAuditableEntity
{
    private Branch()
    {
    }

    private Branch(string name, string address, string? contactNumber, string? email)
    {
        Name = name;
        Address = address;
        ContactNumber = contactNumber;
        Email = email;
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public string? ContactNumber { get; private set; }

    public string? Email { get; private set; }

    public bool IsActive { get; private set; }

    public static Branch Create(string name, string address, string? contactNumber, string? email) =>
        new(name, address, contactNumber, email);

    public void Update(string name, string address, string? contactNumber, string? email)
    {
        Name = name;
        Address = address;
        ContactNumber = contactNumber;
        Email = email;
    }

    public void Deactivate() => IsActive = false;
}
