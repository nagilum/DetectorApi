using DetectorApi.Attributes;
using DetectorApi.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using DetectorApi.Exceptions;

namespace DetectorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultController : ControllerBase
    {
        /// <summary>
        /// Get a list of all scan results.
        /// </summary>
        /// <param name="resourceId">Resource to filter by.</param>
        /// <returns>List of scan results.</returns>
        [HttpGet]
        [VerifyAuthorization]
        public async Task<ActionResult> GetAll([FromQuery] string resourceId = null)
        {
            if (resourceId == null)
            {
                throw new BadRequestResponseException("The query-parameter 'resourceId' is required");
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

                var results = await db.ScanResults
                    .Where(n => n.ResourceId == resource.Id)
                    .OrderByDescending(n => n.Created)
                    .ToListAsync();

                var list = results
                    .Select(n => n.CreateApiOutput())
                    .ToList();

                return this.Ok(list);
            }
            catch (BadRequestResponseException ex)
            {
                return this.BadRequest(new
                {
                    message = ex.Message
                });
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