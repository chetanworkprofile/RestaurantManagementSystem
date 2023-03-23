using Microsoft.IdentityModel.Tokens;
using RestaurantManagementSystem.Controllers;
using RestaurantManagementSystem.Models.OutputModels;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Data;

namespace RestaurantManagementSystem.Services
{
    public class SecondaryAuthService
    {
        Response response = new Response();
        ResponseWithoutData response2 = new ResponseWithoutData();
        CreateToken tokenUser = new CreateToken();
        object result = new object();
        private readonly RestaurantDbContext DbContext;
        private readonly IConfiguration _configuration;

        public SecondaryAuthService(IConfiguration configuration, RestaurantDbContext dbContext, ILogger<AuthController> logger)
        {
            this._configuration = configuration;
            DbContext = dbContext;
        }
        internal ResponseWithoutData SendEmail(string email, int value)
        {
            try
            {
                MailMessage message = new MailMessage();

                // set the sender and recipient email addresses
                message.From = new MailAddress("verification@chatapp.chicmic.co.in");
                message.Subject = "Mail Verification by ChatApplication. Verify your account";
                message.To.Add(new MailAddress(email));

                // set the subject and body of the email
                //message.Subject = "Verify your account";
                message.Body = "Please verify your reset password attempt. Your One Time Password for verification is " + value;             /*"Please click on the following link to verify your account: http://192.180.2.159:4040/api/v1/verify?email=" + email+"&value"+value;    //verificationCode;*/

                // create a new SmtpClient object
                SmtpClient client = new SmtpClient();

                // set the SMTP server credentials and port
                client.Credentials = new NetworkCredential(_configuration.GetSection("MailCredentials:email").Value!, _configuration.GetSection("MailCredentials:password").Value!);
                client.Host = "mail.chicmic.co.in";
                client.Port = 587;
                client.EnableSsl = true;

                // send the email
                client.Send(message);
                response2.statusCode = 200;
                response2.message = "Verification Email Sent successfully";
                //response.Data = string.Empty;
                response2.success = true;
                return response2;
            }
            catch (Exception ex)
            {
                response2.statusCode = 500;
                response2.message = ex.Message;
                //response.Data = ex.Data;
                Console.WriteLine(ex);
                response2.success = false;
                return response2;
            }
        }

        //function used to create token 
        public string CreateToken(CreateToken user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid,user.userId.ToString()),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Name, user.firstName),
                new Claim(ClaimTypes.Role,user.role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        //used to create password hash that is stored in database
        public byte[] CreatePasswordHash(string password)
        {
            byte[] salt = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Password_Salt").Value!);
            byte[] passwordHash;
            using (var hmac = new HMACSHA512(salt))
            {
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
            return passwordHash;
        }

        //to verify password with hash stored in database
        public bool VerifyPasswordHash(string password, byte[] passwordHash)
        {
            byte[] salt = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Password_Salt").Value!);
            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
