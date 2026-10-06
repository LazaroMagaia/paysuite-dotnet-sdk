using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PaySuite.Sdk;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaySuite(this IServiceCollection services,
        Action<PaySuiteOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient<PaySuiteClient>((sp, http) =>
            PaySuiteClient.Configure(http, sp.GetRequiredService<IOptions<PaySuiteOptions>>().Value));
        return services;
    }
}