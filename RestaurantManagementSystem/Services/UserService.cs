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
            _secondaryAuthService = new SecondaryAuthService(configuration, dbContext);
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
