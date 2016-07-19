using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiErrorRepository : IErrorRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;
		private const int _EDDS_WORKSPACE_ID = -1;

		public RsapiErrorRepository(IHelper helper)
		{
			_helper = helper;
			_workspaceArtifactId = _EDDS_WORKSPACE_ID;
		}

		public void Create(IEnumerable<ErrorDTO> errors)
		{
			Error[] errorArtifacts = errors.Select(ConvertErrorDto).ToArray();

			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					rsapiClient.Repositories.Error.Create(errorArtifacts);
				}
				catch
				{
					// suppress errors (if we can't write relativity errors we're in trouble)
				}
			}
		}

		private Error ConvertErrorDto(ErrorDTO errorToConvert)
		{
			var convertedError = new Error()
			{
				Message = errorToConvert.Message,
				FullError = errorToConvert.FullText
			};

			return convertedError;
		}
	}
}