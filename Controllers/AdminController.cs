using ASP_P42.Data;
using ASP_P42.Models.Admin;
using ASP_P42.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ASP_P42.Controllers
{
    public class AdminController(IStorageService storageService, DataAccessor dataAccessor) : Controller
    {
        private readonly IStorageService _storageService = storageService;
        private readonly DataAccessor _dataAccessor = dataAccessor;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Product()
        {
            AdminGroupViewModel viewModel = new()
            {
                Groups = _dataAccessor.GetAllProductGroups(),
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(AdminAddProductFormModel formModel)
        {
            try
            {
                if (formModel == null)
                {
                    throw new Exception("Product data missing");
                }

                if (String.IsNullOrWhiteSpace(formModel.Name) || formModel.Name.Trim().Length < 2)
                {
                    throw new Exception("Product name must be at least 2 characters long");
                }
                formModel.Name = formModel.Name.Trim();
                if (!Regex.IsMatch(formModel.Name, @"^[\w\s\-\u0400-\u04FF]+$"))
                {
                    throw new Exception("Product name contains invalid special characters");
                }

                if (!String.IsNullOrEmpty(formModel.Description) && formModel.Description.Length > 1000)
                {
                    throw new Exception("Product description cannot exceed 1000 characters");
                }

                if (!String.IsNullOrEmpty(formModel.Slug))
                {
                    formModel.Slug = formModel.Slug.Trim();
                    if (!Regex.IsMatch(formModel.Slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
                    {
                        throw new Exception("Product slug must contain only lowercase letters, digits, and hyphens");
                    }
                }

                if (formModel.Stock < -1)
                {
                    throw new Exception("Product stock must be a non-negative integer or -1");
                }

                if (formModel.Price < 0.01)
                {
                    throw new Exception("Product price must be greater than or equal to 0.01");
                }

                await _dataAccessor.IsProductFormModelValidAsync(formModel);

                String? imageUrl = null;
                if (formModel.Image != null)
                {
                    imageUrl = _storageService.Save(formModel.Image);
                }

                await _dataAccessor.AddNewProduct(formModel, imageUrl);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }

        public IActionResult Group()
        {
            AdminGroupViewModel viewModel = new()
            {
                Groups = _dataAccessor.GetAllProductGroups(),
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddGroup(AdminAddGroupFormModel formModel)
        {
            try
            {
                if (formModel == null)
                {
                    throw new Exception("Group data missing");
                }

                if (String.IsNullOrWhiteSpace(formModel.Name) || formModel.Name.Trim().Length < 2)
                {
                    throw new Exception("Group name must be at least 2 characters long");
                }
                formModel.Name = formModel.Name.Trim();
                if (!Regex.IsMatch(formModel.Name, @"^[\w\s\-\u0400-\u04FF]+$"))
                {
                    throw new Exception("Group name contains invalid special characters");
                }

                if (!String.IsNullOrEmpty(formModel.Description) && formModel.Description.Length > 500)
                {
                    throw new Exception("Group description cannot exceed 500 characters");
                }

                if (String.IsNullOrWhiteSpace(formModel.Slug))
                {
                    throw new Exception("Group slug cannot be empty");
                }
                formModel.Slug = formModel.Slug.Trim();
                if (!Regex.IsMatch(formModel.Slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
                {
                    throw new Exception("Group slug must contain only lowercase letters, digits, and hyphens");
                }
                if ((await _dataAccessor.GetProductGroupBySlug(formModel.Slug)) != null)
                {
                    throw new Exception($"Group slug '{formModel.Slug}' is already in use");
                }

                String? imageUrl = null;
                if (formModel.Image != null)
                {
                    imageUrl = "/storage/image/" + _storageService.Save(formModel.Image);
                }

                await _dataAccessor.AddNewProductGroup(new()
                {
                    ParentId = formModel.ParentId,
                    Name = formModel.Name,
                    Description = formModel.Description,
                    Slug = formModel.Slug,
                    IsHidden = formModel.IsHidden,
                    OrderInPrice = formModel.Order,
                    ImageUrl = imageUrl ?? ""
                });
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
