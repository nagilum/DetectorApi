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
                    Active = true,
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
        /// Bulk add resources.
        /// </summary>
        /// <param name="urls">List of URLs to add.</param>
        /// <returns>Success.</returns>
        [HttpPost]
        [Route("bulk")]
        [VerifyAuthorization]
        public async Task<ActionResult> CreateBulk([FromBody] List<string> urls)
        {
            if (!(this.HttpContext.Items["DbUser"] is User user))
            {
                return this.Unauthorized(null);
            }

            try
            {
                await using var db = new DatabaseContext();

                var alreadyExists = new List<string>();
                var added = new List<string>();

                foreach (var url in urls)
                {
                    var actUrl = url
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Trim();

                    var resource = await db.Resources
                        .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                                  n.Url == actUrl);

                    if (resource != null)
                    {
                        alreadyExists.Add(actUrl);
                        continue;
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
                        Active = true,
                        Identifier = id,
                        Name = actUrl,
                        Url = actUrl
                    };

                    await db.Resources.AddAsync(resource);
                    await db.SaveChangesAsync();

                    await Log.LogInformation(
                        "Resource created.",
                        user.Id,
                        "resource",
                        resource.Id);

                    added.Add(actUrl);
                }

                return this.Ok(new
                {
                    added,
                    alreadyExists
                });
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
        /// Mark multiple resources as deleted, in bulk.
        /// </summary>
        /// <param name="idList">List of resource Ids.</param>
        /// <returns>Success.</returns>
        [HttpDelete]
        [Route("bulk/{idList}")]
        [VerifyAuthorization]
        public async Task<ActionResult> DeleteBulk([FromRoute] string idList)
        {
            if (!(this.HttpContext.Items["DbUser"] is User user))
            {
                return this.Unauthorized(null);
            }

            try
            {
                var notFound = new List<string>();
                var deleted = new List<string>();

                await using var db = new DatabaseContext();

                var ids = idList.Split(',');

                foreach (var id in ids)
                {
                    var resource = await db.Resources
                        .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                                  n.Identifier == id);

                    if (resource == null)
                    {
                        notFound.Add(id);
                        continue;
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

                    deleted.Add(id);
                }

                return this.Ok(new
                {
                    deleted,
                    notFound
                });
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
        /// Toggle active on multiple resources in bulk.
        /// </summary>
        /// <param name="idList">List of resource Ids.</param>
        /// <returns>Success.</returns>
        [HttpPost]
        [Route("toggle-active/bulk/{idList}")]
        [VerifyAuthorization]
        public async Task<ActionResult> ToggleActive([FromRoute] string idList)
        {
            if (!(this.HttpContext.Items["DbUser"] is User user))
            {
                return this.Unauthorized(null);
            }

            try
            {
                var notFound = new List<string>();
                var updated = new List<string>();

                await using var db = new DatabaseContext();

                var ids = idList.Split(',');

                foreach (var id in ids)
                {
                    var resource = await db.Resources
                        .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                                  n.Identifier == id);

                    if (resource == null)
                    {
                        notFound.Add(id);
                        continue;
                    }

                    if (resource.Active.HasValue)
                    {
                        resource.Active = !resource.Active.Value;
                    }
                    else
                    {
                        resource.Active = false;
                    }

                    await db.SaveChangesAsync();

                    var changes = new List<ChangeEntry>
                    {
                        new()
                        {
                            PropertyName = "Active",
                            OldValue = resource.Active?.ToString(),
                            NewValue = resource.Active.Value.ToString()
                        }
                    };

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

                    updated.Add(id);
                }

                return this.Ok(new
                {
                    updated,
                    notFound
                });
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
                var changes = new List<ChangeEntry>();

                await using var db = new DatabaseContext();

                var resource = await db.Resources
                    .FirstOrDefaultAsync(n => !n.Deleted.HasValue &&
                                              n.Identifier == id);

                if (resource == null)
                {
                    throw new NotFoundResponseException();
                }

                if (!string.IsNullOrWhiteSpace(payload.Name) &&
                    resource.Name != payload.Name)
                {
                    changes.Add(
                        new ChangeEntry
                        {
                            PropertyName = "Name",
                            OldValue = resource.Name,
                            NewValue = payload.Name
                        });

                    resource.Name = payload.Name;
                }

                if (!string.IsNullOrWhiteSpace(payload.Url) &&
                    resource.Url != payload.Url)
                {
                    changes.Add(
                        new ChangeEntry
                        {
                            PropertyName = "URL",
                            OldValue = resource.Url,
                            NewValue = payload.Url
                        });

                    resource.Url = payload.Url;
                }

                if (payload.Active.HasValue &&
                    payload.Active != resource.Active)
                {
                    changes.Add(
                        new ChangeEntry
                        {
                            PropertyName = "Active",
                            OldValue = resource.Active?.ToString(),
                            NewValue = payload.Active.Value.ToString()
                        });

                    resource.Active = payload.Active;
                }

                if (changes.Any())
                {
                    resource.Updated = DateTimeOffset.Now;
                }

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