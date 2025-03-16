using ExtensionMarket.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExtensionMarket.Attributes;

public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params UserRole[] roles)
        => Roles = string.Join(',', roles.Select(r => Convert.ToInt32(r).ToString()).ToArray());
}
