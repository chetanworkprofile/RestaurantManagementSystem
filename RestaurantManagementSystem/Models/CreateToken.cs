namespace RestaurantManagementSystem.Models
{
    public class CreateToken
    {
        public Guid userId { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
    }
}
