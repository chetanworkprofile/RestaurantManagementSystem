﻿namespace RestaurantManagementSystem.Models.InputModels
{
    public class PlaceOrder
    {
        //public Guid userId { get; set; }
        public int totalPrice { get; set; } = 0;
        public List<InputFoodMap> list { get; set; } = new List<InputFoodMap>();

        public PlaceOrder() { }
        public PlaceOrder(Guid userId, int totalPrice, List<InputFoodMap> inpList)
        {
            this.totalPrice = totalPrice;
            foreach (var item in inpList)
            {
                list.Add(item);
            }
        }
    }
}
