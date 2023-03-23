namespace RestaurantManagementSystem.Models.OutputModels
{
    public class ResponseUser
    {
        public Guid userId { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public long phone { get; set; }
        public string address { get; set; } = string.Empty;
        public DateTime dateOfBirth { get; set; }
        public string pathToProfilePic { get; set; } = string.Empty;
        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
    }
}
