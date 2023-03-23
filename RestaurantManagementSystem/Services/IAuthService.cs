using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public interface IAuthService
    {
        public Task<object> CreateUser(RegisterUser inpUser);
        public Object Login(UserDTO request);
        //public Task<Object> ForgetPassword(string email);
        //public Task<Object> Verify(ResetpassModel r, string email);
        //public Task<object> ChangePassword(ChangePassModel r, string email, string token);
        //public Task<object> Logout(string email, string token);
    }
}
