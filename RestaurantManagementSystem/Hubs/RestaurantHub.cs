using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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
        public static string adminUserId = "8F30D30D-9467-4F22-86AB-290EC4583691";
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
                    //await Clients.Client(adminId).SendAsync("GetOnlineUsers");
                    await Clients.All.SendAsync("GetOnlineUsers");
                    await Clients.All.SendAsync("mes",new PlaceOrder());
                }
                catch(Exception ex)
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
            await Clients.All.SendAsync("GetOnlineUsers");
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
                return Users.Where(x => x.Key == user).Select(x => x.Value).First();
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
            await Clients.Caller.SendAsync("UpdateOnlineUsers", activeList);

        }

        public async Task UserPlaceOrder(PlaceOrder inpPlacedOrder)
        {
            try
            {
                //make list of busy chefs add to it 
                var onlineChefs = OnlineUsersService();     //get length generate random number assign to that chef
                onlineChefs = onlineChefs.Where(s => s.userRole == "chef" && s.isActive==true).ToList();
                var httpContext = Context.GetHttpContext();
                var userIdString = httpContext.User.FindFirst(ClaimTypes.Sid)?.Value;
                Guid userId = new Guid(userIdString);
                /*foreach (var a in onlineChefs)
                {
                    if (BusyChefs.Contains(a.userId.ToString()))
                    {
                        onlineChefs.Remove(a);
                    }
                }*/
                int length = onlineChefs.Count();
                if (length <= 0)
                {
                    await Clients.Caller.SendAsync("Message", "No Chef available please try again later");
                    return;
                }
                Random random = new Random();
                int chefNum = random.Next(0, length);
                ActiveUsers chefSelected = onlineChefs[chefNum];
                //BusyChefs.Add(chefSelected.userId.ToString());

                Order order = new Order(Guid.NewGuid(), userId, "queued", inpPlacedOrder.totalPrice, DateTime.Now);
                await DbContext.Orders.AddAsync(order);
                foreach (var a in inpPlacedOrder.list)
                {
                    OrderFoodMapping temp = new OrderFoodMapping(Guid.NewGuid(), order.orderId, a.foodId, a.quantity);
                    await DbContext.OrderFoodMap.AddAsync(temp);
                }
                DbContext.SaveChangesAsync();

                OrderOutput res = new OrderOutput(order);
                string chefId = GetConnectionIdByUser(chefSelected.userId.ToString());

                await Clients.Client(chefId).SendAsync("ChefReceivedOrder", res);
                await Clients.Caller.SendAsync("UserOrderStatus", res);
            }
            catch (Exception ex)
            {

                await Clients.Caller.SendAsync("message", ex);
            }

        }

        public async Task ChefConfirmOrder(ChefConfirmOrder a)
        {
            Guid orderGuid = new Guid(a.orderId);
            Order order = DbContext.Orders.Find(orderGuid);

            if (order == null)
            {
                await Clients.Caller.SendAsync("Message", "Order not found please check details again");
                return;
            }
            if (a.accepted)
            {
                order.status = "preparing";
                DbContext.SaveChangesAsync();
                OrderOutput orderOutput = new OrderOutput(order);
                TimeSpan totalTime = TimeSpan.Zero;
                var orderMaps = DbContext.OrderFoodMap.Where(s=>s.orderId == orderGuid).ToList();
                foreach (var map in orderMaps)
                {
                    var food = DbContext.Foods.Find(map.foodId);
                    totalTime += food.timeToPrepare;
                }
                await Clients.Client(GetConnectionIdByUser(order.userId.ToString())).SendAsync("UserOrderStatus", orderOutput);
                await Clients.Client(GetConnectionIdByUser(order.userId.ToString())).SendAsync("Message", $"Time left for your order to be done {totalTime}");
                await Clients.Caller.SendAsync("Message", $"Time left for order {totalTime}");
            }
            else
            {
                order.rejectAttempts++;
                if(order.rejectAttempts >= 3 )
                {
                    order.status = "rejected";
                    DbContext.SaveChangesAsync();
                    OrderOutput orderOutput = new OrderOutput(order);
                    await Clients.Client(GetConnectionIdByUser(order.userId.ToString())).SendAsync("UserOrderStatus", orderOutput);
                }
                else
                {
                    var onlineChefs = OnlineUsersService();     //get length generate random number assign to that chef
                    onlineChefs = onlineChefs.Where(s => s.userRole == "chef").ToList();
                    int length = onlineChefs.Count();
                    if (length <= 0)
                    {
                        await Clients.Caller.SendAsync("Message", "No Chef available please try again later");
                        return;
                    }
                    Random random = new Random();
                    int chefNum = random.Next(0, length);
                    ActiveUsers chefSelected = onlineChefs[chefNum];
                    OrderOutput res = new OrderOutput(order);
                    string chefId = GetConnectionIdByUser(chefSelected.userId.ToString());
                    await Clients.Client(chefId).SendAsync("ChefReceivedOrder", res);
                }
            }
        }

        public async Task ChefCompletesOrder(string orderId)
        {
            Guid orderGuid = new Guid(orderId);
            Order order = DbContext.Orders.Find(orderGuid);
            order.status = "completed";
            DbContext.SaveChangesAsync();
            OrderOutput orderOutput = new OrderOutput(order);
            await Clients.Client(GetConnectionIdByUser(order.userId.ToString())).SendAsync("UserOrderStatus", orderOutput);
            await Clients.Caller.SendAsync("Message", "Order submitted successfully");
        }

        // ---------------------------------  service functions goes here --------------------------------------------------------------//
        public List<ActiveUsers> OnlineUsersService()
        {
            var onlineUsers = GetOnlineUsers(); //this will return userids of all online users
            var httpContext = Context.GetHttpContext();
            Guid loggedInUserId = new Guid(httpContext.User.FindFirst(ClaimTypes.Sid)?.Value);

            var users = DbContext.Users.ToList();
            users = users.OrderBy(u => u.userRole).ToList();
            List<ActiveUsers> activeList = new List<ActiveUsers>();
            foreach (var u in users)
            {
                var a = new ActiveUsers(u);
                if (onlineUsers.Contains(u.userId.ToString()))
                {
                    a.isActive = true;
                }
                activeList.Add(a);
            }
            var adminActive = activeList.Where(s => s.userId == loggedInUserId).First();
            activeList.Remove(adminActive);
            return activeList;
        }



        /*
        public async Task SendMessage(InputMessage msg, string? PathToFileAttachement = "empty")
        {
            _logger.LogInformation("SendMessage method started ");
            if (msg.ReceiverEmail == "" || msg.ReceiverEmail == null)
            {
                return;
            }
            //string? SenderMail = Context.User.FindFirstValue(ClaimTypes.Email);
            var httpContext = Context.GetHttpContext();
            var user1 = httpContext.User;
            var SenderMail = user1.FindFirst(ClaimTypes.Email)?.Value;
            //string SenderMail = GetUserByConnectionId(Context.ConnectionId);
            //string path = msg.PathToFileAttachement;

            //var response = AddMessage(SenderMail, msg.ReceiverEmail, msg.Content,msg.Type,msg.PathToFileAttachement);

            Console.WriteLine(msg);
            Message message = new Message()
            {
                MessageId = Guid.NewGuid(),
                SenderEmail = SenderMail,
                ReceiverEmail = msg.ReceiverEmail,
                Content = msg.Content,
                DateTime = DateTime.Now,
                Type = msg.Type,
                PathToFileAttachement = PathToFileAttachement,
                IsDeleted = false
            };
            _logger.LogInformation(message.ToString());

            await DbContext.Messages.AddAsync(message);

            var chatmap = DbContext.ChatMappings.Where(s => (s.FirstEmail == msg.ReceiverEmail && s.SecondEmail == SenderMail) || (s.FirstEmail == SenderMail && s.SecondEmail == msg.ReceiverEmail)).FirstOrDefault();
            chatmap.DateTime = DateTime.Now;
            await DbContext.SaveChangesAsync();


            string ReceiverId = GetConnectionIdByUser(msg.ReceiverEmail);
            var fileName = PathToFileAttachement.Split("/").Last();
            var sender = DbContext.Users.Where(x => x.Email == SenderMail).First();
            var receiver = DbContext.Users.Where(x => x.Email == msg.ReceiverEmail).First();
            RecevierMessage sendMsg = new RecevierMessage()
            {
                SenderEmail = SenderMail,
                SenderPicPath = sender.PathToProfilePic,
                ReceiverEmail = msg.ReceiverEmail,
                ReceiverPicPath = receiver.PathToProfilePic,
                Content = msg.Content,
                Type = msg.Type,
                DateTime = message.DateTime,
                PathToFileAttachement = PathToFileAttachement,
                FileName = fileName
            };

            await Clients.Caller.SendAsync("ReceivedMessage", sendMsg);
            if (ReceiverId != null)
            {
                await Clients.Client(ReceiverId).SendAsync("ReceivedMessage", sendMsg);
            }
            refesh();
            return;
            //await Clients.Caller.SendAsync("ReceivedMessage","helo sender");
            //handle if user is not online
        }

        public async Task CreateChat(string ConnectToMail)
        {
            Console.WriteLine("createChat fxn called");
            //string? SenderMail = Context.User.FindFirstValue(ClaimTypes.Email);
            //string SenderMail = GetUserByConnectionId(Context.ConnectionId);
            var httpContext = Context.GetHttpContext();
            var user1 = httpContext.User;
            var SenderMail = user1.FindFirst(ClaimTypes.Email)?.Value;
            string ReceiverId = GetConnectionIdByUser(ConnectToMail);
            if (SenderMail == ConnectToMail)
            {
                await Clients.Caller.SendAsync("ChatCreated", "Can't Connect to same email");
                return;
            }
            var res = await AddChat(SenderMail, ConnectToMail);
            await Clients.Client(ReceiverId).SendAsync("ChatCreated", res);
            await Clients.Caller.SendAsync("ChatCreated", res);
            await OnlineUsers();
            *//*  string ReceiverId = _chatService.GetConnectionIdByUser(ConnectToMail);
              await Clients.Client(ReceiverId).SendAsync("ReceivedMessage", res);*//*
        }

        public List<OutputChatMappings> GetChats()
        {
            _logger.LogInformation("GetChats fxn called");
            //string? Mail = Context.User.FindFirstValue(ClaimTypes.Email);
            //string Mail = GetUserByConnectionId(Context.ConnectionId);
            var httpContext = Context.GetHttpContext();
            var user1 = httpContext.User;
            var Mail = user1.FindFirst(ClaimTypes.Email)?.Value;
            var res = GetChatsService(Mail);
            string ReceiverId = GetConnectionIdByUser(Mail);
            Clients.Client(ReceiverId).SendAsync("RecievedChats", res);
            return res;
        }

        public List<OutputMessage> GetChatMessages(string OtherMail, int pageNumber)
        {
            _logger.LogInformation("GetChatMessages fxn called");
            //string? Mail = Context.User.FindFirstValue(ClaimTypes.Email);
            //string Mail = GetUserByConnectionId(Context.ConnectionId);
            var httpContext = Context.GetHttpContext();
            var user1 = httpContext.User;
            var Mail = user1.FindFirst(ClaimTypes.Email)?.Value;
            List<OutputMessage> res = GetChatMessagesService(Mail, OtherMail, pageNumber, 30);
            string ReceiverId = GetConnectionIdByUser(OtherMail);
            Clients.Caller.SendAsync("RecievedChatMessages", res);
            //Clients.Client(ReceiverId).SendAsync("RecievedChatMessages", res);
            return res;
        }



        


        //------------------------------------------------------------------------------------------------------------------//
        //----------------------------service functions-------------------------------------------------------------------//

        //create new message and add in database
        public OutputMessage AddMessage(string sender, string reciever, string content, int type, string path)
        {
            Message message = new Message()
            {
                MessageId = Guid.NewGuid(),
                SenderEmail = sender,
                ReceiverEmail = reciever,
                Content = content,
                DateTime = DateTime.Now,
                Type = type,
                PathToFileAttachement = path,
                IsDeleted = false
            };
            Console.WriteLine(message);
            Console.WriteLine("path" + path);
            OutputMessage res = new OutputMessage()
            {
                MessageId = message.MessageId,
                Content = message.Content,
                DateTime = message.DateTime,
                ReceiverEmail = message.ReceiverEmail,
                SenderEmail = message.SenderEmail,
                Type = type,
                PathToFileAttachement = path,
            };
            DbContext.Messages.Add(message);
            DbContext.SaveChanges();
            return res;
        }

        // create new chat mapping if exist send it
        public async Task<object> AddChat(string FirstMail, string SecondMail)
        {
            var chatdb = DbContext.ChatMappings;
            var user2 = DbContext.Users.Where(s => s.Email == SecondMail).FirstOrDefault();
            if (user2 == null)
            {
                response2.StatusCode = 400;
                response2.Message = "User you are trying to connect does not exist";
                response2.Success = true;
                return response2;
            }
            var chats = chatdb.Where(c => c.FirstEmail == FirstMail && c.SecondEmail == SecondMail).FirstOrDefault();
            if (chats == null)
            {
                chats = chatdb.Where(c => c.FirstEmail == SecondMail && c.SecondEmail == FirstMail).FirstOrDefault();
            }

            if (chats == null)
            {
                ChatMappings chatMap = new ChatMappings()
                {
                    ChatId = Guid.NewGuid(),
                    FirstEmail = FirstMail,
                    SecondEmail = SecondMail,
                    DateTime = DateTime.Now,
                    IsDeleted = false
                };

                await DbContext.ChatMappings.AddAsync(chatMap);
                await DbContext.SaveChangesAsync();
                chats = chatMap;
                *//* response.Data = output;*//*
            }
            var user1 = DbContext.Users.Where(s => s.Email == chats.FirstEmail).FirstOrDefault();
            user2 = DbContext.Users.Where(s => s.Email == chats.SecondEmail).FirstOrDefault();
            OutputChatMappings output = new OutputChatMappings()
            {
                ChatId = chats.ChatId,
                FirstEmail = chats.FirstEmail,
                FirstName1 = user1.FirstName,
                LastName1 = user1.LastName,
                SecondEmail = chats.SecondEmail,
                FirstName2 = user2.FirstName,
                LastName2 = user2.LastName,
                DateTime = chats.DateTime,
            };
            response.Data = output;
            response.StatusCode = 200;
            response.Message = "Chat created/exists";
            response.Success = true;

            return response;
        }

        //function invoked to get all chat mappings created for a particular user
        public List<OutputChatMappings> GetChatsService(string email)
        {
            var chatMaps = DbContext.ChatMappings.ToList();
            chatMaps = chatMaps.Where(s => (s.FirstEmail == email) || (s.SecondEmail == email)).ToList();
            //var chatMaps2 = chatMaps.Where(s => (s.SecondEmail == email)).ToList();
            *//*chatMaps = chatMaps.OrderBy(m => m.DateTime).Select(m => m).ToList();
            chatMaps2 = chatMaps2.OrderBy(m => m.DateTime).Select(m => m).ToList();*//*
            chatMaps.Remove(chatMaps.Where(s => s.FirstEmail == s.SecondEmail).FirstOrDefault());
            List<OutputChatMappings> res = new List<OutputChatMappings>();
            OutputChatMappings output = new OutputChatMappings() { };
            foreach (var cm in chatMaps)
            {
                var user1 = DbContext.Users.Where(s => s.Email == cm.FirstEmail).FirstOrDefault();
                var user2 = DbContext.Users.Where(s => s.Email == cm.SecondEmail).FirstOrDefault();


                output.ChatId = cm.ChatId;
                output.FirstEmail = cm.FirstEmail;
                output.FirstName1 = user1.FirstName;
                output.LastName1 = user1.LastName;
                output.SecondEmail = cm.SecondEmail;
                output.FirstName2 = user2.FirstName;
                output.LastName2 = user2.LastName;
                output.DateTime = cm.DateTime;

                res.Add(output);
            }

            res = res.OrderByDescending(x => x.DateTime).ToList();
            Console.WriteLine(res.Count);

            return res;
        }

        // function invoked to get previous chat between two users
        public List<OutputMessage> GetChatMessagesService(string email, string otherEmail, int pageNumber, int skipLimit)
        {
            //var messages = DbContext.Messages.AsQueryable();
            var messages = DbContext.Messages.ToList();
            //chatMaps = chatMaps.Where(s => (s.FirstEmail == email || s.SecondEmail == email)).ToList();
            messages = messages.Where(m => (m.SenderEmail == email && m.ReceiverEmail == otherEmail) || (m.SenderEmail == otherEmail && m.ReceiverEmail == email)).ToList();

            messages = messages.OrderByDescending(m => m.DateTime).Select(m => m).ToList();
            messages = messages.Skip((pageNumber - 1) * skipLimit).Take(skipLimit).ToList();

            List<OutputMessage> res = new List<OutputMessage>();

            foreach (var msg in messages)
            {
                var sender = DbContext.Users.Where(x => x.Email == msg.SenderEmail).First();
                var receiver = DbContext.Users.Where(x => x.Email == msg.ReceiverEmail).First();
                var fileName = msg.PathToFileAttachement.Split("/").Last();
                OutputMessage output = new OutputMessage()
                {
                    MessageId = msg.MessageId,
                    Content = msg.Content,
                    DateTime = msg.DateTime,
                    ReceiverEmail = msg.ReceiverEmail,
                    ReceiverPicPath = receiver.PathToProfilePic,
                    SenderEmail = msg.SenderEmail,
                    SenderPicPath = sender.PathToProfilePic,
                    Type = msg.Type,
                    PathToFileAttachement = msg.PathToFileAttachement,
                    FileName = fileName,
                };
                res.Add(output);
            }
            res.Reverse();
            return res;
        }*/
    }
}
