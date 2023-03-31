using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using RestaurantManagementSystem.Controllers;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models.OutputModels;
using System.Collections.Immutable;
using System.Security.Claims;

namespace RestaurantManagementSystem.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestaurantHub : Hub
    {
        // to keep track of online users dict key-value pair
        // key - userId     value - connectionId
        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>();
        //private static List<string> BusyChefs = new List<string>();
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        object result = new object();

        private readonly RestaurantDbContext DbContext;
        //private readonly IConfiguration _configuration;
        private readonly ILogger<string> _logger;
        public static string adminUserId = "8f30d30d-9467-4f22-86ab-290ec4583691";
        public string adminId;

        public RestaurantHub(RestaurantDbContext dbContext, ILogger<string> logger)
        {
            DbContext = dbContext;
            _logger = logger;
        }

        public async override Task<Task> OnConnectedAsync()             // on connected server calls client function to send email that is added in dictionary
        {
            try
            {
                _logger.LogInformation("New User connected to socket " + Context.ConnectionId);
                string? email = Context.User.FindFirstValue(ClaimTypes.Email);
                string? userId = Context.User.FindFirstValue(ClaimTypes.Sid);
                try {
                    AddUserConnectionId(userId);
                    adminId = GetConnectionIdByUser(adminUserId);
                    _logger.LogInformation($"admin userId: {adminUserId} admin id: {adminId}");
                    if (adminId != null)
                    {
                        await Clients.Clients(adminId).SendAsync("GetOnlineUsers");
                    }
                    //await Clients.Clients(adminId).SendAsync("GetOnlineUsers");
                    //await Clients.Caller.SendAsync(adminId);
                    //await Clients.AllExcept(Context.ConnectionId).SendAsync("hi All");
                    //await Clients.All.SendAsync("GetOnlineUsers");
                    /*await Clients.All.SendAsync("mes",new PlaceOrder());*/
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                //Clients.Caller.SendAsync("UserConnected");
                //Clients.All.SendAsync("refresh");

                //refereh function is called on client side to alarm them to call Online users function and get updated list of online users

                //DisplayOnlineUsers();
                //await Clients.All.SendAsync("UpdateOnlineUsers",_chatService.GetOnlineUsers());
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
            }
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("user disconnected");
            var user = GetUserByConnectionId(Context.ConnectionId);
            RemoveUserFromList(user);
            adminId = GetConnectionIdByUser(adminUserId);
            _logger.LogInformation($"admin userId: {adminUserId} admin id: {adminId}");
            if (adminId != null)
            {
                await Clients.Client(adminId).SendAsync("GetOnlineUsers"); 
            }
            //await Clients.All.SendAsync("GetOnlineUsers");
            //Clients.All.SendAsync("refresh");
            //await OnlineUsers();
            await base.OnDisconnectedAsync(exception);
        }


        /*public void refesh()
        {
            Clients.All.SendAsync("refresh");
        }*/
        public void AddUserConnectionId(string userId)
        {
            _logger.LogInformation("User added to online dictionary "+ userId);
            AddUserToList(userId, Context.ConnectionId);
            //await OnlineUsers();
        }
        public bool AddUserToList(string userToAdd, string connectionId)
        {
            lock (Users)
            {
                foreach (var user in Users)
                {
                    if (user.Key.ToLower() == userToAdd.ToLower())
                    {
                        return false;
                    }
                }

                Users.Add(userToAdd, connectionId);
                _logger.LogInformation($"Add user : {userToAdd} connection id: {connectionId}");
                return true;
            }
        }

        public string GetUserByConnectionId(string connectionId)
        {
            lock (Users)
            {
                return Users.Where(x => x.Value == connectionId).Select(x => x.Key).First();
            }
        }

        public string GetConnectionIdByUser(string user)
        {
            lock (Users)
            {
                return Users.Where(x => x.Key == user).Select(x => x.Value).FirstOrDefault();
            }
        }

        public void RemoveUserFromList(string user)
        {
            lock (Users)
            {
                if (Users.ContainsKey(user))
                {
                    Users.Remove(user);
                }
            }
        }

        //get users email in dictionary
        public string[] GetOnlineUsers()
        {
            lock (Users)
            {
                return Users.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
            }
        }

        //[Authorize(Roles = "admin")]
        public async Task OnlineUsers()
        {
            _logger.LogInformation("Online Users method started");
            List<ActiveUsers> activeList = OnlineUsersService();
            var users = DbContext.Users.ToList();
            int count = users.Count;
            DataListForGet res = new DataListForGet(count, activeList);
            await Clients.Caller.SendAsync("UpdateOnlineUsers", res);

        }

        public async Task UserPlaceOrder(PlaceOrder inpPlacedOrder)
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userIdString = httpContext.User.FindFirst(ClaimTypes.Sid)?.Value;
                Guid userId = new Guid(userIdString);

                Order order = new Order(Guid.NewGuid(), userId, "queued", 0 , DateTime.Now);
                
                int totalPrice = 0;
                foreach (var a in inpPlacedOrder.list)
                {
                    OrderFoodMapping temp = new OrderFoodMapping(Guid.NewGuid(), order.orderId, a.foodId, a.quantity);
                    var food = DbContext.Foods.Find(a.foodId);
                    totalPrice += food.price*a.quantity;
                    await DbContext.OrderFoodMap.AddAsync(temp);
                }
                order.totalPrice = totalPrice;
                await DbContext.Orders.AddAsync(order);
                DbContext.SaveChanges();

                string chefId = SelectChef();
                if(chefId == null)
                {
                    await Clients.Caller.SendAsync("Message", "No Chef available please try again later");
                    return;
                }

                OrderOutput res = new OrderOutput(order);

                await Clients.Client(chefId).SendAsync("ChefReceivedOrder", res);
                await Clients.Caller.SendAsync("UserOrderStatus", res);
                adminId = GetConnectionIdByUser(adminUserId);
                if (adminId != null)
                {
                    await Clients.Clients(adminId).SendAsync("GetOrders",order);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.ToString());
            }

        }

        public async Task ChefConfirmOrder(ChefConfirmOrder a)
        {
            Guid orderGuid = new Guid(a.orderId);
            Order order = await DbContext.Orders.FindAsync(orderGuid);

            if (order == null)
            {
                await Clients.Caller.SendAsync("Message", "Order not found please check details again");
                return;
            }
            if (a.accepted)
            {
                order.status = "preparing";
                await DbContext.SaveChangesAsync();
                OrderOutput orderOutput = new OrderOutput(order);
                TimeSpan totalTime = TimeSpan.Zero;
                var orderMaps = DbContext.OrderFoodMap.Where(s=>s.orderId == orderGuid).ToList();
                foreach (var map in orderMaps)
                {
                    var food = DbContext.Foods.Find(map.foodId);
                    //totalTime += food.timeToPrepare*map.quantity;
                    totalTime += food.timeToPrepare;
                }
                await Clients.Client(GetConnectionIdByUser(order.userId.ToString())).SendAsync("UserOrderStatus", orderOutput);
                await Clients.Client(GetConnectionIdByUser(order.userId.ToString())).SendAsync("Message", $"Time left for your order to be done {totalTime}");
                await Clients.Caller.SendAsync("Message", $"Time left for order {totalTime}");
                adminId = GetConnectionIdByUser(adminUserId);
                if (adminId != null)
                {
                    await Clients.Clients(adminId).SendAsync("GetOrders", order);
                }
            }
            else
            {
                order.rejectAttempts++;
                if(order.rejectAttempts >= 3 )
                {
                    order.status = "rejected";
                    await DbContext.SaveChangesAsync();
                    OrderOutput orderOutput = new OrderOutput(order);
                    await Clients.Client(GetConnectionIdByUser(order.userId.ToString())).SendAsync("UserOrderStatus", orderOutput);
                    adminId = GetConnectionIdByUser(adminUserId);
                    if (adminId != null)
                    {
                        await Clients.Clients(adminId).SendAsync("GetOrders", order);
                    }
                }
                else
                {
                    string chefId = SelectChef();
                    if (chefId == null)
                    {
                        string userId = GetConnectionIdByUser(order.userId.ToString());
                        await Clients.Client(userId).SendAsync("Message", "No Chef available please try again later");
                        await DbContext.SaveChangesAsync();
                        return;
                    }
                    OrderOutput res = new OrderOutput(order);
                    await Clients.Client(chefId).SendAsync("ChefReceivedOrder", res);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task ChefCompletesOrder(string orderId)
        {
            Guid orderGuid = new Guid(orderId);
            Order order = await DbContext.Orders.FindAsync(orderGuid);
            order.status = "completed";
            await DbContext.SaveChangesAsync();
            OrderOutput orderOutput = new OrderOutput(order);
            await Clients.Client(GetConnectionIdByUser(order.userId.ToString())).SendAsync("UserOrderStatus", orderOutput);
            await Clients.Caller.SendAsync("Message", "Order submitted successfully");
            adminId = GetConnectionIdByUser(adminUserId);
            if (adminId != null)
            {
                await Clients.Clients(adminId).SendAsync("GetOrders", order);
            }
        }

        public async Task ChefChangeFoodStatus(ChangeFoodStatus inp)       // true - available  false - not available
        {
            Guid foodGuid = new Guid(inp.foodId);
            Food food= DbContext.Foods.Find(foodGuid);
            food.status = inp.status;
            DbContext.SaveChangesAsync();
            FoodResponse foodOutput = new FoodResponse(food);
            //await Clients.Client(GetConnectionIdByUser(order.userId.ToString())).SendAsync("UserOrderStatus", orderOutput);
            await Clients.All.SendAsync("UpdateFoods",food);
        }

        // ---------------------------------  service functions goes here --------------------------------------------------------------//
        public List<ActiveUsers> OnlineUsersService()
        {
            var onlineUsers = GetOnlineUsers(); //this will return userids of all online users
            var httpContext = Context.GetHttpContext();
            Guid loggedInUserId = new Guid(httpContext.User.FindFirst(ClaimTypes.Sid)?.Value);

            var users = DbContext.Users.ToList();
            users = users.OrderBy(u => u.userRole).ToList();
;
            List<ActiveUsers> activeList = new List<ActiveUsers>();
            foreach (var u in onlineUsers)
            {
                Guid tempId = new Guid(u);
                User tempUser = DbContext.Users.Find(tempId);
                var a = new ActiveUsers(tempUser);
                a.isActive = true;
                /*if (onlineUsers.Contains(u.userId.ToString()))
                {
                    a.isActive = true;
                }*/
                activeList.Add(a);
            }
            var adminActive = activeList.Where(s => s.userId == loggedInUserId).First();
            activeList.Remove(adminActive);
            return activeList;
        }

        public string SelectChef()
        {
            /*foreach (var a in onlineChefs)
                {
                    if (BusyChefs.Contains(a.userId.ToString()))
                    {
                        onlineChefs.Remove(a);
                    }
                }*/

            //make list of busy chefs add to it 
            var onlineChefs = OnlineUsersService();     //get length generate random number assign to that chef
            onlineChefs = onlineChefs.Where(s => s.userRole == "chef" && s.isActive == true).ToList();
            int length = onlineChefs.Count();
            if (length <= 0)
            {
                return null;
            }
            Random random = new Random();
            int chefNum = random.Next(0, length);
            ActiveUsers chefSelected = onlineChefs[chefNum];
            //BusyChefs.Add(chefSelected.userId.ToString());
            string chefId = GetConnectionIdByUser(chefSelected.userId.ToString());
            return chefId;
        }

    }
}
