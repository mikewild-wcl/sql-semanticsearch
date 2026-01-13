using Aspire.Hosting.Publishing;

namespace Sql.SemanticSearch.AppHost.ParameterDefaults;

internal sealed class EmptyParameterDefault : ParameterDefault
{
    public override string GetDefaultValue()
    {
        return string.Empty;
    }

    public override void WriteToManifest(ManifestPublishingContext context)
    {
    }
}
