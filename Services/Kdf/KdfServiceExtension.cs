namespace ASP_P42.Services.Kdf
{
    public static class KdfServiceExtension
    {
        public static IServiceCollection AddKdf(
            this IServiceCollection services)
        {
            return services.AddSingleton<IKdfService, PbKdf1Service>();
        }
    }
}
