using DetectorApi.Attributes;
using DetectorApi.Core;
using DetectorApi.Database;
using DetectorApi.Database.Tables;
using DetectorApi.Payloads;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetectorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Verify the given Google credentials and log the user in.
        /// </summary>
        /// <param name="payload">Credentials from Google one-tap login.</param>
        /// <returns>API token.</returns>
        [HttpPost]
        public async Task<ActionResult> VerifyGoogleCredentials([FromBody] AuthPostPayload payload)
        {
            if (payload?.Credentials == null)
            {
                return this.Unauthorized(null);
            }

            var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={payload.Credentials}";

            try
            {
                if (!(WebRequest.Create(url) is HttpWebRequest req))
                {
                    throw new Exception($"Unable to create HttpWebRequest for {url}");
                }

                req.Method = "GET";
                req.UserAgent = "Detector/1.0.0";
                req.Accept = "application/json";

                if (!(req.GetResponse() is HttpWebResponse res))
                {
                    throw new Exception($"Unable to get HttpWebResponse from HttpWebRequest for {url}");
                }

                var stream = res.GetResponseStream();
                var json = await new StreamReader(stream).ReadToEndAsync();

                var info = JsonSerializer.Deserialize<GoogleTokenInfo>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // Valid response?
                if (info?.email == null ||
                    info.name == null ||
                    info.picture == null)
                {
                    return this.Unauthorized(null);
                }

                // Restrict based on e-mail domains?
                var domains = Config.GetStrings("auth", "options", "restrictUserDomains");

                if (domains?.Length > 0)
                {
                    var found = false;

                    foreach (var domain in domains)
                    {
                        if (info.email.EndsWith(domain))
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        return this.Unauthorized(null);
                    }
                }

                // Create/get user from db.
                await using var db = new DatabaseContext();

                var created = DateTimeOffset.Now;

                var user = await db.Users
                               .FirstOrDefaultAsync(n => n.Email == info.email.ToLower()) ??
                           new User
                           {
                               Created = created,
                               Email = info.email.ToLower(),
                               Name = info.name,
                               PictureUrl = info.picture
                           };

                user.Updated = created;

                if (user.Id == 0)
                {
                    await db.Users.AddAsync(user);
                }

                await db.SaveChangesAsync();

                // Generate token.
                var token = BCrypt.Net.BCrypt.HashPassword(user.CompileTokenContent());

                // Return to user as cookie.
                var bytes = Encoding.UTF8.GetBytes(token);
                var b64 = Convert.ToBase64String(bytes, 0, bytes.Length);

                this.Response.Cookies.Append(
                    "AccessToken",
                    b64,
                    new CookieOptions
                    {
                        Secure = true,
                        HttpOnly = true,
                        Expires = DateTimeOffset.Now.AddDays(3),
                        Path = "/",
                        SameSite = SameSiteMode.Strict
                    });

                return this.Ok();
            }
            catch
            {
                return this.Unauthorized(null);
            }
        }

        /// <summary>
        /// Get user info based on passed cookie.
        /// </summary>
        /// <returns>Info about the user.</returns>
        [HttpGet]
        [VerifyAuthorization]
        public ActionResult GetUserInfo()
        {
            if (!(this.HttpContext.Items["DbUser"] is User user))
            {
                return this.Unauthorized(null);
            }

            return this.Ok(user);
        }

        /// <summary>
        /// Log the user out by deleting the cookie.
        /// </summary>
        [HttpDelete]
        [VerifyAuthorization]
        public ActionResult LogOut()
        {
            this.Response.Cookies.Delete("AccessToken");

            return this.Ok();
        }
    }
}