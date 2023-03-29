namespace RestaurantManagementSystem.Models.InputModels
{
    public class UpdateFood
    {
        public Guid foodId { get; set; } = Guid.Empty;
        public string foodName { get; set; } = string.Empty;
        public int price { get; set; } = -1;
        public int timeToPrepare { get; set; } = -1;
        public string category { get; set; } = string.Empty;
        public string pathToPic { get; set; } = string.Empty;

    }
}
