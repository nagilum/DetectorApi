using DetectorApi.Attributes;
using DetectorApi.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DetectorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        /// <summary>
        /// Get a list of all alerts.
        /// </summary>
        /// <param name="resourceId">Resource to filter by.</param>
        /// <returns>List of alerts.</returns>
        [HttpGet]
        [VerifyAuthorization]
        public async Task<ActionResult> GetAll([FromQuery] string resourceId = null)
        {
            if (resourceId == null)
            {
                return this.BadRequest(new
                {
                    message = "The query-parameter 'resourceId' is required"
                });
            }

            try
            {
                await using var db = new DatabaseContext();

                var resource = await db.Resources
                    .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                              n.Identifier == resourceId);

                if (resource == null)
                {
                    return this.NotFound(null);
                }

                var alerts = await db.Alerts
                    .Where(n => n.ResourceId == resource.Id)
                    .OrderByDescending(n => n.Created)
                    .ToListAsync();

                var list = alerts
                    .Select(n => n.CreateApiOutput())
                    .ToList();

                return this.Ok(list);
            }
            catch
            {
                return this.BadRequest(null);
            }
        }
    }
}