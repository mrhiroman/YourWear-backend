namespace YourWear_backend.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string TokenValue { get; set; }
    public virtual User User { get; set; }
    public DateTime Created { get; set; }
    public DateTime Expires { get; set; }
}