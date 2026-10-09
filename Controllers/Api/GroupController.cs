using ASP_P42.Data;
using ASP_P42.Data.Entities;
using ASP_P42.Models.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ASP_P42.Controllers.Api
{
    [Route("api/group")]
    [ApiController]
    public class GroupController(DataContext dataContext, DataAccessor dataAccessor) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly DataAccessor _dataAccessor = dataAccessor;

        [HttpGet]
        public async Task<IActionResult> GetAllGroups(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return Respond(RestStatus.BadRequest, new { message = "Page must be positive and pageSize must be between 1 and 100." });
            }

            var query = _dataContext.ProductGroups
                .AsNoTracking()
                .Include(group => group.Children.Where(child => child.IsHidden == 0))
                .Where(group => group.IsHidden == 0 && group.ParentId == null)
                .OrderBy(group => group.OrderInPrice);

            int totalItems = await query.CountAsync();
            var groups = await query
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToListAsync();

            RestMetaPagination pagination = new()
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
            };

            return Respond(RestStatus.Ok, groups.Select(ToPublicResponse).ToArray(), pagination);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetGroup(String slug)
        {
            var group = await _dataContext.ProductGroups
                .AsNoTracking()
                .Include(item => item.Children.Where(child => child.IsHidden == 0))
                .FirstOrDefaultAsync(item => item.IsHidden == 0 && item.Slug == slug);

            return group == null
                ? Respond(RestStatus.NotFound)
                : Respond(RestStatus.Ok, ToPublicResponse(group));
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] RestProductGroupRequest request)
        {
            String? validationError = Validate(request);
            if (validationError != null)
            {
                return Respond(RestStatus.BadRequest, new { message = validationError });
            }

            if (await _dataAccessor.GetProductGroupBySlug(request.Slug) != null)
            {
                return Respond(RestStatus.Conflict, new { message = $"Group slug '{request.Slug}' is already in use." });
            }

            if (request.ParentId.HasValue &&
                !await _dataContext.ProductGroups.AsNoTracking().AnyAsync(group => group.Id == request.ParentId.Value))
            {
                return Respond(RestStatus.BadRequest, new { message = "Parent group was not found." });
            }

            ProductGroup group = new()
            {
                ParentId = request.ParentId,
                Name = request.Name,
                Description = request.Description,
                Slug = request.Slug,
                ImageUrl = request.ImageUrl ?? String.Empty,
                IsHidden = request.IsHidden,
                OrderInPrice = request.OrderInPrice,
            };
            await _dataAccessor.AddNewProductGroup(group);

            return CreatedAtAction(
                nameof(GetGroup),
                new { slug = group.Slug },
                CreateResponse(RestStatus.Created, ToPublicResponse(group)));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] RestProductGroupRequest request)
        {
            String? validationError = Validate(request);
            if (validationError != null)
            {
                return Respond(RestStatus.BadRequest, new { message = validationError });
            }

            ProductGroup? existing = await _dataAccessor.GetProductGroupById(id);
            if (existing == null)
            {
                return Respond(RestStatus.NotFound);
            }

            var duplicateSlug = await _dataAccessor.GetProductGroupBySlug(request.Slug);
            if (duplicateSlug != null && duplicateSlug.Id != id)
            {
                return Respond(RestStatus.Conflict, new { message = $"Group slug '{request.Slug}' is already in use." });
            }

            if (request.ParentId == id)
            {
                return Respond(RestStatus.BadRequest, new { message = "A group cannot be its own parent." });
            }

            if (request.ParentId.HasValue &&
                !await _dataContext.ProductGroups.AsNoTracking().AnyAsync(group => group.Id == request.ParentId.Value))
            {
                return Respond(RestStatus.BadRequest, new { message = "Parent group was not found." });
            }

            existing.ParentId = request.ParentId;
            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Slug = request.Slug;
            existing.IsHidden = request.IsHidden;
            existing.OrderInPrice = request.OrderInPrice;
            if (request.ImageUrl != null)
            {
                existing.ImageUrl = request.ImageUrl;
            }

            await _dataAccessor.UpdateProductGroup(existing);
            return Respond(RestStatus.Ok, ToPublicResponse(existing));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            bool deleted = await _dataAccessor.DeleteProductGroup(id);
            return deleted
                ? Respond(RestStatus.Ok, new { id, deleted = true })
                : Respond(RestStatus.NotFound);
        }

        private String? Validate(RestProductGroupRequest request)
        {
            if (request == null)
            {
                return "Group data is required.";
            }

            if (String.IsNullOrWhiteSpace(request.Name))
            {
                return "Group name is required.";
            }

            if (request.Description == null || request.Slug == null)
            {
                return "Group description and slug are required.";
            }

            request.Name = request.Name.Trim();
            request.Description = request.Description.Trim();
            request.Slug = request.Slug.Trim();

            if (request.Name.Length < 2 || request.Name.Length > 200)
            {
                return "Group name must contain between 2 and 200 characters.";
            }

            if (!Regex.IsMatch(request.Name, @"^[\w\s\-\u0400-\u04FF]+$"))
            {
                return "Group name contains invalid characters.";
            }

            if (request.Description.Length > 500)
            {
                return "Group description cannot exceed 500 characters.";
            }

            if (!Regex.IsMatch(request.Slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            {
                return "Group slug must contain lowercase letters, digits, and hyphens.";
            }

            if (request.IsHidden is not 0 and not 1)
            {
                return "IsHidden must be 0 or 1.";
            }

            return null;
        }

        private RestProductGroup ToPublicResponse(ProductGroup group)
        {
            return ToResponse(group) with
            {
                ImageUrl = FullImageUrl(group.ImageUrl),
                Children = [.. group.Children.Select(child => ToResponse(child) with
                {
                    ImageUrl = FullImageUrl(child.ImageUrl),
                })],
            };
        }

        private static RestProductGroup ToResponse(ProductGroup group)
        {
            return new()
            {
                Id = group.Id,
                ParentId = group.ParentId,
                Name = group.Name,
                Description = group.Description,
                Slug = group.Slug,
                ImageUrl = group.ImageUrl,
                IsHidden = group.IsHidden,
                OrderInPrice = group.OrderInPrice,
                Children = [],
            };
        }

        private String? FullImageUrl(String? url)
        {
            if (String.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return url;
            }

            String path = url.StartsWith('/')
                ? url
                : Url.Action("Image", "Storage", new { id = url }) ?? $"/Storage/Image/{Uri.EscapeDataString(url)}";
            return $"{Request.Scheme}://{Request.Host}{path}";
        }

        private IActionResult Respond(RestStatus status, Object? data = null, RestMetaPagination? pagination = null)
        {
            return StatusCode(status.Code, CreateResponse(status, data, pagination));
        }

        private RestResponse CreateResponse(RestStatus status, Object? data = null, RestMetaPagination? pagination = null)
        {
            return new()
            {
                Status = status,
                Meta = new()
                {
                    ApiName = "Product Groups",
                    DataType = "application/json",
                    Manipulations = ["GET", "POST", "PUT", "DELETE"],
                    Links = new()
                    {
                        ["self"] = $"{Request.Scheme}://{Request.Host}{Request.Path}",
                        ["collection"] = $"{Request.Scheme}://{Request.Host}/api/group",
                    },
                    Pagination = pagination,
                },
                Data = data,
            };
        }
    }
}
