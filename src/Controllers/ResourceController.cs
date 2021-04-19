using DetectorApi.Attributes;
using DetectorApi.Core;
using DetectorApi.Database;
using DetectorApi.Database.Tables;
using DetectorApi.Exceptions;
using DetectorApi.Payloads;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetectorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceController : ControllerBase
    {
        /// <summary>
        /// Create a new resource.
        /// </summary>
        /// <param name="payload">Resource info.</param>
        /// <returns>Resource.</returns>
        [HttpPost]
        [VerifyAuthorization]
        public async Task<ActionResult> Create([FromBody] ResourcePostPayload payload)
        {
            if (!(this.HttpContext.Items["DbUser"] is User user))
            {
                return this.Unauthorized(null);
            }

            try
            {
                if (payload.Name == null ||
                    payload.Url == null)
                {
                    throw new BadRequestResponseException("Name and/or URL cannot be blank");
                }

                await using var db = new DatabaseContext();

                var resource = await db.Resources
                    .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                              n.Name == payload.Name &&
                                              n.Url == payload.Url);

                if (resource != null)
                {
                    throw new BadRequestResponseException("A resource with the same name and URL already exists");
                }

                var id = Guid.NewGuid().ToString().Substring(0, 8);

                while (true)
                {
                    if (await db.Resources.CountAsync(n => n.Identifier == id) == 0)
                    {
                        break;
                    }

                    id = Guid.NewGuid().ToString().Substring(0, 8);
                }

                resource = new Resource
                {
                    Created = DateTimeOffset.Now,
                    Updated = DateTimeOffset.Now,
                    Identifier = id,
                    Name = payload.Name,
                    Url = payload.Url
                };

                await db.Resources.AddAsync(resource);
                await db.SaveChangesAsync();

                await Log.LogInformation(
                    "Resource created.",
                    user.Id,
                    "resource",
                    resource.Id);

                return this.Ok(resource);
            }
            catch (BadRequestResponseException ex)
            {
                return this.BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch
            {
                return this.BadRequest(null);
            }
        }

        /// <summary>
        /// Mark a resource as deleted.
        /// </summary>
        /// <param name="id">Id of resource.</param>
        [HttpDelete]
        [Route("{id}")]
        [VerifyAuthorization]
        public async Task<ActionResult> Delete([FromRoute] string id)
        {
            if (!(this.HttpContext.Items["DbUser"] is User user))
            {
                return this.Unauthorized(null);
            }

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

                resource.Updated = DateTimeOffset.Now;
                resource.Deleted = DateTimeOffset.Now;

                var issues = await db.Issues
                    .Where(n => n.ResourceId == resource.Id)
                    .ToListAsync();

                var alerts = await db.Alerts
                    .Where(n => n.ResourceId == resource.Id)
                    .ToListAsync();

                db.Issues.RemoveRange(issues);
                db.Alerts.RemoveRange(alerts);

                await db.SaveChangesAsync();

                await Log.LogWarning(
                    "Resource deleted.",
                    user.Id,
                    "resource",
                    resource.Id);

                return this.Ok(null);
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

        /// <summary>
        /// Get a single resource.
        /// </summary>
        /// <param name="id">Id of resource.</param>
        /// <returns>Resource.</returns>
        [HttpGet]
        [Route("{id}")]
        [VerifyAuthorization]
        public async Task<ActionResult> Get([FromRoute] string id)
        {
            try
            {
                var resource = await new DatabaseContext()
                    .Resources
                    .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                              n.Identifier == id);

                if (resource == null)
                {
                    throw new NotFoundResponseException();
                }

                return this.Ok(resource);
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

        /// <summary>
        /// Update an existing resource.
        /// </summary>
        /// <param name="id">Id of resource.</param>
        /// <param name="payload">Resource info.</param>
        [HttpPost("{id}")]
        [VerifyAuthorization]
        public async Task<ActionResult> Update([FromRoute] string id, [FromBody] ResourcePostPayload payload)
        {
            if (!(this.HttpContext.Items["DbUser"] is User user))
            {
                return this.Unauthorized(null);
            }

            try
            {
                if (payload.Name == null ||
                    payload.Url == null)
                {
                    throw new BadRequestResponseException("Name and/or URL cannot be blank");
                }

                var changes = new List<ChangeEntry>();

                await using var db = new DatabaseContext();

                var resource = await db.Resources
                    .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                              n.Identifier == id);

                if (resource == null)
                {
                    throw new NotFoundResponseException();
                }

                if (resource.Name != payload.Name)
                {
                    changes.Add(
                        new ChangeEntry
                        {
                            PropertyName = "Name",
                            OldValue = resource.Name,
                            NewValue = payload.Name
                        });
                }

                if (resource.Url != payload.Url)
                {
                    changes.Add(
                        new ChangeEntry
                        {
                            PropertyName = "URL",
                            OldValue = resource.Url,
                            NewValue = payload.Url
                        });
                }

                resource.Updated = DateTimeOffset.Now;
                resource.Name = payload.Name;
                resource.Url = payload.Url;

                await db.SaveChangesAsync();

                var message = "Resource updated.";

                if (changes.Any())
                {
                    message += " " + string.Join(", ", changes);
                }

                await Log.LogInformation(
                    message,
                    user.Id,
                    "resource",
                    resource.Id);

                return this.Ok(null);
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