using Sql.SemanticSearch.AppHost.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Sql.SemanticSearch.AppHost.Extensions;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Extension members don't need to be static")]
internal static class ResourceBuilderExtensions
{
    extension(IResourceBuilder<ParameterResource> parameter)
    {
        internal string? GetValue() => parameter.Resource
            .GetValueAsync(default)
            .AsTask()
            .GetAwaiter()
            .GetResult();

        internal T? GetValue<T>() where T : struct, IParsable<T>
        {
            var value = parameter.GetValue();
            return T.TryParse(value, CultureInfo.InvariantCulture, out T result)
                ? result
                : default;
        }
    }

    extension(IResourceBuilder<OllamaResource> builder)
    {
        internal IResourceBuilder<OllamaResource> WithGPUSupportIfAvailable(
            IResourceBuilder<ParameterResource> vendorParameter)
        {
            var vendorValue = vendorParameter.GetValue();
            var vendor = vendorValue is not null && Enum.TryParse<OllamaGpuVendor>(vendorValue, out var gpuVendor)
                ? gpuVendor
                : default(OllamaGpuVendor?);

            return vendor is not null
                ? builder.WithGPUSupport(vendor.Value)
                : builder;
        }
    }

    extension(IResourceBuilder<SqlServerServerResource> builder)
    {
        internal IResourceBuilder<SqlServerServerResource> WithHostPortAndEndpointIfProvided(
            string name,
            IResourceBuilder<ParameterResource>? portParameter)
        {
            var port = portParameter?.GetValue<int>();
            return (port is not null && port > 0)
                ? builder.WithHostPort(port)
                          .WithEndpoint(targetPort: port, name: name)
            : builder;
        }

        internal IResourceBuilder<SqlServerServerResource> WithPasswordIfProvided(
            IResourceBuilder<ParameterResource>? password) =>
                password?.GetValue() is not null
                    ? builder.WithPassword(password)
                    : builder;
    }
}
