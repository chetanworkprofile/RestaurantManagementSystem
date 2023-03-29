using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public interface IOrderService
    {
        public Object GetOrdersAsAdmin(Guid? orderId, string loggedInUserId, string token, string? userId, string? status, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber, out int code);
        public Object GetOrdersAsUser(Guid? orderId, string loggedInUserId, string token, string? status, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber, out int code);
    }
}
