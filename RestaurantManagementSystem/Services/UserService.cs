using RestaurantManagementSystem.Controllers;
using RestaurantManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Data;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Object = System.Object;
using RestaurantManagementSystem.Models.InputModels;

namespace RestaurantManagementSystem.Services
{
    public class UserService: IUserService
    {
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        private readonly RestaurantDbContext DbContext;
        private readonly IConfiguration _configuration;
        IAuthService authService;
        // secondary service file to make code clean
        SecondaryAuthService _secondaryAuthService;

        public UserService(IConfiguration configuration, RestaurantDbContext dbContext, ILogger<AuthController> logger)

        {
            this._configuration = configuration;
            DbContext = dbContext;
            authService = new AuthService(configuration, dbContext, logger);
            _secondaryAuthService = new SecondaryAuthService(configuration, dbContext, logger);
        }

        public object GetYourself(string userId, string token)
        {
            Guid id = new Guid(userId);
            var userLoggedIn = DbContext.Users.Find(id);

            if (userLoggedIn == null || userLoggedIn.isDeleted == true)
            {
                response2 = new ResponseWithoutData(404, "Can't get details user not found", false);
                return response2;
            }

            if (token != userLoggedIn.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                return response2;
            }

            ResponseUser r = new ResponseUser(userLoggedIn.userId, userLoggedIn.firstName, userLoggedIn.lastName, userLoggedIn.email, userLoggedIn.phone,userLoggedIn.userRole, userLoggedIn.address, userLoggedIn.pathToProfilePic, userLoggedIn.createdAt, userLoggedIn.updatedAt);

            response = new Response(200, "User fetched", r, true);
            return response;
        }
        public object GetUsers(string userId,string token,Guid? UserId, string? searchString, string? Email, long Phone, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber)          // sort order   ===   e1 for ascending   -1 for descending
        {
            //get logged in user from database
            Guid id = new Guid(userId);
            var userLoggedIn = DbContext.Users.Find(id);
            var userss = DbContext.Users.AsQueryable();

            //userss = userss.Where(x => (x.UserId == UserId || UserId == null) && (x.IsDeleted == false) && (EF.Functions.Like(x.FirstName, "%" + searchString + "%") || EF.Functions.Like(x.LastName, "%" + searchString + "%") || searchString == null) &&
            //(x.Email == Email || Email == null)).Select(x => x);

            userss = userss.Where(t => t.isDeleted == false);     //remove deleted users from list
            
            if (token != userLoggedIn.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                return response2;
            }

            //--------------------------filtering based on userId,searchString, Email, or Phone---------------------------------//
            
            if (UserId != null) { userss = userss.Where(s => (s.userId == UserId)); }
            if (searchString != null) { userss = userss.Where(s => EF.Functions.Like(s.firstName, "%" + searchString + "%") || EF.Functions.Like(s.lastName, "%" + searchString + "%") || EF.Functions.Like(s.firstName +" "+s.lastName, "%" + searchString + "%")); }
            //if (FirstName != null) { users = users.Where(s => (s.FirstName == FirstName)).ToList(); }
            if (Email != null) { userss = userss.Where(s => (s.email == Email)); }
            if (Phone != -1) { userss = userss.Where(s => (s.phone == Phone)); }

            var users = userss.ToList();

            // delegate used to create orderby depending on user input
            Func<User, Object> orderBy = s => s.userId;
            if (OrderBy == "UserId" || OrderBy == "ID" || OrderBy == "Id")
            {
                orderBy = x => x.userId;
            }
            else if (OrderBy == "FirstName" || OrderBy == "Name" || OrderBy == "firstname")
            {
                orderBy = x => x.firstName;
            }
            else if (OrderBy == "Email" || OrderBy == "email")
            {
                orderBy = x => x.email;
            }
            else if (OrderBy == "UserRole" || OrderBy == "userRole" || OrderBy == "userrole")
            {
                orderBy = x => x.userRole;
            }

            // sort according to input based on orderby
            if (SortOrder == 1)
            {
                users = users.OrderBy(orderBy).Select(c => (c)).ToList();
            }
            else
            {
                users = users.OrderByDescending(orderBy).Select(c => (c)).ToList();
            }

            //pagination
            users = users.Skip((PageNumber - 1) * RecordsPerPage)
                                  .Take(RecordsPerPage).ToList();

            List<ResponseUser> res = new List<ResponseUser>();

            foreach (var user in users)
            {
                ResponseUser r = new ResponseUser(user.userId, user.firstName, user.lastName, user.email, user.phone,user.userRole, user.address, user.pathToProfilePic, user.createdAt, user.updatedAt);
                res.Add(r);
            }

            if (!res.Any())
            {
                response2 = new ResponseWithoutData(200, "No User found.", true);
                return response2;
            }
            response = new Response(200, "Users list fetched", res, true);
            return response;
        }

        public async Task<Object> UpdateUser(string userId, UpdateUser u, string tokenloggedin)
        {
            Guid id = new Guid(userId);
            User? user = DbContext.Users.Find(id);

            if (tokenloggedin != user.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                return response2;
            }

            if (user != null && user.isDeleted == false)
            {
                if (u.firstName != "string" && u.firstName != string.Empty)
                {
                    user.firstName = u.firstName;
                }
                if (u.lastName != "string" && u.lastName != string.Empty)
                {
                    user.lastName = u.lastName;
                }
                if (u.pathToProfilePic != "string" && u.pathToProfilePic != string.Empty)
                {
                    user.pathToProfilePic = u.pathToProfilePic;
                }
                if (u.address != "string" && u.address != string.Empty)
                {
                    user.address = u.address;
                }
                if (u.phone != -1 && u.phone != 0)
                {
                    user.phone = u.phone;
                }
                
                user.updatedAt = DateTime.Now;
                await DbContext.SaveChangesAsync();

                /*var userToken = new CreateToken(user.userId, user.firstName, user.email, "user");
                string token = _secondaryAuthService.CreateToken(userToken);
                user.token = token;*/
                RegistrationLoginResponse data = new RegistrationLoginResponse(user.userId, user.email, user.firstName, user.lastName, user.userRole, user.token);

                /*await DbContext.SaveChangesAsync();*/
                response = new Response(200, "User updated successfully", data, true);
                return response;
            }
            else
            {
                response2 = new ResponseWithoutData(404, "User not found", false);
                return response2;
            }
        }

        public async Task<object> DeleteUser(string userId, string token, string password)
        {
            Guid id = new Guid(userId);
            User? user = DbContext.Users.Find(id);
            if (token != user.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                return response2;
            }
            byte[] phash = user.passwordHash;
            if (!_secondaryAuthService.VerifyPasswordHash(password, phash))
            {
                response2 = new ResponseWithoutData(400, "Wrong password.", false);
                return response2;
            }

            if (user != null && user.isDeleted == false)
            {
                user.isDeleted = true;
                user.token = string.Empty;
                await DbContext.SaveChangesAsync();

                response2 = new ResponseWithoutData(200, "User deleted successfully", true);
                return response2;
            }
            else
            {
                response2 = new ResponseWithoutData(404, "User Not found", false);
                return response2;
            }

        }
    }
}
