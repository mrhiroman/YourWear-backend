namespace YourWear_backend.Models;

public class AuthRequestResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string Username { get; set; }
}