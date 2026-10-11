using Microsoft.AspNetCore.Identity;

namespace FleetFlow.Infrastructure.Authentication;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
}
