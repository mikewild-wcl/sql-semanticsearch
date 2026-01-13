using Aspire.Hosting.Publishing;

namespace Sql.SemanticSearch.AppHost.ParameterDefaults;

internal sealed class BooleanParameterDefault(bool value = false) : ParameterDefault
{
    public override string GetDefaultValue()
    {
        return value.ToString();
    }

    public override void WriteToManifest(ManifestPublishingContext context)
    {
    }
}
