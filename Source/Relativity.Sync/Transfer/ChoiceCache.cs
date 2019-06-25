using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal sealed class ChoiceCache : IChoiceCache
	{
		private readonly ISynchronizationConfiguration _configuration;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IDictionary<int, ChoiceWithParentInfo> _cache;

		public ChoiceCache(ISynchronizationConfiguration configuration, ISourceServiceFactoryForUser serviceFactory)
		{
			_configuration = configuration;
			_serviceFactory = serviceFactory;
			_cache = new Dictionary<int, ChoiceWithParentInfo>();
		}

		public async Task<IList<ChoiceWithParentInfo>> GetChoicesWithParentInfoAsync(ICollection<Choice> choices)
		{
			var choicesWithParentInfo = new List<ChoiceWithParentInfo>();

			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				foreach (Choice choice in choices)
				{
					ChoiceWithParentInfo choiceWithParentInfo;

					if (_cache.ContainsKey(choice.ArtifactID))
					{
						choiceWithParentInfo = _cache[choice.ArtifactID];
					}
					else
					{
						choiceWithParentInfo = await QueryChoiceWithParentInfoAsync(objectManager, choice, choices).ConfigureAwait(false);
						_cache.Add(choice.ArtifactID, choiceWithParentInfo);
					}

					choicesWithParentInfo.Add(choiceWithParentInfo);
				}
			}

			return choicesWithParentInfo;
		}

		private async Task<ChoiceWithParentInfo> QueryChoiceWithParentInfoAsync(IObjectManager objectManager, Choice choice, ICollection<Choice> choices)
		{
			var request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = (int)ArtifactType.Code
				},
				Condition = $"'ArtifactID' == {choice.ArtifactID}"
			};
			QueryResult queryResult = await objectManager.QueryAsync(_configuration.SourceWorkspaceArtifactId, request, 1, 1).ConfigureAwait(false);
			if (queryResult.ResultCount == 0)
			{
				throw new SyncException($"Query for Choice Artifact ID '{choice.ArtifactID}' returned no results.");
			}

			int? parentArtifactId = queryResult.Objects[0].ParentObject.ArtifactID;
			if (choices.All(x => x.ArtifactID != parentArtifactId))
			{
				parentArtifactId = null;
			}

			var choiceWithParentInfo = new ChoiceWithParentInfo(parentArtifactId, choice.ArtifactID, choice.Name);
			return choiceWithParentInfo;
		}
	}
}