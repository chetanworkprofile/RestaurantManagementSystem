using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public interface IAuthService
    {
        public Task<object> CreateUser(RegisterUser inpUser);
        public Object Login(UserDTO request);
        public Task<Object> ForgetPassword(string email);
        public Task<Object> Verify(ResetPasswordModel r, string userId);
        public Task<object> ChangePassword(ChangePasswordModel r, string userId, string token);
        public Task<object> Logout(string userId, string token);
    }
}
