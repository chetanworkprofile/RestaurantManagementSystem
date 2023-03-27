using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public interface IAuthService
    {
        public Object CreateUser(RegisterUser inpUser, out int code);
        public Object Login(UserDTO request,out int code);
        public Object ForgetPassword(string email, out int code);
        public Object Verify(ResetPasswordModel r, string userId, out int code);
        public Object ChangePassword(ChangePasswordModel r, string userId, string token, out int code);
        public Object Logout(string userId, string token, out int code);
    }
}
