using DetectorApi.Attributes;
using DetectorApi.Database;
using DetectorApi.Database.Tables;
using DetectorApi.Exceptions;
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
            try
            {
                await using var db = new DatabaseContext();

                Resource resource = null;

                if (resourceId != null)
                {
                    resource = await db.Resources
                        .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                                  n.Identifier == resourceId);

                    if (resource == null)
                    {
                        throw new NotFoundResponseException();
                    }
                }

                var query = db.Alerts
                    .OrderByDescending(n => n.Created)
                    .AsQueryable();

                if (resource != null)
                {
                    query = query.Where(n => n.ResourceId == resource.Id);
                }

                var alerts = await query.ToListAsync();

                return this.Ok(alerts);
            }
            catch (NotFoundResponseException)
            {
                return this.NotFound(null);
            }
            catch
            {
                return this.BadRequest(null);
            }
        }
    }
}