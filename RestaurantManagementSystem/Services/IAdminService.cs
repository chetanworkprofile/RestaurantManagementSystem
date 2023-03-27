using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public interface IAdminService
    {
        public Task<Object> AddChef(RegisterUser inpUser);
        public object GetUsers(string userId, string token, Guid? UserId, string? searchString, string? Email, long Phone, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber);

    }
}
