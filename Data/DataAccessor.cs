using ASP_P42.Data.Entities;
using ASP_P42.Models.Admin;
using ASP_P42.Models.User;
using ASP_P42.Services.Kdf;
using Microsoft.EntityFrameworkCore;

namespace ASP_P42.Data
{
    public class DataAccessor(DataContext dataContext, IKdfService kdfService)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;

        public Guid GetDbIdentity()
        {
            try
            {
                return _dataContext.Database.SqlQuery<Guid>($"SELECT NEWID() AS [Value]").AsEnumerable().First();
            }
            catch
            {
                return Guid.NewGuid();
            }
        }

        public List<Entities.ProductGroup> GetAllProductGroups(bool isIncludeHidden = false)
        {
            IQueryable<Entities.ProductGroup> query = _dataContext.ProductGroups;
            if (!isIncludeHidden)
            {
                query = query.Where(g => g.IsHidden == 0);
            }
            return [.. query.OrderBy(g => g.OrderInPrice)];
        }

        public async Task<Entities.ProductGroup?> GetProductGroupById(Guid guid)
        {
            return await _dataContext.ProductGroups.FirstOrDefaultAsync(g => g.Id == guid);
        }

        public async Task<Entities.ProductGroup?> GetProductGroupBySlug(string slug)
        {
            return await _dataContext.ProductGroups.FirstOrDefaultAsync(g => g.Slug == slug);
        }

        public async Task<Guid> AddNewProductGroup(Entities.ProductGroup productGroup)
        {
            Guid id = GetDbIdentity();
            productGroup.Id = id;
            _dataContext.ProductGroups.Add(productGroup);
            await _dataContext.SaveChangesAsync();
            return id;
        }

        public async Task<bool> UpdateProductGroup(Entities.ProductGroup productGroup)
        {
            var existing = await _dataContext.ProductGroups.FindAsync(productGroup.Id);
            if (existing == null) return false;

            existing.Name = productGroup.Name;
            existing.Description = productGroup.Description;
            existing.Slug = productGroup.Slug;
            existing.ImageUrl = productGroup.ImageUrl;
            existing.IsHidden = productGroup.IsHidden;
            existing.OrderInPrice = productGroup.OrderInPrice;
            existing.ParentId = productGroup.ParentId;

            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductGroup(Guid id)
        {
            var existing = await _dataContext.ProductGroups.FindAsync(id);
            if (existing == null) return false;

            _dataContext.ProductGroups.Remove(existing);
            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<Guid> AddNewProduct(AdminAddProductFormModel formModel, String? imageUrl)
        {
            Guid id = GetDbIdentity();
            if (formModel.ProductId != null)
            {
                _dataContext.ProductVersions.Add(new()
                {
                    Id = id,
                    ProductId = (await GetProductById(formModel.ProductId.Value))!.Id,
                    ImageUrl = imageUrl,
                    Price = (decimal)formModel.Price,
                    Stock = formModel.Stock,
                    OrderInPrice = formModel.Order,
                    Slug = formModel.Slug,
                    IsHidden = formModel.IsHidden,
                    Version = formModel.Name
                });
            }
            else
            {
                Entities.ProductGroup group = (await GetProductGroupById(formModel.GroupId))!;
                Guid productId = GetDbIdentity();
                _dataContext.Products.Add(new()
                {
                    Id = productId,
                    GroupId = group.Id,
                    Name = formModel.Name,
                    Description = formModel.Description,
                    ImageUrl = imageUrl,
                    IsHidden = formModel.IsHidden,
                    OrderInPrice = formModel.Order,
                    Slug = formModel.Slug,
                });
                _dataContext.ProductVersions.Add(new()
                {
                    Id = id,
                    ProductId = productId,
                    ImageUrl = imageUrl,
                    Price = (decimal)formModel.Price,
                    Stock = formModel.Stock,
                    OrderInPrice = 1,
                    Slug = formModel.Slug,
                    IsHidden = formModel.IsHidden,
                });
            }

            await _dataContext.SaveChangesAsync();
            return id;
        }

        public async Task<bool> IsProductFormModelValidAsync(AdminAddProductFormModel formModel)
        {
            var group = await GetProductGroupById(formModel.GroupId)
                   ?? throw new Exception($"Product group not found with id='{formModel.GroupId}'");
            if (formModel.ProductId != null)
            {
                _ = await GetProductById(formModel.ProductId.Value)
                ?? throw new Exception($"Product not found with id='{formModel.ProductId}'");
            }
            if (formModel.Slug != null)
            {
                if (_dataContext.Products.Any(p => p.Slug == formModel.Slug))
                {
                    throw new Exception($"Slug '{formModel.Slug}' is already in use by other product");
                }
            }
            return true;
        }

        public async Task<Entities.Product?> GetProductById(Guid guid)
        {
            return await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == guid);
        }

        public UserAccess? AuthenticateUser(string login, string password)
        {
            if (_dataContext
                .UserAccesses
                .Include(ua => ua.UserData)
                .Include(ua => ua.UserRole)
                .AsNoTracking()
                .FirstOrDefault(ua => ua.Login == login)
                is UserAccess userAccess)
            {
                String dk = _kdfService.Dk(password, userAccess.Salt);
                if (dk == userAccess.Dk)
                {
                    return userAccess;
                }
            }
            return null;
        }

        public UserAccess RegisterUser(UserSignupFormModel formModel)
        {
            if (_dataContext.UserAccesses.Any(ua => ua.Login == formModel.Login))
            {
                throw new Exception($"Login '{formModel.Login}' is already in use");
            }

            Guid userId = GetDbIdentity();
            UserData userData = new()
            {
                Id = userId,
                FullName = formModel.FullName,
                Email = formModel.Email,
                Phone = formModel.Phone,
                RegisteredAt = DateTime.Now,
                Birthdate = default,
            };
            _dataContext.UsersData.Add(userData);

            String salt = Guid.NewGuid().ToString();
            UserAccess userAccess = new()
            {
                Id = GetDbIdentity(),
                UserId = userId,
                RoleId = _dataContext.UserRoles.First(r => r.Name == "User").Id,
                Login = formModel.Login,
                Salt = salt,
                Dk = _kdfService.Dk(formModel.Password, salt),
                UserData = userData,
            };
            _dataContext.UserAccesses.Add(userAccess);
            _dataContext.SaveChanges();

            return userAccess;
        }
    }
}
