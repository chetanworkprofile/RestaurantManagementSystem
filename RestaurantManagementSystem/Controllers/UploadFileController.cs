using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models.OutputModels;
using RestaurantManagementSystem.Services;
using System.Data;
using System.Security.Claims;

namespace RestaurantManagementSystem.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        IUploadPicService uploadPicServiceInstance;      //service dependency
        private readonly ILogger<UploadFileController> _logger;
        object result = new object();
        ResponseWithoutData response2 = new ResponseWithoutData();
        Response response = new Response();
        public UploadFileController(ILogger<UploadFileController> logger, IConfiguration configuration, RestaurantDbContext dbContext)
        {
            uploadPicServiceInstance = new UploadPicService(configuration, dbContext);
            _logger = logger;
        }

        [HttpPost, DisableRequestSizeLimit, Authorize(Roles = "user,chef,admin")]
        [Route("/api/v1/uploadProfilePic")]
        public async Task<IActionResult> ProfilePicUploadAsync(IFormFile file)                //[FromForm] FileUpload File
        {
            _logger.LogInformation("Pic Upload method started");
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.Sid);
                string userRole = User.FindFirstValue(ClaimTypes.Role);
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                result = await uploadPicServiceInstance.ProfilePicUploadAsync(file, userId, token,userRole);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        }

        /*[HttpPost, DisableRequestSizeLimit, Authorize(Roles = "user")]
        [Route("/api/v1/uploadFile")]
        public async Task<IActionResult> FileUploadAsync(int type, IFormFile file)
        {
            //type 2 is for image and save in images folder and type 2 is for file to save in files folder
            _logger.LogInformation("File/Image Upload method started");
            try
            {
                string? email = User.FindFirstValue(ClaimTypes.Email);                                //extracting email from header token
                string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();       //getting token from authorization header
                result = await uploadPicServiceInstance.FileUploadAsync(file, email, token, type);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Internal server error ", ex.Message);
                response2 = new ResponseWithoutData(500, $"Internal server error: {ex.Message}", false);
                return StatusCode(500, response2);
            }
        }*/


    }
}
