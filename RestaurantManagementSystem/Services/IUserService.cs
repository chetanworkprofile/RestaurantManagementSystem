using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public interface IUserService
    {
        public object GetYourself(string userId, string token);
        public Task<object> UpdateUser(string userId, UpdateUser u, string token);
        public Task<object> DeleteUser(string userId, string token, string password);
    }
}
