namespace RestaurantManagementSystem.Models.InputModels
{
    public class ChangeFoodStatus
    {
        public string foodId { get; set; } = string.Empty;
        public bool status { get; set; } = false;
    }
}
