namespace RestaurantManagementSystem.Models.InputModels
{
    public class ResetPasswordModel
    {
        public int OTP { get; set; } = 0;
        public string Password { get; set; } = string.Empty;
    }
}
