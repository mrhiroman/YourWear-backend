using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace YourWear_backend;

public class AuthOptions
{
    public const string ISSUER = "YourWearServer"; 
    public const string AUDIENCE = "YourWearClient"; 
    const string KEY = "mysupersecret_secretkey!123";   // get from environment
    public const int LIFETIME = 15; 
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}