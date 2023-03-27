using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public interface IFoodService
    {
        public Task<Object> AddFood(AddFood inpFood);
    }
}
