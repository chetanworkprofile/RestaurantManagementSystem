namespace RestaurantManagementSystem.Models.OutputModels
{
    public class ActiveUsers
    {
        public Guid userId { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public long phone { get; set; } = -1;
        public string userRole { get; set; } = string.Empty;    //user,chef,admin
        public string address { get; set; } = string.Empty;
        public string pathToProfilePic { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public bool isBlocked { get; set; } = false;
        public bool isActive { get; set; } = false;

        public ActiveUsers() { }
        public ActiveUsers(User user)
        {
            this.userId = user.userId;
            this.firstName = user.firstName;
            this.lastName = user.lastName;
            this.email = user.email;
            this.phone = user.phone;
            this.userRole = user.userRole;
            this.address = user.address;
            this.pathToProfilePic = user.pathToProfilePic;
            this.createdAt = user.createdAt;
            this.isBlocked = user.isBlocked;
            isActive = false;
        }
    }
}
