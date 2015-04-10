using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ImportApiFactory
	{
		public virtual IImportAPI GetImportAPI(ImportSettings settings)
		{
			IImportAPI importAPI = null;
			try
			{
				importAPI = new ExtendedImportAPI(settings.WebServiceURL);
			}
			catch (Exception ex)
			{
				if (ex.Message.Equals("Login failed."))
				{
					throw new AuthenticationException(Properties.ErrorMessages.Login_Failed);
				}
				//LoggedException.PreserveStack(ex);
				throw;
			}
			return importAPI;
		}
	}
}
