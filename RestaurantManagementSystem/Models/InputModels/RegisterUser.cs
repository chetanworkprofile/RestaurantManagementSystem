namespace RestaurantManagementSystem.Models.InputModels
{
    public class RegisterUser
    {
        public string firstName { get; set; } = "Firstname";
        public string lastName { get; set; } = "Lastname";
        public string email { get; set; } = "email@chatapp.com";
        public long phone { get; set; } = 9999999999;
        public string password { get; set; } = "fgh@98gh!#cf$5";
        public string address { get; set; } = string.Empty;

        public string pathToProfilePic { get; set; } = string.Empty;
    }
}

//input model while user registration