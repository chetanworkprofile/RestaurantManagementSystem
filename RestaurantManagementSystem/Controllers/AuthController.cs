using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Models.InputModels;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Services;

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

    }
}
