using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class FolderStructureBehaviorValidator : IValidator
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;

		public FolderStructureBehaviorValidator(ISourceServiceFactoryForUser sourceServiceFactoryForUser)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult result = new ValidationResult();

			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				const int documentArtifactTypeId = 10;
				const string fieldType = "Field Type";
				const string longText = "Long Text";
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = documentArtifactTypeId
					},
					Condition = $"(('ArtifactID' == {configuration.FolderPathSourceFieldArtifactId}))",
					Fields = new[]
					{
						new FieldRef() { Name = fieldType},
					}
				};
				const int start = 0;
				const int length = 1;
				QueryResult queryResult = await objectManager.QueryAsync(configuration.SourceWorkspaceArtifactId, queryRequest, start, length, token,
					new EmptyProgress<ProgressReport>()).ConfigureAwait(false);
				if (queryResult.Objects.Count > 0)
				{
					string fieldTypeName = queryResult.Objects.First()[fieldType].Value.ToString();
					if (longText != fieldTypeName)
					{
						result.Add($"Folder Path Source Field has invalid type: '{fieldTypeName}' but expected '{longText}'");
					}
				}
				else
				{
					result.Add($"Field Artifact ID: {configuration.FolderPathSourceFieldArtifactId} not found.");
				}
			}

			return result;
		}
	}
}