namespace ASP_P42.Middleware.AuthSession
{
    public static class AuthSessionExtension
    {
        public static IApplicationBuilder UseAuthSession(
        this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthSessionMiddleware>();
        }
    }
}
