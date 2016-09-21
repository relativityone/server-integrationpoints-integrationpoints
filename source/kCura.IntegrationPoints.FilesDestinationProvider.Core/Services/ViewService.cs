
using System.Collections.Generic;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using ViewDTO = kCura.IntegrationPoints.Domain.Models.ViewDTO;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ViewService : IViewService
	{
		#region Fields

		private readonly IConfig _config;
		private readonly ICredentialProvider _credentialProvider;

		#endregion //Fields

		#region Constructors

		public ViewService(IConfig config, ICredentialProvider credentialProvider)
		{
			_config = config;
			_credentialProvider = credentialProvider;
		}

		#endregion //Constructors

		#region Methods

		public List<ViewDTO> GetViewsByWorkspaceAndArtifactType(int workspceId, int artifactTypeId)
		{
			// TODO (after upgrading new Rel GOLD build package)
			// ISearchManager searchManager = ServiceManagerProvider.Create<ISearchManager, SearchManagerFactory>(_config, _credentialProvider);
			// searchManager.RetrieveViewsByContextArtifactID(workspceId, artifactType)

			return new List<ViewDTO>();
		}

		#endregion //Methods


	}
}
