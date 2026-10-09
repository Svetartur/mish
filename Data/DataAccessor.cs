using ASP_P42.Data.Entities;
using ASP_P42.Models.Admin;
using ASP_P42.Models.User;
using ASP_P42.Services.Kdf;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ASP_P42.Data
{
    public class DataAccessor(DataContext dataContext, IKdfService kdfService)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;

        public async Task<Guid> GetDbIdentityAsync()
        {
            return await _dataContext.Database
                .SqlQuery<Guid>($"SELECT NEWID() AS Value")
                .SingleAsync();
        }

        public List<Entities.ProductGroup> GetAllProductGroups(bool isIncludeHidden = false)
        {
            IQueryable<Entities.ProductGroup> query = _dataContext.ProductGroups.AsNoTracking();
            if (!isIncludeHidden)
            {
                query = query.Where(g => g.IsHidden == 0);
            }
            return [.. query.OrderBy(g => g.OrderInPrice)];
        }

        public async Task<Entities.ProductGroup?> GetProductGroupById(Guid guid)
        {
            return await _dataContext.ProductGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guid);
        }

        public async Task<Entities.ProductGroup?> GetProductGroupBySlug(string slug)
        {
            return await _dataContext.ProductGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Slug == slug);
        }

        public async Task<Guid> AddNewProductGroup(Entities.ProductGroup productGroup)
        {
            Guid id = await GetDbIdentityAsync();
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
            existing.IsHidden = productGroup.IsHidden;
            existing.OrderInPrice = productGroup.OrderInPrice;
            existing.ParentId = productGroup.ParentId;
            if (!string.IsNullOrEmpty(productGroup.ImageUrl))
            {
                existing.ImageUrl = productGroup.ImageUrl;
            }

            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductGroup(Guid id)
        {
            var existing = await _dataContext.ProductGroups.FindAsync(id);
            if (existing == null) return false;

            existing.IsHidden = 1;
            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<Guid> AddNewProduct(AdminAddProductFormModel formModel, String? imageUrl)
        {
            Guid id = await GetDbIdentityAsync();
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
                Guid productId = await GetDbIdentityAsync();
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
                if (_dataContext.Products.AsNoTracking().Any(p => p.Slug == formModel.Slug))
                {
                    throw new Exception($"Slug '{formModel.Slug}' is already in use by other product");
                }
            }
            return true;
        }

        public async Task<Entities.Product?> GetProductById(Guid guid)
        {
            return await _dataContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == guid);
        }

        public async Task<UserAccess?> AuthenticateUserAsync(string login, string password)
        {
            UserAccess? userAccess = await _dataContext
                .UserAccesses
                .Include(ua => ua.UserData)
                .Include(ua => ua.UserRole)
                .AsNoTracking()
                .FirstOrDefaultAsync(ua => ua.Login == login);

            String? candidateDk = null;
            bool isOk = false;
            if (userAccess != null)
            {
                candidateDk = _kdfService.Dk(password, userAccess.Salt);
                isOk = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(candidateDk),
                    Encoding.UTF8.GetBytes(userAccess.Dk));
            }

            _dataContext.AuthJournals.Add(new()
            {
                Id = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                Login = login,
                Dk = candidateDk == null
                    ? String.Empty
                    : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(candidateDk))),
                IsOk = isOk,
            });
            await _dataContext.SaveChangesAsync();

            return isOk ? userAccess : null;
        }

        public async Task<UserAccess> RegisterUserAsync(UserSignupFormModel formModel)
        {
            if (await _dataContext.UserAccesses.AsNoTracking().AnyAsync(ua => ua.Login == formModel.Login))
            {
                throw new Exception($"Login '{formModel.Login}' is already in use");
            }

            Guid userId = await GetDbIdentityAsync();
            UserData userData = new()
            {
                Id = userId,
                FullName = formModel.FullName,
                Email = formModel.Email,
                Phone = formModel.Phone,
                RegisteredAt = DateTime.Now,
                Birthdate = default,
            };
            String salt = Guid.NewGuid().ToString();
            UserAccess userAccess = new()
            {
                Id = await GetDbIdentityAsync(),
                UserId = userId,
                RoleId = await _dataContext.UserRoles
                    .Where(r => r.Name == "User")
                    .Select(r => r.Id)
                    .FirstAsync(),
                Login = formModel.Login,
                Salt = salt,
                Dk = _kdfService.Dk(formModel.Password, salt),
                UserData = userData,
            };
            _dataContext.UserAccesses.Add(userAccess);
            await _dataContext.SaveChangesAsync();

            return userAccess;
        }
    }
}
