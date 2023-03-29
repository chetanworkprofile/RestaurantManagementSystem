using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public interface IFoodService
    {
        public Object AddFood(AddFood inpFood, out int code);
        public Object GetFoods(Guid? foodId, string userId, string token, string? searchString, string? category, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber, out int code);
        public Object DeleteFood(string userId, string token, Guid foodId, out int code);
        public Object UpdateFood(string userId, UpdateFood f, string tokenloggedin, out int code);
    }
}
