namespace RestaurantManagementSystem.Models.InputModels
{
    public class AddFood
    {
        public string foodName { get; set; } = string.Empty;
        public int price { get; set; } = 0;
        public int timeToPrepare { get; set; } = 0; //mins
        public string category { get; set; } = string.Empty;
        public string pathToPic { get; set; } = string.Empty;
    }
}
