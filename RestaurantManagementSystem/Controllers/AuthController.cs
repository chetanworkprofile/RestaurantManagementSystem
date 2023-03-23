﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Security.Claims;

namespace RestaurantManagementSystem.Controllers
{
    //auth controller to handle all authentication related works like register,login, logout, reset password etc.
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IAuthService authService;               //service dependency
        Response response = new Response();     //response model instance
        ResponseWithoutData response2 = new ResponseWithoutData();      //response model in case we don't return data
        object result = new object();                                   //object to match both response models in return values from function
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, RestaurantDbContext dbContext, ILogger<AuthController> logger)          //constructor
        {
            authService = new AuthService(configuration, dbContext, logger);
            _logger = logger;
        }

        [HttpPost]
        [Route("/api/v1/user/register")]
        public IActionResult RegisterUser([FromBody] RegisterUser inpUser)             //register user function uses authService to create a new user in db
        {
            if (!ModelState.IsValid)
            {   //checks for validation of model
                response2.statusCode = 400;
                response2.message = "Invalid Input/One or more fields are invalid";
                response2.success = false;
                return BadRequest(response2);
            }
            try
            {
                _logger.LogInformation("User registration attempt");
                result = authService.CreateUser(inpUser).Result; ;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                //ResponseWithoutData response = new ResponseWithoutData();
                response2.statusCode = 500;
                response2.message = ex.Message;
                response2.success = false;
                return StatusCode(500, response2);
            }
        }

        [HttpPost("/api/v1/user/login")]
        public ActionResult<User> UserLogin(UserDTO request)
        {
            _logger.LogInformation("User Login attempt");
            try
            {
                result = authService.Login(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2.statusCode = 500;
                response2.message = ex.Message;
                response2.success = false;
                return StatusCode(500, response2);
            }
        }

        [HttpPost]
        [Route("/api/v1/forgetPassword")]
        public ActionResult<User> ForgetPassword(string Email)
        {
            _logger.LogInformation("forget password attempt");
            try
            {
                result = authService.ForgetPassword(Email).Result;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2.statusCode = 500;
                response2.message = ex.Message;
                response2.success = false;
                return StatusCode(500, response2);
            }
        }

        [HttpPost, Authorize(Roles = "resetpassword")]
        [Route("/api/v1/resetPassword")]
        public ActionResult<User> Verify(ResetPasswordModel r)
        {
            _logger.LogInformation("verification attempt");
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.Sid);                  //extracting userid from token
                result = authService.Verify(r, userId).Result;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2.statusCode = 500;
                response2.message = ex.Message;
                response2.success = false;
                return StatusCode(500, response);
            }
        }

        [HttpPost, Authorize(Roles = "user")]
        [Route("/api/v1/changePassword")]
        public ActionResult<User> ChangePasswod(ChangePasswordModel r)
        {
            _logger.LogInformation("reset password attempt");
            try
            {
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();        //getting token from header
                /*var user = HttpContext.User;
                string email = user.FindFirst(ClaimTypes.Email)?.Value;*/
                string? userId = User.FindFirstValue(ClaimTypes.Sid);
                result = authService.ChangePassword(r, userId, token).Result;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2.statusCode = 500;
                response2.message = ex.Message;
                response2.success = false;
                return StatusCode(500, response2);
            }
        }

        [HttpPost, Authorize(Roles = "login")]
        [Route("/api/v1/user/logout")]
        public ActionResult<User> Logout()
        {
            _logger.LogInformation("user logout attempt");
            try
            {
                string? userId = User.FindFirstValue(ClaimTypes.Sid);
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                result = authService.Logout(userId, token).Result;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2.statusCode = 500;
                response2.message = ex.Message;
                response2.success = false;
                return StatusCode(500, response2);
            }
        }

    }
}
