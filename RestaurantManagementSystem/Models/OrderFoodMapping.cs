using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class OrderFoodMapping
    {
        [Key]
        public Guid mapId { get; set; }
        public Guid orderId { get; set; } = Guid.Empty;
        public Guid foodId { get; set; } = Guid.Empty;
        public int quantity { get; set; } = 0;
    }
}
