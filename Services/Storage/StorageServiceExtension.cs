namespace ASP_P42.Services.Storage
{
    public static class StorageServiceExtension
    {
        public static IServiceCollection AddStorage(
           this IServiceCollection services)
        {
            return services.AddSingleton<IStorageService, LocalStorageService>();
        }
    }
}
