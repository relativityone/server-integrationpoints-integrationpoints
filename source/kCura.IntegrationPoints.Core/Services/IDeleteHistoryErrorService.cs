using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IDeleteHistoryErrorService
	{
		void DeleteErrorAssociatedWithHistory(int historyId);
		void DeleteErrorAssociatedWithHistories(List<int> ids );
	}
}
