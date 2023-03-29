using RestaurantManagementSystem.Controllers;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Models.InputModels;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace RestaurantManagementSystem.Services
{
    public class AdminService : IAdminService
    {
        Response response = new Response();                             //response models/objects
        ResponseWithoutData response2 = new ResponseWithoutData();
        CreateToken tokenUser = new CreateToken();              // model to create token
        object result = new object();
        private readonly RestaurantDbContext DbContext;
        private readonly IConfiguration _configuration;

        // secondary service file to make code clean
        SecondaryAuthService _secondaryAuthService;

        //constructor
        public AdminService(IConfiguration configuration, RestaurantDbContext dbContext, ILogger<AdminController> logger)
        {
            this._configuration = configuration;
            DbContext = dbContext;
            _secondaryAuthService = new SecondaryAuthService(configuration, dbContext);
        }

        public Object AddChef(RegisterUser inpUser, out int code)
        {
            var DbUsers = DbContext.Users;
            bool existingUser = DbUsers.Where(u => u.email == inpUser.email).Any();
            if (!existingUser)
            {
                //-----------------------------------------------------------------------------------------------------------------//
                //-----------------model validations--------------------------------------//

                /*string regexPatternFirstName = "^[A-Z][a-zA-Z]*$";
                if (!Regex.IsMatch(inpUser.firstName, regexPatternFirstName))
                {
                    response2 = new ResponseWithoutData(400, "Please Enter Valid Name", false);
                    code = 400;
                    return response2;
                }*/
                string regexPatternEmail = "^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$";
                if (!Regex.IsMatch(inpUser.email, regexPatternEmail))
                {
                    response2 = new ResponseWithoutData(400, "Please Enter Valid Email", false);
                    code = 400;
                    return response2;
                }
                string regexPatternPassword = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
                if (!Regex.IsMatch(inpUser.password, regexPatternPassword))
                {
                    response2 = new ResponseWithoutData(400, "Please Enter Valid Password. Must contain atleast one uppercase letter, one lowercase letter, one number and one special chararcter and must be atleast 8 characters long", false);
                    code = 400;
                    return response2;
                }
                string regexPatternPhone = "^[6-9]\\d{9}$";
                if (!Regex.IsMatch(inpUser.phone.ToString(), regexPatternPhone))
                {
                    response2 = new ResponseWithoutData(400, "Please Enter Valid PhoneNo", false);
                    code = 400;
                    return response2;
                }

                //-----------------------------------------------------------------------------------------------------------------//
                tokenUser = new CreateToken(Guid.NewGuid(), inpUser.firstName, inpUser.email, "chef");
                // create token to return after successful registration
                string token = _secondaryAuthService.CreateToken(tokenUser);
                //create new user object to add into database
                var user = new User(tokenUser.userId, inpUser.firstName, inpUser.lastName, inpUser.email, inpUser.phone, "chef" , inpUser.address, _secondaryAuthService.CreatePasswordHash(inpUser.password), inpUser.pathToProfilePic, token);

                DbContext.Users.Add(user);
                DbContext.SaveChanges();

                //response object
                RegistrationLoginResponse data = new RegistrationLoginResponse(user.userId, user.email, user.firstName, user.lastName, user.userRole, token);
                response = new Response(200, "Chef added Successfully", data, true);
                code = 200;
                return response;
            }
            else
            {
                response2 = new ResponseWithoutData(400, "Email already registered please try another", false);
                code = 400;
                return response2;
            }
        }

        public Object GetUsers(string userId, string userType,string token, Guid? UserId, string? searchString, string? Email, long Phone, string OrderBy, int SortOrder, int RecordsPerPage, int PageNumber, out int code)          // sort order   ===   e1 for ascending   -1 for descending
        {
            //get logged in user from database
            Guid id = new Guid(userId);
            var userLoggedIn = DbContext.Users.Find(id);
            var userss = DbContext.Users.AsQueryable();

            //userss = userss.Where(x => (x.UserId == UserId || UserId == null) && (x.IsDeleted == false) && (EF.Functions.Like(x.FirstName, "%" + searchString + "%") || EF.Functions.Like(x.LastName, "%" + searchString + "%") || searchString == null) &&
            //(x.Email == Email || Email == null)).Select(x => x);

            userss = userss.Where(t => t.isDeleted == false);     //remove deleted users from list

            if (userType != "all")
            {
                userss = userss.Where(t => t.userRole == userType);
            }

            if (token != userLoggedIn.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                code = 401;
                return response2;
            }

            //--------------------------filtering based on userId,searchString, Email, or Phone---------------------------------//

            if (UserId != null) { userss = userss.Where(s => (s.userId == UserId)); }
            if (searchString != null) { userss = userss.Where(s => EF.Functions.Like(s.firstName, "%" + searchString + "%") || EF.Functions.Like(s.lastName, "%" + searchString + "%") || EF.Functions.Like(s.firstName + " " + s.lastName, "%" + searchString + "%")); }
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
            else if (OrderBy == "FirstName" || OrderBy == "Name" || OrderBy == "firstname" || OrderBy == "firstName")
            {
                orderBy = x => x.firstName;
            }
            else if (OrderBy == "LastName" || OrderBy == "lastname" || OrderBy == "lastName")
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
                ResponseUser r = new ResponseUser(user.userId, user.firstName, user.lastName, user.email, user.phone, user.userRole, user.address, user.pathToProfilePic, user.createdAt, user.updatedAt);
                res.Add(r);
            }

            if (!res.Any())
            {
                response2 = new ResponseWithoutData(404, "No User found.", true);
                code= 404;
                return response2;
            }
            response = new Response(200, "Users list fetched", res, true);
            code = 200;
            return response;
        }

        public Object DeleteUser(string userId, string token, out int code)
        {
            Guid id = new Guid(userId);
            User? user = DbContext.Users.Find(id);
            User admin = DbContext.Users.Where(s=> (s.userRole == "admin")).First();
            if (token != admin.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                code = 401;
                return response2;
            }

            if (user != null && user.isDeleted == false)
            {
                user.isDeleted = true;
                user.token = string.Empty;
                DbContext.SaveChanges();

                response2 = new ResponseWithoutData(200, "User/Chef deleted successfully", true);
                code = 200;
                return response2;
            }
            else
            {
                response2 = new ResponseWithoutData(404, "User/Chef Not found", false);
                code = 404;
                return response2;
            }

        }
    }
}
