namespace YourWear_backend.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public bool IsGoogle { get; set; }
    public virtual List<Order> Orders { get; set; }
    public virtual List<PublishedWear> PublishedWears { get; set; }
    public virtual List<RefreshToken> RefreshTokens { get; set; }
}