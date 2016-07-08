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
			ICoreContext context = ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactID);

			kCura.Data.DataView dataview = (new SearchQuery()).RetrieveAllExportableViewFields(context, artifactTypeID);

			var fields = new List<FieldEntry>();
			for (int i = 0; i < dataview.Count; i++)
			{
				var fieldCategory = default(global::Relativity.FieldCategory);
				Enum.TryParse(dataview[i]["FieldCategoryID"].ToString(), out fieldCategory);

				fields.Add(new FieldEntry
				{
					FieldIdentifier = dataview[i]["AvfId"].ToString(),
					DisplayName = dataview[i]["DisplayName"].ToString(),
					IsIdentifier = fieldCategory == global::Relativity.FieldCategory.Identifier
				});
			}

			return fields.OrderBy(x => x.DisplayName).ToArray();
		}

		public FieldEntry[] GetAllViewFields(int workspaceArtifactID, int viewArtifactID, int artifactTypeID)
		{
			ICoreContext context = ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactID);

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