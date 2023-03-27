namespace RestaurantManagementSystem.Services
{
    public interface IUploadPicService
    {
        public Task<object> ProfilePicUploadAsync(IFormFile file, string userId, string token, string userRole);
        public Task<object> FoodPicUploadAsync(IFormFile file, string userId, string token);
    }
}
