namespace RestaurantManagementSystem.Models.InputModels
{
    public class ChefConfirmOrder
    {
        public string orderId { get; set; } = string.Empty;
        public bool accepted { get; set; } = false;
    }
}
