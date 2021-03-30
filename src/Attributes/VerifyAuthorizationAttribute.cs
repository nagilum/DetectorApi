using DetectorApi.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DetectorApi.Attributes
{
    public class VerifyAuthorizationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Validate the users authorization.
        /// </summary>
        /// <param name="context">Current executing context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var b64 = context.HttpContext.Request.Cookies["AccessToken"];

                if (b64 == null)
                {
                    throw new Exception("Missing access token cookie.");
                }

                var bytes = Convert.FromBase64String(b64);
                var token = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                var user = new DatabaseContext()
                    .Users
                    .ToList()
                    .FirstOrDefault(n => n.VerifyToken(token));

                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                context.HttpContext.Items.Add(
                    "DbUser",
                    user);
            }
            catch
            {
                this.ReturnUnauthorized(context);
            }
        }

        /// <summary>
        /// Return an empty unauthorized response.
        /// </summary>
        /// <param name="context">Current executing context.</param>
        /// <param name="content">Object to serialize back to client.</param>
        private void ReturnUnauthorized(ActionExecutingContext context, object content = null)
        {
            content ??= new object();

            context.Result = new ContentResult
            {
                Content = JsonSerializer.Serialize(content),
                ContentType = "application/json"
            };

            context.HttpContext.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
        }
    }
}