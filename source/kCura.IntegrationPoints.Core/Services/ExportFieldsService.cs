using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Query;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ExportFieldsService : IExportFieldsService
	{
		private readonly IOnBehalfOfUserClaimsPrincipalFactory _claimsPrincipalFactory;

		public ExportFieldsService(IOnBehalfOfUserClaimsPrincipalFactory claimsPrincipalFactory)
		{
			_claimsPrincipalFactory = claimsPrincipalFactory;
		}

		public FieldEntry[] GetAllExportableFields(int workspaceArtifactID, int artifactTypeID)
		{
			ClaimsPrincipal principal = _claimsPrincipalFactory.CreateClaimsPrincipal(9); // TODO: get current user id, admin for now
			ICoreContext context = principal.GetUnversionContext(workspaceArtifactID);

			kCura.Data.DataView dataview = (new SearchQuery()).RetrieveAllExportableViewFields(context, artifactTypeID);

			var fields = new List<FieldEntry>();
			for (int i = 0; i < dataview.Count; i++)
			{
				fields.Add(new FieldEntry
				{
					FieldIdentifier = dataview[i]["AvfId"].ToString(),
					DisplayName = dataview[i]["DisplayName"].ToString()
				});
			}

			return fields.ToArray();
		}

		public FieldEntry[] GetAllViewFields(int workspaceArtifactID, int viewArtifactID, int artifactTypeID)
		{
			ClaimsPrincipal principal = _claimsPrincipalFactory.CreateClaimsPrincipal(9); // TODO: get current user id, admin for now
			ICoreContext context = principal.GetUnversionContext(workspaceArtifactID);

			IPermissionsMatrix permissions = UserPermissionsMatrixFactory.Instance.CreateUserPermissionsMatrix(context);
			var fieldDTOs = ViewHelper.GetViewFields(context as BaseServiceContext, viewArtifactID, artifactTypeID, permissions);

			var fields = new List<FieldEntry>();
			for (int i = 0; i < fieldDTOs.Count; i++)
			{
				fields.Add(new FieldEntry
				{
					FieldIdentifier = fieldDTOs[i].ArtifactViewFieldID.ToString(),
					DisplayName = fieldDTOs[i].HeaderName
				});
			}

			return fields.ToArray();
		}
	}
}