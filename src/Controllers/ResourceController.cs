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

                var list = resources
                    .Select(n => n.CreateApiOutput())
                    .ToList();

                return this.Ok(list);
            }
            catch
            {
                return this.BadRequest(null);
            }
        }

        /// <summary>
        /// Get a single resource.
        /// </summary>
        /// <param name="id">Id of resource.</param>
        /// <returns>Resource.</returns>
        [HttpGet]
        [Route("{id}")]
        [VerifyAuthorization]
        public async Task<ActionResult> Get(string id)
        {
            try
            {
                var resource = await new DatabaseContext()
                    .Resources
                    .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                              n.Identifier == id);

                if (resource == null)
                {
                    return this.NotFound(null);
                }

                return this.Ok(resource.CreateApiOutput());
            }
            catch
            {
                return this.BadRequest(null);
            }
        }
    }
}