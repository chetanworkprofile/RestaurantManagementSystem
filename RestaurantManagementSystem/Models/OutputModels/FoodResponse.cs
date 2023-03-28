using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class FoodResponse
    {
        [Key]
        public Guid foodId { get; set; }
        public string foodName { get; set; } = string.Empty;
        public int price { get; set; } = 0;
        public TimeSpan timeToPrepare { get; set; } = TimeSpan.Zero;
        public bool status { get; set; } = false;
        public string category { get; set; } = string.Empty;
        public string pathToPic { get; set; } = string.Empty;

        public FoodResponse() { }
        public FoodResponse(Food f)     //timetoprepare is in mins
        {
            this.foodId = f.foodId;
            this.foodName = f.foodName;
            this.price = f.price;
            this.timeToPrepare = f.timeToPrepare;
            this.status = f.status;
            this.category = f.category;
            this.pathToPic = f.pathToPic;
        }
    }
}
