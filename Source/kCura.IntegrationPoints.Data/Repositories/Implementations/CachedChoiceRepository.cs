using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class CachedChoiceRepository : IChoiceRepository
    {
        private readonly IChoiceRepository _innerChoiceRepository;
        private readonly IDictionary<int, ChoiceWithParentInfoDto> _cache;

        public CachedChoiceRepository(IChoiceRepository innerChoiceRepository)
        {
            _innerChoiceRepository = innerChoiceRepository;
            _cache = new Dictionary<int, ChoiceWithParentInfoDto>();
        }

        public async Task<IList<ChoiceWithParentInfoDto>> QueryChoiceWithParentInfoAsync(
            ICollection<ChoiceDto> choicesToQuery,
            ICollection<ChoiceDto> allChoices)
        {
            var choicesWithParentInfo = new List<ChoiceWithParentInfoDto>();
            var choicesMissingFromCache = new List<ChoiceDto>();

            foreach (ChoiceDto choice in choicesToQuery)
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
                    await QueryMissingChoicesAsync(choicesMissingFromCache, allChoices).ConfigureAwait(false);
                choicesWithParentInfo.AddRange(choicesWithParentInfoMissingFromCache);
            }

            return choicesWithParentInfo;
        }

        private async Task<IList<ChoiceWithParentInfoDto>> QueryMissingChoicesAsync(
            ICollection<ChoiceDto> missingChoices,
            ICollection<ChoiceDto> allChoices)
        {
            IList<ChoiceWithParentInfoDto> missingChoicesWithParentInfo =
                await _innerChoiceRepository.QueryChoiceWithParentInfoAsync(missingChoices, allChoices).ConfigureAwait(false);

            foreach (var choice in missingChoicesWithParentInfo)
            {
                _cache.Add(choice.ArtifactID, choice);
            }

            return missingChoicesWithParentInfo;
        }
    }
}
