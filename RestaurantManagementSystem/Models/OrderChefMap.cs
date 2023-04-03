using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class OrderChefMap
    {
        [Key]
        public Guid mapId { get; set; }
        public Guid orderId { get; set; } = Guid.Empty;
        public Guid chefId { get; set; } = Guid.Empty;

        public OrderChefMap() { }
        public OrderChefMap(Guid mapId, Guid orderId, Guid chefId)
        {
            this.mapId = mapId;
            this.orderId = orderId;
            this.chefId = chefId;
        }
    }
}
