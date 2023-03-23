namespace RestaurantManagementSystem.Models.OutputModels
{
    public class RegistrationLoginResponse
    {
        public Guid userId { get; set; }
        public string email { get; set; } = string.Empty;
        public string firstName = string.Empty;
        public string lastName = string.Empty;
        public string token { get; set; } = string.Empty;

        public RegistrationLoginResponse() { }

        public RegistrationLoginResponse(Guid userId, string email, string firstName, string lastName, string token)
        {
            this.userId = userId;
            this.email = email;
            this.firstName = firstName;
            this.lastName = lastName;
            this.token = token;
        }
    }
}
