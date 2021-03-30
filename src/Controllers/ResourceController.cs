using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DetectorApi.Attributes;
using DetectorApi.Database;
using System.Linq;
using System.Threading.Tasks;

namespace DetectorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceController : ControllerBase
    {
        /// <summary>
        /// Get a list of all resources.
        /// </summary>
        /// <returns>List of resources.</returns>
        [HttpGet]
        [VerifyAuthorization]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var resources = await new DatabaseContext()
                    .Resources
                    .Where(n => !n.Deleted.HasValue)
                    .OrderBy(n => n.Name)
                    .ToListAsync();

                return this.Ok(resources);
            }
            catch
            {
                return this.BadRequest(null);
            }
        }
    }
}