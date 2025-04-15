using Microsoft.AspNetCore.Identity;

namespace IdentityService.Model;
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public string Role { get; set; }
    public string Address { get; set; }
}
