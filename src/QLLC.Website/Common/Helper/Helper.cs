using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Tasin.Website.Common.Helper
{
    public static class Helper
    {
        public static Bitmap GenerateQrCode(string url)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                return qrCode.GetGraphic(20); // 20 is the pixel size
            }
        }
        public static byte[] ResizeImage(byte[] imageData, int width, int height)
        {
            using (var ms = new MemoryStream(imageData))
            {
                using (var originalImage = Image.FromStream(ms))
                {
                    var resizedImage = new Bitmap(width, height);

                    using (var graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        graphics.DrawImage(originalImage, 0, 0, width, height);
                    }

                    using (var outputMs = new MemoryStream())
                    {
                        resizedImage.Save(outputMs, originalImage.RawFormat);
                        return outputMs.ToArray();
                    }
                }
            }
        }
        
        public static string GenerateQrCodeAsBase64(string url)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);

                using (var bitmap = qrCode.GetGraphic(20))
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] imageBytes = memoryStream.ToArray();
                    return "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                }
            }
        }
        public static async Task<string> ProcessTelegramResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON string into the TelegramResponse object
                var telegramResponse = JsonSerializer.Deserialize<TelegramResponse>(jsonResponse);

                // Access the telegramChatId and other data
                string telegramChatId = telegramResponse.TelegramChatId;
                string sentMessage = telegramResponse.SentMessage;
                return telegramChatId;
                //Console.WriteLine($"Message sent successfully to Chat ID: {telegramChatId}");
                //Console.WriteLine($"Message Content: {sentMessage}");
            }
            else
            {
                // Handle errors
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<TelegramResponse>(jsonResponse);

                //Console.WriteLine($"Failed to send message: {errorResponse.ErrorMessage}");
                return "";
            }
        }

        public class LoginClaim
        {
            public int UserId { get; set; }
            public string Password { get; set; }
            public int AccountType { get; set; }
            public int? TenantId { get; set; }
            public string UserName { get; set; }
            public bool RememberMe { get; set; }
            public bool IsMobile { get; set; }
            public List<int> RoleIdList { get; set; }
            public List<int> EnumActionList { get; set; }

        }
        public class LoginClaimCustomModel
        {
            public AuthenticationProperties AuthenticationProperties { get; set; }
            public ClaimsPrincipal ClaimsPrincipal { get; set; }
            public LoginViewModel LoginViewModel { get; set; }
        }
        public static LoginClaimCustomModel GenerateLoginClaim(LoginClaim model)
        {
            // Create the identity from the user info
            var claims = new List<Claim>
                {
                    new Claim("UserID", model.UserId.ToString()),
                    new Claim("IsMobile", model.IsMobile.ToString()),
                    new Claim("TenantId", model.TenantId.HasValue ? model.TenantId.Value.ToString() : ""),
                    new Claim(ClaimTypes.Name, model.UserName),
                    new Claim("RoleIds", string.Join(",",model.RoleIdList)),
                    new Claim("Actions", string.Join(",", model.EnumActionList)),
                };

            var account = new LoginViewModel
            {
                UserName = model.UserName,
                Password = model.Password,
                AccountType = model.AccountType,
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            // setup Authenticate using the identity
            var principal = new ClaimsPrincipal(identity);
            // setup AuthenticationProperties
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddDays(1),
                IsPersistent = model.RememberMe
            };
            return new LoginClaimCustomModel()
            {
                AuthenticationProperties = authProperties,
                LoginViewModel = account,
                ClaimsPrincipal = principal
            };
        }

        public static string GenerateToken(LoginClaimCustomModel model, string tokenSecretKey)
        {
            #region Generate JWT Token
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(tokenSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                
                Subject = new ClaimsIdentity(model.ClaimsPrincipal.Claims.ToList()),
                Expires = model.AuthenticationProperties.ExpiresUtc?.UtcDateTime ?? DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var jwtToken = jwtTokenHandler.CreateToken(tokenDescriptor);
            var token = jwtTokenHandler.WriteToken(jwtToken);
            #endregion
            return token;
        }
    }

}
