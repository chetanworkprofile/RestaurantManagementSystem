namespace RestaurantManagementSystem.Services
{
    public interface IUploadPicService
    {
        public Task<object> ProfilePicUploadAsync(IFormFile file, string userId, string token, string userRole);
    }
}
