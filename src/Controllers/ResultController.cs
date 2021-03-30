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
    public class ResultController : ControllerBase
    {
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

                var results = await db.ScanResults
                    .Where(n => n.ResourceId == resource.Id)
                    .OrderByDescending(n => n.Created)
                    .ToListAsync();

                var list = results
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