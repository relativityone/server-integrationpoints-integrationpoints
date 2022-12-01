using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    internal interface IChoiceCache
    {
        Task<IList<ChoiceWithParentInfo>> GetChoicesWithParentInfoAsync(ICollection<Choice> choices);
    }
}
