using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal sealed class ChoiceCache : IChoiceCache
	{
		private readonly IChoiceRepository _choiceRepository;
		private readonly IDictionary<int, ChoiceWithParentInfoDto> _cache;

		public ChoiceCache(IChoiceRepository choiceRepository)
		{
			_choiceRepository = choiceRepository;
			_cache = new Dictionary<int, ChoiceWithParentInfoDto>();
		}

		public async Task<IList<ChoiceWithParentInfoDto>> GetChoicesWithParentInfoAsync(ICollection<ChoiceDto> choices)
		{
			var choicesWithParentInfo = new List<ChoiceWithParentInfoDto>();
			var choicesMissingFromCache = new List<ChoiceDto>();

			foreach (ChoiceDto choice in choices)
			{
				if (_cache.ContainsKey(choice.ArtifactID))
				{
					ChoiceWithParentInfoDto choiceWithParentInfo = _cache[choice.ArtifactID];
					choicesWithParentInfo.Add(choiceWithParentInfo);
				}
				else
				{
					choicesMissingFromCache.Add(choice);
				}
			}

			if (choicesMissingFromCache.Any())
			{
				IList<ChoiceWithParentInfoDto> choicesWithParentInfoMissingFromCache =
					await QueryMissingChoicesAsync(choicesMissingFromCache, choices).ConfigureAwait(false);
				choicesWithParentInfo.AddRange(choicesWithParentInfoMissingFromCache);
			}

			return choicesWithParentInfo;
		}

		private async Task<IList<ChoiceWithParentInfoDto>> QueryMissingChoicesAsync(
			ICollection<ChoiceDto> missingChoices,
			ICollection<ChoiceDto> allChoices)
		{
			IList<ChoiceWithParentInfoDto> missingChoicesWithParentInfo =
				await _choiceRepository.QueryChoiceWithParentInfoAsync(missingChoices, allChoices).ConfigureAwait(false);

			foreach (var choice in missingChoicesWithParentInfo)
			{
				_cache.Add(choice.ArtifactID, choice);
			}

			return missingChoicesWithParentInfo;
		}
	}
}
