using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Services
{
    public interface IUserService
    {
        public object GetYourself(string userId, string token);
        public object GetUsers(string userId, string token, Guid? UserId, string? searchString, string? Email, long Phone, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber);
        //public Task<object> UpdateUser(string email, UpdateUser u, string token);
        //public Task<object> DeleteUser(string email, string token, string password);
    }
}
