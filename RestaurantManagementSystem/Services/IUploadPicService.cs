namespace RestaurantManagementSystem.Services
{
    public interface IUploadPicService
    {
        public Object ProfilePicUpload(IFormFile file, string userId, string token, string userRole, out int code);
        public Object FoodPicUpload(IFormFile file, string userId, string token, out int code);
    }
}
