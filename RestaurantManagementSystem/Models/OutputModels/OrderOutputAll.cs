namespace RestaurantManagementSystem.Models.OutputModels
{
    public class OrderOutputAll
    {
        public Guid orderId { get; set; }
        public Guid userId { get; set; }
        public string status { get; set; } = string.Empty;
        public int totalPrice { get; set; } = 0;
        public DateTime orderDate { get; set; }
        public  List<FoodOrderOutput> foodOrders { get; set; } = new List<FoodOrderOutput>();
        public OrderOutputAll() { }
        public OrderOutputAll(Order order, List<FoodOrderOutput> foodOrders)     //timetoprepare is in mins
        {
            this.orderId = order.orderId;
            this.userId = order.userId;
            this.status = order.status;
            this.totalPrice = order.totalPrice;
            this.orderDate = order.orderDate;
            this.foodOrders = foodOrders;
        }
    }
}
