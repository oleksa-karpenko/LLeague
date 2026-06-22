namespace LLeague.Api.Domain;

public class AdminUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";   // bcrypt
}
