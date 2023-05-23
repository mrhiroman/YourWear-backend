using YourWear_backend.Entities;

namespace YourWear_backend.Models;

public class UserInfoModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public  List<WearModel> PublishedWears { get; set; }
}