using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Controllers;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models.OutputModels;
using System.Net;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Object = System.Object;

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
            _secondaryAuthService = new SecondaryAuthService(configuration, dbContext);
        }

        public Object CreateUser(RegisterUser inpUser, out int code)
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
                    response2 = new ResponseWithoutData(400, "Please Enter Valid Name", false);
                    code = 400;
                    return response2;
                }
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
                tokenUser = new CreateToken(Guid.NewGuid(),inpUser.firstName, inpUser.email, "user");
                // create token to return after successful registration
                string token = _secondaryAuthService.CreateToken(tokenUser);
                //create new user object to add into database
                var user = new User(tokenUser.userId, inpUser.firstName, inpUser.lastName, inpUser.email, inpUser.phone, "user" , inpUser.address, _secondaryAuthService.CreatePasswordHash(inpUser.password), inpUser.pathToProfilePic, token);

                DbContext.Users.Add(user);
                DbContext.SaveChanges();

                //response object
                RegistrationLoginResponse data = new RegistrationLoginResponse(user.userId, user.email, user.firstName, user.lastName, user.userRole,token);
                response = new Response(200, "User added Successfully", data, true);
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

        public Object Login(UserDTO request, out int code)
        {
            //-----------------------------------------------------------------------------------------------------------------//
            //-----------------model validations--------------------------------------//
            string regexPatternEmail = "^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$";
            if (!Regex.IsMatch(request.email, regexPatternEmail))
            {
                response2 = new ResponseWithoutData(400, "Please Enter Valid Email", false);
                code = 400;
                return response2;
            }
            //int index = details.Teacher.FindIndex(t => t.Username == request.Username);
            var user = DbContext.Users.Where(u => u.email == request.email).FirstOrDefault();
            /*if(user.isBlocked)
            {
                response2 = new ResponseWithoutData(401, "You are blocked. Please contact administrator", false);
                code = 401;
                return response2;
            }*/
            if (user == null)
            {
                response2 = new ResponseWithoutData(404, "User not found", false);
                code = 404;
                return response2;
            }
            else if (request.password == null)
            {
                response2 = new ResponseWithoutData(400, "Null/Wrong password", false);
                code = 400;
                return response2;
            }
            else if (!_secondaryAuthService.VerifyPasswordHash(request.password, user.passwordHash))
            {
                response2 = new ResponseWithoutData(400, "Wrong password.", false);
                code = 400;
                return response2;
            }
            //-----------------------------------------------------------------------------------------------------------------//

            //creating token
            tokenUser = new CreateToken(user.userId, user.firstName, user.email, user.userRole);
            string token = _secondaryAuthService.CreateToken(tokenUser);
            user.token = token;

            DbContext.SaveChanges();            // save into database

            RegistrationLoginResponse data = new RegistrationLoginResponse(user.userId, user.email, user.firstName, user.lastName, user.userRole, token);
            response = new Response(200, "Login Successful", data, true);
            code = 200;
            return response;
        }

        public Object ForgetPassword(string email, out int code)
        {
            try
            {
                string regexPatternEmail = "^[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,4}$";
                if (!Regex.IsMatch(email, regexPatternEmail))
                {
                    response2 = new ResponseWithoutData(400, "Please Enter Valid Email", false);
                    code = 400;
                    return response2;
                }
                //find user in database
                var user = DbContext.Users.Where(u => u.email == email).FirstOrDefault();
                bool exists = DbContext.Users.Where(u => u.email == email).Any();

                if (!exists || user == null)            //retrun if user doesn't exist
                {
                    response2 = new ResponseWithoutData(404, "User not found", false);
                    code = 404;
                    return response2;
                }

                //generate random otp 
                Random random = new Random();
                int otp = random.Next(100000, 999999);

                //save otp in database
                user.verificationOTP = otp;
                user.otpUsableTill = DateTime.Now.AddHours(1);               // otp check valid for 1 hour only
                user.token = string.Empty;                                  //clear token from database

                //send mail function used to send mail 
                response2 = _secondaryAuthService.SendEmail(email, otp);
                DbContext.SaveChanges();

                // generate token used for reseting password can't user this token to login
                var tokenUser = new CreateToken(user.userId, user.firstName, user.email, "resetpassword");

                string returntoken = _secondaryAuthService.CreateToken(tokenUser);
                //response object
                if (response2.statusCode == 200)
                {
                    RegistrationLoginResponse data = new RegistrationLoginResponse(user.userId, user.email, user.firstName, user.lastName, user.userRole, returntoken);
                    response = new Response(200, response2.message, data, true);
                    code = 200;
                    return response;
                }
                code = response2.statusCode;
                return response2;
            }
            catch (Exception ex)
            {
                response2 = new ResponseWithoutData(500, "Invalid Mail or " + ex.Message, false);
                code = 500;
                return response2;
            }
        }

        public Object Verify(ResetPasswordModel r, string userId, out int code)
        {
            //this api function is used after forget password to verify user and help user reset his/her password
            //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.email == email);
            Guid id = new Guid(userId);
            var user =  DbContext.Users.Find(id);
            //Console.WriteLine(user);
            if (user == null)               //check if email exists in database
            {
                response2 = new ResponseWithoutData(404, "User not found", false);
                code = 404;
                return response2;
            }
            if (r.OTP != user.verificationOTP)//(user == null)
            {
                response2 = new ResponseWithoutData(400, "Invalid verification Value/Otp", false);
                code = 400;
                return response2;
            }
            if (user.otpUsableTill < DateTime.Now)           // checks if otp is expired or not
            {
                response2 = new ResponseWithoutData(400, "OTP Expired", false);
                code = 400;
                return response2;
            }
            user.verifiedAt = DateTime.UtcNow;
            result = ResetPassword(r.Password, id, out code);
            user.otpUsableTill = DateTime.Now;
            DbContext.SaveChanges();
            return result;
        }

        internal Object ResetPassword(string password, Guid id, out int code)
        {
            //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            var user = DbContext.Users.Find(id);

            //password validation
            string regexPatternPassword = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
            if (!Regex.IsMatch(password, regexPatternPassword))
            {
                response2 = new ResponseWithoutData(400, "Please Enter Valid Password. Must contain atleast one uppercase letter, one lowercase letter, one number and one special chararcter and must be atleast 8 characters long", false);
                code = 400;
                return response2;
            }
            try
            {
                //creating new password hash
                byte[] pass = _secondaryAuthService.CreatePasswordHash(password);
                user.passwordHash = pass;

                //create token
                tokenUser = new CreateToken(user.userId, user.firstName, user.email, user.userRole);
                string token = _secondaryAuthService.CreateToken(tokenUser);

                user.token = token;
                code = 200;
                DbContext.SaveChanges();

                var responsedata = new RegistrationLoginResponse(user.userId, user.email, user.firstName, user.lastName, user.userRole, token);
                response = new Response(200, "Password Reset was successful", responsedata, true);
                return response;
            }
            catch (Exception ex)
            {
                response2 = new ResponseWithoutData(500, ex.Message, false);
                code = 500;
                return response;
            }
        }

        public Object ChangePassword(ChangePasswordModel r, string userId, string token, out int code)
        {
            //var user = await DbContext.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            Guid id = new Guid(userId);
            var user = DbContext.Users.Find(id);
            //var PasswordHash = CreatePasswordHash(r.oldPassword);
            if (token != user.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                code = 401;
                return response2;
            }
            if (user == null)
            {
                response2 = new ResponseWithoutData(404, "User not found", false);
                code = 404;
                return response2;
            }
            string regexPatternPassword = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
            if (!Regex.IsMatch(r.Password, regexPatternPassword))
            {
                response2 = new ResponseWithoutData(400, "Enter Valid Password. Must contain atleast one uppercase letter, one lowercase letter, one number and one special chararcter and must be atleast 8 characters long", false);
                code = 400;
                return response2;
            }
            if (!_secondaryAuthService.VerifyPasswordHash(r.oldPassword, user.passwordHash))
            {
                response2 = new ResponseWithoutData(400, "Invalid old password.", false);
                code = 400;
                return response2;
            }

            try
            {
                byte[] pass = _secondaryAuthService.CreatePasswordHash(r.Password);
                user.passwordHash = pass;

                tokenUser = new CreateToken(user.userId, user.firstName, user.email, user.userRole);

                /*string token = CreateToken(tokenUser);
                user.Token = token;*/
                DbContext.SaveChanges();
                var responsedata = new RegistrationLoginResponse(user.userId, user.email, user.firstName, user.lastName, user.userRole, user.token);

                response = new Response(200, "Password change successful", responsedata, true);
                code = 200;
                return response;
            }
            catch (Exception ex)
            {
                response2 = new ResponseWithoutData(500, ex.Message, false);
                code = 500;
                return response2;
            }
        }

        public Object Logout(string userId, string token, out int code)
        {
            Guid id = new Guid(userId);
            var user = DbContext.Users.Find(id);

            if (user == null)
            {
                response2 = new ResponseWithoutData(404, "User not found", false);
                code = 404;
                return response2;
            }
            if (token != user.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                code = 401;
                return response2;
            }
            try
            {
                // remove token from database
                user.token = string.Empty;
                DbContext.SaveChanges();
                var responsedata = new RegistrationLoginResponse(user.userId, user.email, user.firstName, user.lastName, user.userRole, token);

                response2 = new ResponseWithoutData(200, "User Logged out Successfully", true);
                code = 200;
                return response2;
            }
            catch (Exception ex)
            {
                response2 = new ResponseWithoutData(500, ex.Message, false);
                code= 500;
                return response2;
            }
        }

    }
}
