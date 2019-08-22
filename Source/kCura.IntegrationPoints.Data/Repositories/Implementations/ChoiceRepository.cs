using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class ChoiceRepository : IChoiceRepository
	{
		private const int _CHOICE_ARTIFACT_TYPE_ID = 7;

		private readonly IRelativityObjectManager _objectManager;

		public ChoiceRepository(IRelativityObjectManager objectManager)
		{
			_objectManager = objectManager;
		}

		public async Task<IList<ChoiceWithParentInfoDto>> QueryChoiceWithParentInfoAsync(
			ICollection<ChoiceDto> choicesToQuery, 
			ICollection<ChoiceDto> allChoices)
		{
			string condition = BuildCondition(choicesToQuery);

			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = _CHOICE_ARTIFACT_TYPE_ID },
				Fields = new FieldRef[0],
				Condition = condition
			};

			List<RelativityObject> choiceObjects = await _objectManager.QueryAsync(queryRequest).ConfigureAwait(false);

			List<ChoiceWithParentInfoDto> choiceWithParentInfoDtos = new List<ChoiceWithParentInfoDto>();

			foreach (var choiceObject in choiceObjects)
			{
				int? parentArtifactID = choiceObject.ParentObject.ArtifactID;
				if (allChoices.All(x => x.ArtifactID != parentArtifactID))
				{
					parentArtifactID = null;
				}

				string name = choicesToQuery.Single(x => x.ArtifactID == choiceObject.ArtifactID).Name;
				var choiceWithParentInfo = new ChoiceWithParentInfoDto(parentArtifactID, choiceObject.ArtifactID, name);
				choiceWithParentInfoDtos.Add(choiceWithParentInfo);
			}

			return choiceWithParentInfoDtos;
		}

		private static string BuildCondition(IEnumerable<ChoiceDto> choices)
		{
			IEnumerable<string> singleConditions = choices.Select(x => $"(('Artifact ID' == {x.ArtifactID}))");
			string condition = string.Join(" OR ", singleConditions);
			return condition;
		}
	}
}
