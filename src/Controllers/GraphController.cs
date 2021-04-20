using DetectorApi.Attributes;
using DetectorApi.Database;
using DetectorApi.Exceptions;
using DetectorApi.Payloads;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetectorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        /// <summary>
        /// Get a list of all graph points for a resource.
        /// </summary>
        /// <param name="id">Resource identifier.</param>
        /// <returns>List of graph points.</returns>
        [HttpGet]
        [Route("resource/{id}")]
        [VerifyAuthorization]
        public async Task<ActionResult> GetForResource([FromRoute] string id)
        {
            try
            {
                await using var db = new DatabaseContext();

                var resource = await db.Resources
                    .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                              n.Identifier == id);

                if (resource == null)
                {
                    throw new NotFoundResponseException();
                }

                var graphData = await db.GraphData
                    .FirstOrDefaultAsync(n => n.ResourceId == resource.Id);

                var list = graphData != null
                    ? JsonSerializer.Deserialize<List<GraphPoint>>(graphData.GraphJson ?? "[]")
                    : new List<GraphPoint>();

                return this.Ok(list);
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