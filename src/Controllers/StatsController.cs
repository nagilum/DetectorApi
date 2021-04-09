using DetectorApi.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading.Tasks;

namespace DetectorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        /// <summary>
        /// Get various stats for the app.
        /// </summary>
        /// <returns>Stats and stuff.</returns>
        [HttpGet]
        public async Task<ActionResult> GetStats()
        {
            // Get app version.
            string version = null;

            try
            {
                version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            }
            catch
            {
                //
            }

            // Get number of resources.
            var resourceCount = 0;

            try
            {
                resourceCount = await new DatabaseContext()
                    .Resources
                    .CountAsync(n => !n.Deleted.HasValue);
            }
            catch
            {
                //
            }

            // Get number of open issues.
            var openIssueCount = 0;

            try
            {
                openIssueCount = await new DatabaseContext()
                    .Issues
                    .CountAsync(n => !n.Resolved.HasValue);
            }
            catch
            {
                //
            }

            // Create API output.
            return this.Ok(new
            {
                app = new
                {
                    version
                },
                stats = new
                {
                    resourceCount,
                    openIssueCount
                }
            });
        }
    }
}