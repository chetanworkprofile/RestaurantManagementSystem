using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagementSystem.Models
{
    public class Order
    {
        [Key]
        public Guid orderId { get; set; }
        public Guid userId { get; set; }
        public string status { get; set; } = string.Empty;
        public int totalPrice { get; set; } = 0;
        public DateTime orderDate { get; set; }
        public int rejectAttempts { get; set; } = 0;
        public Order() { }
        public Order(Guid orderId, Guid userId, string status, int totalPrice,DateTime orderDate)     //timetoprepare is in mins
        {
            this.orderId = orderId;
            this.userId = userId;
            this.status = status;
            this.totalPrice = totalPrice;
            this.orderDate = orderDate;
        }
    }
}
