using Microsoft.AspNetCore.Identity;
using RestaurantManagementSystem.Controllers;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models.OutputModels;
using System.Net;
using System.Text.RegularExpressions;

namespace RestaurantManagementSystem.Services
{
    public class AuthService : IAuthService
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
        public AuthService(IConfiguration configuration, RestaurantDbContext dbContext, ILogger<AuthController> logger)
        {
            this._configuration = configuration;
            DbContext = dbContext;
            _secondaryAuthService = new SecondaryAuthService(configuration, dbContext, logger);
        }

        public async Task<object> CreateUser(RegisterUser inpUser)
        {
            var DbUsers = DbContext.Users;
            bool existingUser = DbUsers.Where(u => u.email == inpUser.email).Any();
            if (!existingUser)
            {
                //-----------------------------------------------------------------------------------------------------------------//
                //-----------------model validations--------------------------------------//

                string regexPatternFirstName = "^[A-Z][a-zA-Z]*$";
                if (!Regex.IsMatch(inpUser.firstName, regexPatternFirstName))
                {
                    response2.statusCode = 400;
                    response2.message = "Please Enter Valid Name";
                    response2.success = false;
                    return response2;
                }
                string regexPatternEmail = "^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$";
                if (!Regex.IsMatch(inpUser.email, regexPatternEmail))
                {
                    response2.statusCode = 400;
                    response2.message = "Please Enter Valid Email";
                    response2.success = false;
                    return response2;
                }
                string regexPatternPassword = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
                if (!Regex.IsMatch(inpUser.password, regexPatternPassword))
                {
                    response2.statusCode = 400;
                    response2.message = "Please Enter Valid Password. Must contain atleast one uppercase letter, one lowercase letter, one number and one special chararcter and must be atleast 8 characters long";
                    response2.success = false;
                    return response2;
                }
                string regexPatternPhone = "^[6-9]\\d{9}$";
                if (!Regex.IsMatch(inpUser.phone.ToString(), regexPatternPhone))
                {
                    response2.statusCode = 400;
                    response2.message = "Please Enter Valid PhoneNo";
                    response2.success = false;
                    return response2;
                }

                //-----------------------------------------------------------------------------------------------------------------//
                tokenUser = new CreateToken()
                {
                    email = inpUser.email,
                    firstName = inpUser.firstName,
                    role = "login"
                };
                // create token to return after successful registration
                string token = _secondaryAuthService.CreateToken(tokenUser);
                //create new user object to add into database
                var user = new User(inpUser.firstName, inpUser.lastName, inpUser.email, inpUser.phone, inpUser.address, _secondaryAuthService.CreatePasswordHash(inpUser.password), inpUser.pathToProfilePic, token);

                await DbContext.Users.AddAsync(user);
                await DbContext.SaveChangesAsync();

                response.statusCode = 200;
                response.message = "User added Successfully";
                //response object
                RegistrationLoginResponse data = new RegistrationLoginResponse()
                {
                    userId = user.userId,
                    email = user.email,
                    firstName = user.firstName,
                    lastName = user.lastName,
                    token = token
                };
                response.data = data;
                response.success = true;

                return response;
            }
            else
            {
                response2.statusCode = 409;
                response2.message = "Email already registered please try another";
                response2.success = false;
                return response2;
            }
        }

        public Object Login(UserDTO request)
        {
            //-----------------------------------------------------------------------------------------------------------------//
            //-----------------model validations--------------------------------------//
            string regexPatternEmail = "^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$";
            if (!Regex.IsMatch(request.email, regexPatternEmail))
            {
                response2.statusCode = 400;
                response2.message = "Please Enter Valid Email";
                response2.success = false;
                return response2;
            }
            //int index = details.Teacher.FindIndex(t => t.Username == request.Username);
            var user = DbContext.Users.Where(u => u.email == request.email).FirstOrDefault();
            if (user == null)
            {
                response2.statusCode = 404;
                response2.message = "User not found";
                response2.success = false;
                return response2;
            }
            else if (request.password == null)
            {
                response2.statusCode = 403;
                response2.message = "Null/Wrong password.";
                response2.success = false;
                return response2;
            }
            else if (!_secondaryAuthService.VerifyPasswordHash(request.password, user.passwordHash))
            {
                response2.statusCode = 403;
                response2.message = "Wrong password.";
                response2.success = false;
                return response2;
            }
            //-----------------------------------------------------------------------------------------------------------------//

            //creating token
            tokenUser.email = user.email;
            tokenUser.firstName = user.firstName;
            tokenUser.role = "login";
            string token = _secondaryAuthService.CreateToken(tokenUser);
            user.token = token;

            DbContext.SaveChanges();            // save into database

            response.statusCode = 200;
            response.message = "Login Successful";
            RegistrationLoginResponse data = new RegistrationLoginResponse()            //response model
            {
                userId = user.userId,
                email = user.email,
                firstName = user.firstName,
                lastName = user.lastName,
                token = user.token,
            };
            response.data = data;
            response.success = true;

            return response;
        }
    }
}
