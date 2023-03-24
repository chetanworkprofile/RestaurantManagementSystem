using RestaurantManagementSystem.Controllers;
using RestaurantManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Data;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Object = System.Object;

namespace RestaurantManagementSystem.Services
{
    public class UserService: IUserService
    {
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        //TokenUser tokenUser = new TokenUser();
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
                response2 = new ResponseWithoutData(404, "Can't get details user error", false);
                return response2;
            }

            if (token != userLoggedIn.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                return response2;
            }

            ResponseUser r = new ResponseUser(userLoggedIn.userId, userLoggedIn.firstName, userLoggedIn.lastName, userLoggedIn.email, userLoggedIn.phone, userLoggedIn.address, userLoggedIn.pathToProfilePic, userLoggedIn.createdAt, userLoggedIn.updatedAt);

            response = new Response(200, "Users list fetched", r, true);
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
                ResponseUser r = new ResponseUser(user.userId, user.firstName, user.lastName, user.email, user.phone, user.address, user.pathToProfilePic, user.createdAt, user.updatedAt);
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

        /*public async Task<object> UpdateUser(string email, UpdateUser u,string tokenloggedin)
        {
            User? user = await DbContext.Users.Where(u=>u.Email == email).FirstOrDefaultAsync();
            TimeSpan ageTimeSpan = DateTime.Now - u.DateOfBirth;
            int age = (int)(ageTimeSpan.Days / 365.25);

            if (tokenloggedin != user.Token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                return response2;
            }


            if (user != null && user.IsDeleted == false)
            {
                if (u.FirstName != "string" && u.FirstName != string.Empty)
                {
                    user.FirstName = u.FirstName;
                }
                if (u.LastName != "string" && u.LastName != string.Empty)
                {
                    user.LastName = u.LastName;
                }
               *//* if (u.Email != "string" && u.Email != string.Empty)
                {
                    var EmailAlreadyExists = DbContext.Users.Where(s => s.Email == u.Email).First();
                    if(EmailAlreadyExists!=null)
                    {
                        response2 = new ResponseWithoutData(400, "Email you entered already registered. Please try another", false);
                        return response2;
                    }
                    user.Email = u.Email;
                }*//*
                if (u.PathToProfilePic != "string" && u.PathToProfilePic != string.Empty)
                {
                    user.PathToProfilePic = u.PathToProfilePic;
                }
                if (u.Phone != -1 && u.Phone != 0)
                {
                    user.Phone = u.Phone;
                }
                if (u.DateOfBirth != DateTime.MinValue && u.DateOfBirth != DateTime.Now)
                {
                    // Perform your DOB validation here based on your specific requirements
                    if (age < 12)
                    {
                        // The user is not enough
                        response2.StatusCode = 200;
                        response2.Message = "Not allowed. User is underage. Must be atleast 12 years old";
                        response2.Success = false;
                        return response2;
                    }
                    user.DateOfBirth = u.DateOfBirth;
                }
                user.UpdatedAt= DateTime.Now;
                await DbContext.SaveChangesAsync();
                
                *//*var tokenUser = new TokenUser()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Role = "login"
                    *//* LastName= inpUser.LastName,
                     UserId = user.UserId*//*
                };
                string token = _secondaryAuthService.CreateToken(tokenUser);
                user.Token = token;*//*
                ResponseDataObj2 data = new ResponseDataObj2()
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    //Token = token
                };
                await DbContext.SaveChangesAsync();
                response.StatusCode = 200;
                response.Message = "User updated successfully";
                response.Success = true;
                response.Data = data;
                return response;
            }
            else
            {
                response2.StatusCode = 404;
                response2.Message = "User not found";
                response2.Success = false;
                return response2;
            }
        }*/

        /*public async Task<object> DeleteUser(string email, string token,string password)
        {
            User? user = await DbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
            if (token != user.Token)
            {
                response2.StatusCode = 401;
                response2.Message = "Invalid/expired token. Login First";
                response2.Success = false;
                return response2;
            }
            byte[] phash = user.PasswordHash;
            if (!_secondaryAuthService.VerifyPasswordHash(password, phash)){
                response2.Success = false;
                response2.Message = "Invalid Password";
                response2.StatusCode = 400;
                return response2;
            }

            if (user != null && user.IsDeleted == false)
            {
                user.IsDeleted = true;
                user.Token= string.Empty;
                await DbContext.SaveChangesAsync();

                response2.StatusCode = 200;
                response2.Message = "User deleted successfully";
                response2.Success = true;
                return response2;
            }
            else
            {
                response2.StatusCode = 404;
                response2.Message = "User Not found";
                response2.Success = false;
                return response2;
            }

        }*/
    }
}
