namespace ASP_P42.Services.Hash
{
    public static class HashServiceExtension
    {
        public static IServiceCollection AddHash(
            this IServiceCollection services)
        {
            return services.AddSingleton<IHashService, Md5HashService>();
        }
    }
}