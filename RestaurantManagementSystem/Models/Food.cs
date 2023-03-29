using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class Food
    {
        [Key]
        public Guid foodId { get; set; }
        public bool isDeleted { get; set; }
        public string foodName { get; set; } = string.Empty;
        public int price { get; set; } = 0;
        public TimeSpan timeToPrepare { get; set; } = TimeSpan.Zero;
        public bool status { get; set; } = false;           //1 - available  0 - not available
        public string category { get; set; } = string.Empty;
        public string pathToPic { get; set; } = string.Empty;

        public Food() { }
        public Food(Guid foodId, string foodName, int price, int timeToPrepare, bool status, string category, string pathToPic)     //timetoprepare is in mins
        {
            int hours = 0;
            int mins = 0;
            int secs = 0;
            if (timeToPrepare > 60)
            {
                hours = timeToPrepare/60;
                mins = timeToPrepare%60;
            }
            else
            {
                mins = timeToPrepare;
            }
            this.foodId = foodId;
            this.foodName = foodName;
            this.price = price;
            this.timeToPrepare = new TimeSpan(hours,mins,secs);
            this.status = status;
            this.category = category;
            this.pathToPic = pathToPic;
            isDeleted= false;
        }
    }
}
