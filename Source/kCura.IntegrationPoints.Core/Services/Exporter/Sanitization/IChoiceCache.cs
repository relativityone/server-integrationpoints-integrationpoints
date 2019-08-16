using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal interface IChoiceCache
	{
		Task<IList<ChoiceWithParentInfo>> GetChoicesWithParentInfoAsync(ICollection<Choice> choices);
	}
}
