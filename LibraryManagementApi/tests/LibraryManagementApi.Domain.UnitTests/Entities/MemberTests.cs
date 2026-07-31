using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Domain.UnitTests.Entities;

public class MemberTests
{
    private static Member CreateActiveMember() =>
        Member.Create("MEM-12345678", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), userId: null);

    [Fact]
    public void Create_DefaultsToActiveStatusAndSetsJoinDate()
    {
        var before = DateTime.UtcNow;
        var member = CreateActiveMember();
        var after = DateTime.UtcNow;

        Assert.Equal(MembershipStatus.Active, member.Status);
        Assert.InRange(member.JoinDateUtc, before, after);
    }

    [Fact]
    public void Update_ChangesProfileFields()
    {
        var member = CreateActiveMember();
        var newBranchId = Guid.NewGuid();

        member.Update("Jane R. Doe", "jane.r.doe@example.com", "555-0100", "456 New Address", newBranchId);

        Assert.Equal("Jane R. Doe", member.FullName);
        Assert.Equal("jane.r.doe@example.com", member.Email);
        Assert.Equal("555-0100", member.Phone);
        Assert.Equal("456 New Address", member.Address);
        Assert.Equal(newBranchId, member.HomeBranchId);
    }

    [Fact]
    public void Suspend_FromActive_SetsStatusToSuspended()
    {
        var member = CreateActiveMember();

        member.Suspend();

        Assert.Equal(MembershipStatus.Suspended, member.Status);
    }

    [Fact]
    public void Reactivate_FromSuspended_SetsStatusToActive()
    {
        var member = CreateActiveMember();
        member.Suspend();

        member.Reactivate();

        Assert.Equal(MembershipStatus.Active, member.Status);
    }

    [Fact]
    public void Deactivate_SetsStatusToDeactivated()
    {
        var member = CreateActiveMember();

        member.Deactivate();

        Assert.Equal(MembershipStatus.Deactivated, member.Status);
    }

    [Fact]
    public void Suspend_OnDeactivatedMembership_ThrowsDomainException()
    {
        var member = CreateActiveMember();
        member.Deactivate();

        Assert.Throws<DomainException>(member.Suspend);
    }

    [Fact]
    public void Reactivate_OnDeactivatedMembership_ThrowsDomainException()
    {
        var member = CreateActiveMember();
        member.Deactivate();

        Assert.Throws<DomainException>(member.Reactivate);
    }
}
