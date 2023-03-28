using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Models;
using System.Text.RegularExpressions;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Controllers;

namespace RestaurantManagementSystem.Services
{
    public class OrderService : IOrderService
    {
        Response response = new Response();                             //response models/objects
        ResponseWithoutData response2 = new ResponseWithoutData();
        CreateToken tokenUser = new CreateToken();              // model to create token
        object result = new object();
        private readonly RestaurantDbContext DbContext;
        private readonly IConfiguration _configuration;

        public OrderService(IConfiguration configuration, RestaurantDbContext dbContext)
        {
            this._configuration = configuration;
            DbContext = dbContext;
        }

        public Object GetOrdersAsAdmin(Guid? orderId, string loggedInUserId,string token, string? userId, string? status, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber, out int code)          // sort order   ===   e1 for ascending   -1 for descending
        {
            //get logged in user from database
            Guid id = new Guid(loggedInUserId);
            var userLoggedIn = DbContext.Users.Find(id);
            var orders = DbContext.Orders.AsQueryable();

            if (token != userLoggedIn.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                code = 401;
                return response2;
            }
            //var foods = DbContext.Foods.AsQueryable();
            //var orderFoodMap = DbContext.OrderFoodMap.AsQueryable();
            //ord1erFoodMap = orderFoodMap.Where()
            //--------------------------filtering based on userId,searchString, Email, or Phone---------------------------------//

            if (orderId != null) { orders = orders.Where(s => (s.orderId == orderId)); }
            if (userId != null) { orders = orders.Where(s => (s.userId == new Guid(userId))); }
            //if (searchString != null) { orders = orders.Where(s => EF.Functions.Like(s.foodName, "%" + searchString + "%") || EF.Functions.Like(s.category, "%" + searchString + "%")); }
            //if (FirstName != null) { users = users.Where(s => (s.FirstName == FirstName)).ToList(); }
            if ( status != "all") { orders = orders.Where(s => (s.status == status)); }

            var ordersList = orders.ToList();

            // delegate used to create orderby depending on user input
            Func<Order, Object> orderBy = s => s.orderId;
            if (OrderBy == "OrderId" || OrderBy == "ID" || OrderBy == "Id" || OrderBy == "orderId" || OrderBy == "orderid")
            {
                orderBy = x => x.orderId;
            }
            else if (OrderBy == "userId" || OrderBy == "UserId" || OrderBy == "USERID" || OrderBy == "userid" )
            {
                orderBy = x => x.userId;
            }
            else if (OrderBy == "status" || OrderBy == "Status")
            {
                orderBy = x => x.status;
            }

            // sort according to input based on orderby
            if (SortOrder == 1)
            {
                ordersList = ordersList.OrderBy(orderBy).Select(c => (c)).ToList();
            }
            else
            {
                ordersList = ordersList.OrderByDescending(orderBy).Select(c => (c)).ToList();
            }

            //pagination
            ordersList = ordersList.Skip((PageNumber - 1) * RecordsPerPage)
                                  .Take(RecordsPerPage).ToList();

            List<OrderOutputAll> res = new List<OrderOutputAll>();

            foreach (var order in ordersList)
            {
                var foodsOrderMaps = DbContext.OrderFoodMap.Where(s=>s.orderId== order.orderId).ToList();
                List<FoodOrderOutput> foodsList = new List<FoodOrderOutput>();
                foreach(var foodMap in foodsOrderMaps)
                {
                    var food = DbContext.Foods.Find(foodMap.foodId);
                    FoodOrderOutput temp = new FoodOrderOutput(food, foodMap.quantity);
                    foodsList.Add(temp);
                }
                OrderOutputAll r = new OrderOutputAll(order,foodsList);
                res.Add(r);
            }

            if (!res.Any())
            {
                response2 = new ResponseWithoutData(404, "No order found.", true);
                code = 404;
                return response2;
            }
            response = new Response(200, "Orders list fetched", res, true);
            code = 200;
            return response;
        }
    }
}
