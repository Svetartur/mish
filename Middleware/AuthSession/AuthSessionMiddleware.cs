using ASP_P42.Data;
using ASP_P42.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASP_P42.Middleware.AuthSession
{
    public class AuthSessionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(
            HttpContext context,
            DataContext dataContext
        )
        {
            String authKey = "userAccessId";
            if (context.Request.Query.ContainsKey("logout"))
            {
                context.Session.Remove(authKey);
                context.Response.Redirect(context.Request.Path);
                return;
            }

            context.Items.Add("itemKey", "Item Value");

            if (context.Session.Keys.Contains(authKey))
            {
                String userAccessId = context.Session.GetString(authKey)!;
                UserAccess? userAccess = dataContext
                    .UserAccesses
                    .Include(ua => ua.UserData)
                    .Include(ua => ua.UserRole)
                    .AsNoTracking()
                    .FirstOrDefault(ua => ua.Id.ToString() == userAccessId);
                if (userAccess != null)
                {
                    context.User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [
                                new Claim(ClaimTypes.Name, userAccess.UserData.FullName),
                                new(ClaimTypes.Email, userAccess.UserData.Email),
                                new(ClaimTypes.NameIdentifier, userAccess.Login),
                                new(ClaimTypes.Sid, userAccess.Id.ToString()),
                            ],
                            nameof(AuthSessionMiddleware)
                        )
                    );
                }
            }

            await _next(context);
        }
    }
}
