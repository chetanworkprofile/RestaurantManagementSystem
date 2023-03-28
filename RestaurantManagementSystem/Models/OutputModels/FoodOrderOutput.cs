namespace RestaurantManagementSystem.Models.OutputModels
{
    public class FoodOrderOutput
    {
        public Guid foodId { get; set; }
        public string foodName { get; set; } = string.Empty;
        public int price { get; set; } = 0;
        //public TimeSpan timeToPrepare { get; set; } = TimeSpan.Zero;
        public bool status { get; set; } = false;
        public string category { get; set; } = string.Empty;

        public int quantity { get; set; } = 0;
        public FoodOrderOutput() { }
        public FoodOrderOutput(Food food,int quantity)
        {
            this.foodId = food.foodId;
            this.foodName = food.foodName;
            this.price = food.price;
            this.status = food.status;
            this.category = food.category;
            this.quantity = quantity;
        }
    }
}
//this model is used as a helper model for orderoutputall