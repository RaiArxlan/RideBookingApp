namespace IdentityService.Model;

public record RegisterModel(string FullName, string Email, string Password, string Address, string Role);
