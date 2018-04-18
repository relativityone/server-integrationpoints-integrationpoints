using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Validation
{
    public class IntegrationPointExecutionValidator : IIntegrationPointExecutionValidator
    {
        private readonly ISerializer _serializer;
        private readonly IRelativityProviderValidatorsFactory _validatorsFactory;

        public IntegrationPointExecutionValidator(ISerializer serializer, IRelativityProviderValidatorsFactory validatorsFactory)
        {
            _serializer = serializer;
            _validatorsFactory = validatorsFactory;
        }

        public ValidationResult Validate(IntegrationPointModel integrationModel)
        {
            var result = new ValidationResult();
            SourceConfiguration sourceConfiguration;

            try
            {
                sourceConfiguration =
                    _serializer.Deserialize<SourceConfiguration>(integrationModel.SourceConfiguration);
            }
            catch (JsonReaderException) //handle case of encrypted sourceConfiguration
            {
                return result;
            }

            if (sourceConfiguration.SavedSearchArtifactId != 0)
            {
                SavedSearchValidator savedSearchValidator =
                    _validatorsFactory.CreateSavedSearchValidator(sourceConfiguration.SourceWorkspaceArtifactId,
                        sourceConfiguration.SavedSearchArtifactId);
                result.Add(savedSearchValidator.Validate(sourceConfiguration.SavedSearchArtifactId));
            }

            //ToDo: Add other validators e.g. for production

            return result;
        }
    }
}