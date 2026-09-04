using ASP_P42.Data;
using ASP_P42.Data.Entities;
using ASP_P42.Models.User;
using ASP_P42.Services.Kdf;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_P42.Controllers
{
    public class UserController(
        DataContext dataContext,
        IKdfService kdfService
    ) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;

        public IActionResult SignUp([FromBody] UserSignupFormModel formModel)
        {
            if (formModel == null)
            {
                return BadRequest("Data structure non-bindable to model");
            }
            if (!formModel.IsAgree)
            {
                return BadRequest("You should confirm site policy (agreement)");
            }
            String requiredMessage = " could not be empty";
            if (String.IsNullOrEmpty(formModel.Login))
            {
                return BadRequest(nameof(formModel.Login) + requiredMessage);
            }
            if (String.IsNullOrEmpty(formModel.FullName))
            {
                return BadRequest(nameof(formModel.FullName) + requiredMessage);
            }
            if (String.IsNullOrEmpty(formModel.Email))
            {
                return BadRequest(nameof(formModel.Email) + requiredMessage);
            }
            if (String.IsNullOrEmpty(formModel.Password))
            {
                return BadRequest(nameof(formModel.Password) + requiredMessage);
            }
            if (formModel.Password != formModel.Repeat)
            {
                return BadRequest("Password and Repeat mismatch");
            }

            formModel.FullName = formModel.FullName.Trim();
            if (formModel.FullName.Length < 2)
            {
                return BadRequest(nameof(formModel.FullName) + " too short (2 symbols at least)");
            }
            formModel.Login = formModel.Login.Trim();
            if (formModel.Login.Length < 2)
            {
                return BadRequest(nameof(formModel.Login) + " too short (2 symbols at least)");
            }
            if (formModel.Login.Contains(':'))
            {
                return BadRequest(nameof(formModel.Login) + " could not contain colon (':')");
            }
            formModel.Email = formModel.Email.Trim();
            if (!Regex.IsMatch(
                formModel.Email,
                @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"
            ))
            {
                return BadRequest(nameof(formModel.Email) + " has invalid format");
            }

            if (!String.IsNullOrEmpty(formModel.Phone))
            {
                formModel.Phone = formModel.Phone.Trim();
                if (!Regex.IsMatch(formModel.Phone, @"^0\d{9}$"))
                {
                    return BadRequest(nameof(formModel.Phone) + " must consist of 10 digits starting with 0 (e.g. 0987654321)");
                }
            }

            if (!Regex.IsMatch(formModel.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$"))
            {
                return BadRequest(nameof(formModel.Password) + " does not meet complexity requirements (at least 6 chars, with uppercase, lowercase, and digits)");
            }

            if (_dataContext.UserAccesses.Any(ua => ua.Login == formModel.Login))
            {
                return BadRequest(nameof(formModel.Login) + $" '{formModel.Login}' is already in use");
            }

            Guid userId = Guid.NewGuid();
            _dataContext.UsersData.Add(new()
            {
                Id = userId,
                FullName = formModel.FullName,
                Email = formModel.Email,
                Phone = formModel.Phone,
                RegisteredAt = DateTime.Now,
                Birthdate = default,
            });
            String salt = Guid.NewGuid().ToString();
            _dataContext.UserAccesses.Add(new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = _dataContext.UserRoles.First(r => r.Name == "User").Id,
                Login = formModel.Login,
                Salt = salt,
                Dk = _kdfService.Dk(formModel.Password, salt),
            });
            _dataContext.SaveChanges();
            return Json(formModel);
        }

        public IActionResult BasicAuth()
        {
            UserAccess? userAccess;
            try
            {
                userAccess = AuthenticateUser();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (userAccess == null)
            {
                return Unauthorized(
                    "Credentials rejected: check login and password");
            }

            HttpContext.Session.SetString(
                "userAccessId",
                userAccess.Id.ToString()
            );
            return Ok();
        }

        public IActionResult BasicAuthJwt()
        {
            UserAccess? userAccess;
            try
            {
                userAccess = AuthenticateUser();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (userAccess == null)
            {
                return Unauthorized(
                    "Credentials rejected: check login and password");
            }

            var header = new
            {
                alg = "HS256",
                typ = "JWT"
            };
            long time = (DateTime.Now.Ticks - DateTime.UnixEpoch.Ticks) / 100000;
            var payload = new
            {
                sub = userAccess.Login,
                iat = time,
                exp = time + 1000000,
                name = userAccess.UserData.FullName,
                email = userAccess.UserData.Email
            };
            String body = Base64UrlTextEncoder.Encode(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(header)))
                + "." +
                Base64UrlTextEncoder.Encode(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(payload)));

            String signature = Base64UrlTextEncoder.Encode(
                System.Security.Cryptography.HMACSHA256.HashData(
                    Encoding.UTF8.GetBytes("secret"),
                    Encoding.UTF8.GetBytes(body)
            ));

            return Ok(body + "." + signature);
        }

        private UserAccess? AuthenticateUser()
        {
            String authHeader = HttpContext.Request.Headers.Authorization.ToString();
            if (authHeader == String.Empty)
            {
                throw new Exception("Missing Authorization header");
            }
            String scheme = "Basic ";
            if (!authHeader.StartsWith(scheme))
            {
                throw new Exception("Authorization scheme must be 'Basic'");
            }
            String credentials = authHeader[scheme.Length..];
            byte[] rawData;
            try
            {
                rawData = Convert.FromBase64String(credentials);
            }
            catch
            {
                throw new Exception(
                    "Authorization credentials must be valid Base64::section 4");
            }
            String userPass;
            try
            {
                userPass = Encoding.UTF8.GetString(rawData);
            }
            catch
            {
                throw new Exception(
                    "User-pass must be valid UTF8 string");
            }
            String[] parts = userPass.Split(':', 2);
            if (parts.Length != 2)
            {
                throw new Exception(
                    "User-pass must be concatenated by ':'");
            }
            String login = parts[0];
            String password = parts[1];

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
    }
}
