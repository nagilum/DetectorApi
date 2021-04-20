using DetectorApi.Attributes;
using DetectorApi.Database;
using DetectorApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DetectorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        /// <summary>
        /// Get all log messages attached to a resource.
        /// </summary>
        /// <param name="resourceId">Id of resource.</param>
        /// <param name="limit">Limit the number of log entries returned.</param>
        /// <returns>List of logs.</returns>
        [HttpGet]
        [VerifyAuthorization]
        public async Task<ActionResult> GetAll([FromQuery] string resourceId = null, [FromQuery] int? limit = null)
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
                    throw new NotFoundResponseException();
                }

                var logs = await db.Logs
                    .Where(n => n.ReferenceType == "resource" &&
                                n.ReferenceId == resource.Id)
                    .OrderByDescending(n => n.Created)
                    .ToListAsync();

                if (limit.HasValue)
                {
                    logs = logs
                        .Take(limit.Value)
                        .ToList();
                }

                return this.Ok(logs);
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