using Microsoft.Extensions.Configuration;
using Sql.SemanticSearch.Shared;
using System.Diagnostics.CodeAnalysis;

namespace Sql.SemanticSearch.Core.Configuration;

[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Extension used from other assemblies.")]
public static class ConfigurationManagerExtensions
{
    extension(IConfigurationManager configurationManager)
    {
        public AISettings GetAISettings()
        {
            var aiProvider = configurationManager[ParameterNames.AIProvider];
            var externalEmbeddingModel = configurationManager[ParameterNames.SqlServerExternalEmbeddingModel];
            var embeddingDimensions = configurationManager[ParameterNames.EmbeddingDimensions];

            if (string.IsNullOrWhiteSpace(aiProvider) || string.IsNullOrWhiteSpace(externalEmbeddingModel))
            {
                throw new InvalidOperationException(
                    $"Missing configuration. Both {ParameterNames.AIProvider} and {ParameterNames.SqlServerExternalEmbeddingModel} are required.");
            }

            var embeddingModelDimensions = int.TryParse(embeddingDimensions, out var dimensions) 
                                           && dimensions > 0
                ? dimensions
                : Defaults.DefaultEmbeddingDimensions;

            return new AISettings(
                Provider: aiProvider,
                ExternalEmbeddingModel: externalEmbeddingModel)
            {
                EmbeddingModelDimensions = embeddingModelDimensions
            };
        }
    }
}
