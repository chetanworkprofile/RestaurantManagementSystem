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

        public OrderFoodMapping() { }
        public OrderFoodMapping(Guid mapId, Guid orderId, Guid foodId, int quantity)
        {
            this.mapId = mapId;
            this.orderId = orderId;
            this.foodId = foodId;
            this.quantity = quantity;
        }
    }
}
