using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal sealed class ChoiceCache : IChoiceCache
	{
		private const int _CHOICE_ARTIFACT_TYPE_ID = 7;

		private readonly IRelativityObjectManager _objectManager;
		private readonly IDictionary<int, ChoiceWithParentInfo> _cache;

		public ChoiceCache(IRelativityObjectManager objectManager)
		{
			_objectManager = objectManager;
			_cache = new Dictionary<int, ChoiceWithParentInfo>();
		}

		public async Task<IList<ChoiceWithParentInfo>> GetChoicesWithParentInfoAsync(ICollection<Choice> choices)
		{
			var choicesWithParentInfo = new List<ChoiceWithParentInfo>();

			foreach (Choice choice in choices)
			{
				ChoiceWithParentInfo choiceWithParentInfo;

				if (_cache.ContainsKey(choice.ArtifactID))
				{
					choiceWithParentInfo = _cache[choice.ArtifactID];
				}
				else
				{
					choiceWithParentInfo = await QueryChoiceWithParentInfoAsync(choice, choices).ConfigureAwait(false);
					_cache.Add(choice.ArtifactID, choiceWithParentInfo);
				}

				choicesWithParentInfo.Add(choiceWithParentInfo);
			}

			return choicesWithParentInfo;
		}

		private async Task<ChoiceWithParentInfo> QueryChoiceWithParentInfoAsync(Choice choice, ICollection<Choice> choices)
		{
			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef {ArtifactTypeID = _CHOICE_ARTIFACT_TYPE_ID},
				Fields = new FieldRef[0],
				Condition = $"(('Artifact ID' == {choice.ArtifactID}))"
			};

			List<RelativityObject> result = await _objectManager.QueryAsync(queryRequest).ConfigureAwait(false);
			RelativityObject choiceObject = result.Single();

			int? parentArtifactId = choiceObject.ParentObject.ArtifactID;
			if (choices.All(x => x.ArtifactID != parentArtifactId))
			{
				parentArtifactId = null;
			}
			var choiceWithParentInfo = new ChoiceWithParentInfo(parentArtifactId, choice.ArtifactID, choice.Name);
			return choiceWithParentInfo;
		}
	}
}
