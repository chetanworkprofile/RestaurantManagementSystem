namespace RestaurantManagementSystem.Models
{
    public class User
    {
        public Guid userId { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public long phone { get; set; } = -1;
        public string userRole { get; set; } = string.Empty;    //user,chef,admin
        public string address { get; set; } = string.Empty;
        public byte[] passwordHash { get; set; } = new byte[32];
        public string pathToProfilePic { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string token { get; set; } = string.Empty;
        public int? verificationOTP { get; set; }
        public DateTime otpUsableTill { get; set; }
        public DateTime? verifiedAt { get; set; }
        public bool isDeleted { get; set; }
        //public bool isBlocked { get; set; } = false;

        public User() { }
        public User(Guid userId,string firstName, string lastName,string email,long phone, string userRole,string address, byte[] passwordHash,string pathToProfilePic,string token)
        {
            this.userId = userId;
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.phone = phone;
            this.userRole = userRole;
            this.address = address;
            this.passwordHash = passwordHash;
            this.pathToProfilePic = pathToProfilePic;
            createdAt = DateTime.Now;
            updatedAt = DateTime.Now;
            this.token = token;
            verificationOTP = 999;
            otpUsableTill = DateTime.Now;
            verifiedAt = DateTime.Now;
            isDeleted = false;
            //isBlocked= false;
        }
    }
}
