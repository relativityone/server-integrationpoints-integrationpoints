using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class SavedSearchValidator : IValidator
	{
		private const string _SAVED_SEARCH_NOT_PUBLIC = "The saved search must be public.";
		private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
		private readonly ISyncLog _logger;
		private static readonly ValidationMessage SavedSearchNoAccess = new ValidationMessage(
			errorCode: $"20.004",
			shortMessage: $"Saved search is not available or has been secured from this user. Contact your system administrator."
		);

		public SavedSearchValidator(ISourceServiceFactoryForUser serviceFactoryForUser, ISyncLog logger)
		{
			_serviceFactoryForUser = serviceFactoryForUser;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validating saved search artifact ID: {savedSearchArtifactId} in source workspace artifact ID: {sourceWorkspaceArtifactId}",
				configuration.SavedSearchArtifactId, configuration.SourceWorkspaceArtifactId);

			ValidationResult validationResult = new ValidationResult();

			try
			{
				using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					const int savedSearchArtifactTypeId = 15;
					const string owner = "Owner";
					QueryRequest queryRequest = new QueryRequest()
					{
						ObjectType = new ObjectTypeRef()
						{
							ArtifactTypeID = savedSearchArtifactTypeId
						},
						Condition = $"(('ArtifactId' == {configuration.SavedSearchArtifactId}))",
						Fields = new[]
						{
							new FieldRef() {Name = owner}
						}
					};

					const int start = 0;
					const int length = 1;
					QueryResult queryResult;
					try
					{
						queryResult = await objectManager.QueryAsync(configuration.SourceWorkspaceArtifactId, queryRequest, start, length, token, new EmptyProgress<ProgressReport>()).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Failed to query for saved search artifact ID: {savedSearchArtifactId}", configuration.SavedSearchArtifactId);
						throw;
					}

					if (queryResult == null || queryResult.Objects.Count == 0)
					{
						validationResult.Add(SavedSearchNoAccess);
					}
					else
					{
						string actualOwner = queryResult.Objects[0].FieldValues.First(x => x.Field.Name.Equals(owner, StringComparison.InvariantCulture)).Value.ToString();
						bool savedSearchIsPublic = string.IsNullOrEmpty(actualOwner);
						if (!savedSearchIsPublic)
						{
							validationResult.Add(_SAVED_SEARCH_NOT_PUBLIC);
						}
					}
				}
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred during saved search validation.";
				_logger.LogError(ex, message);
				throw;
			}

			return validationResult;
		}

		public bool ShouldValidate(ISyncPipeline pipeline) => !pipeline.IsNonDocumentPipeline();
	}
}