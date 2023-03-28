namespace RestaurantManagementSystem.Models.InputModels
{
    public class InputFoodMap
    {
        public Guid foodId { get; set; } = Guid.Empty;
        public int quantity { get; set; } = 0;
    }
}
