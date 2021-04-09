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
    public class IssueController : ControllerBase
    {
        /// <summary>
        /// Get a list of all issue.
        /// </summary>
        /// <param name="resourceId">Resource to filter by.</param>
        /// <returns>List of issues.</returns>
        [HttpGet]
        [VerifyAuthorization]
        public async Task<ActionResult> GetAll([FromQuery] string resourceId)
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

                var issues = await db.Issues
                    .Where(n => n.ResourceId == resource.Id)
                    .OrderByDescending(n => n.Created)
                    .ToListAsync();

                return this.Ok(issues);
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