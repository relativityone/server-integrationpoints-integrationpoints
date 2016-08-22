using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IDeleteHistoryErrorService
	{
		void DeleteErrorAssociatedWithHistory(int historyId);
		void DeleteErrorAssociatedWithHistories(List<int> ids );
	}
}
