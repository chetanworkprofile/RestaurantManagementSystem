using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.FileProviders;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Models.OutputModels;
using System.Net;

namespace RestaurantManagementSystem.Services
{
    public class UploadPicService : IUploadPicService
    {
        Response response;
        ResponseWithoutData response2 = new ResponseWithoutData();
        private readonly RestaurantDbContext DbContext;
        private readonly IConfiguration _configuration;

        public UploadPicService(IConfiguration configuration, RestaurantDbContext dbContext)
        {
            response = new Response();
            this._configuration = configuration;
            DbContext = dbContext;
        }

        public async Task<object> ProfilePicUploadAsync(IFormFile file, string userId, string token, string userRole)
        {
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets","ProfilePics");
                string path1 = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "ProfilePics","user");
                string path2 = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "ProfilePics", "chef");
                string path3 = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "ProfilePics", "admin");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(path1);
                }
                if (!Directory.Exists(path2))
                {
                    Directory.CreateDirectory(path2);
                }
                if (!Directory.Exists(path3))
                {
                    Directory.CreateDirectory(path3);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Guid id = new Guid(userId);
            var user = await DbContext.Users.FindAsync(id);
            var folderName = Path.Combine("Assets", "ProfilePics",userRole);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            if (token != user.token)
            {
                response2 = new ResponseWithoutData(401, "Invalid/expired token. Login First", false);
                return response2;
            }
            if (file == null)
            {
                response2 = new ResponseWithoutData(400, "Please provide a file for successful upload", false);
                return response2;
            }
            if (file.Length > 0)
            {
                var fileName = string.Concat(
                                    user.email,
                                    DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                    Path.GetFileNameWithoutExtension(file.FileName),
                                    Path.GetExtension(file.FileName)
                                    );

                var fullPath = Path.Combine(pathToSave, fileName);

                using (var stream = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(stream);
                }
                user.pathToProfilePic = Path.Combine(folderName, fileName);
                await DbContext.SaveChangesAsync();

                ResponseUser responseUser = new ResponseUser(user.userId, user.firstName, user.lastName, user.email, user.phone, user.userRole, user.address, user.pathToProfilePic, user.createdAt, user.updatedAt);
                PicUploadResponse data = new PicUploadResponse()
                {
                    User = responseUser,
                    FileName = fileName,
                    PathToPic = Path.Combine(folderName, fileName)
                };
                response = new Response(200, "File Uploaded Successfully", data, true);
                return response;
            }
            response2 = new ResponseWithoutData(400, "Please provide a file for successful upload", false);
            return response2;
        }
    }
}
