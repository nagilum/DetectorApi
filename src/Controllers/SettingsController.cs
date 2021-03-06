using DetectorApi.Core;
using Microsoft.AspNetCore.Mvc;

namespace DetectorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        /// <summary>
        /// Get common settings.
        /// </summary>
        /// <returns>A list of settings.</returns>
        [HttpGet]
        public ActionResult GetSettings()
        {
            // Build API response.
            return this.Ok(new
            {
                auth = new
                {
                    google = new
                    {
                        clientId = Config.Get("auth", "google", "clientId")
                    }
                }
            });
        }
    }
}