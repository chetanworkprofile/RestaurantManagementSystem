namespace RestaurantManagementSystem.Models.OutputModels
{
    public class OrderOutput
    {
        public Guid orderId { get; set; }
        public Guid userId { get; set; }
        public string status { get; set; } = string.Empty;
        public int totalPrice { get; set; } = 0;
        public DateTime orderDate { get; set; }
        public OrderOutput() { }
        public OrderOutput(Order order)     //timetoprepare is in mins
        {
            this.orderId = order.orderId;
            this.userId = order.userId;
            this.status = order.status;
            this.totalPrice = order.totalPrice;
            this.orderDate = order.orderDate;
        }
    }
}
