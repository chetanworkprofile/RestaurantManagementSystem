namespace RestaurantManagementSystem.Models.InputModels
{
    public class UpdateUser
    {
            public string firstName { get; set; } = string.Empty;
            public string lastName { get; set; } = string.Empty;
            public long phone { get; set; } = -1;
            public string address { get; set; } = string.Empty;
            public string pathToProfilePic { get; set; } = string.Empty;
    }
}
