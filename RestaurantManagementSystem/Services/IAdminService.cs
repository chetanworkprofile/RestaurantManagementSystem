using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public interface IAdminService
    {
        public Object AddChef(RegisterUser inpUser, out int code);
        public Object GetUsers(string userId, string userType, string token, Guid? UserId, string? searchString, string? Email, long Phone, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber, out int code);
        public Object DeleteUser(string userId, string token, out int code);
        public Object ToggleBlockUser(string userId, string token, out int code);

    }
}
