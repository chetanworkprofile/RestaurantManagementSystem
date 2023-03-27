namespace RestaurantManagementSystem.Models.OutputModels
{
    public class ResponseUser
    {
        public Guid userId { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public long phone { get; set; }
        public string userRole { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string pathToProfilePic { get; set; } = string.Empty;
        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        //public bool isBlocked { get; set; }

        public ResponseUser() { }
        public ResponseUser(Guid userId, string firstName, string lastName, string email, long phone, string userRole, string address,string pathToProfilePic, DateTime createdAt, DateTime updatedaAt)
        {
            this.userId = userId;
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.phone = phone;
            this.userRole = userRole;
            this.address = address;
            this.pathToProfilePic = pathToProfilePic;
            this.createdAt = createdAt;
            this.updatedAt = updatedAt;
            //this.isBlocked = isBlocked;
        }
    }
}
