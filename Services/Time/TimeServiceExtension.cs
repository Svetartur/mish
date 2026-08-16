using Microsoft.Extensions.DependencyInjection;

namespace ASP_P42.Services.Time
{
    public static class TimeServiceExtension
    {
        public static IServiceCollection AddTime(
            this IServiceCollection services)
        {
            return services.AddSingleton<ITimeService, TimeService>();
        }
    }
}
