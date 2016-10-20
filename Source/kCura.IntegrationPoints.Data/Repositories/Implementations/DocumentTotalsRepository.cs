﻿using System;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class DocumentTotalsRepository : IDocumentTotalsRepository
	{
		#region Fields

		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;
		private readonly IAPILog _logger;

		#endregion Fields

		#region Constructors

		public DocumentTotalsRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<DocumentTotalsRepository>();
			_workspaceArtifactId = workspaceArtifactId;
		}

		#endregion //Constructors

		#region Methods

		public int GetSavedSearchTotalDocsCount(int savedSearchId)
		{
			var query = new Query<Relativity.Client.DTOs.Document>
			{
				Condition = new SavedSearchCondition(savedSearchId),
				Fields = FieldValue.NoFields
			};
			return QueryForTotals(query, "Failed to retrieve total export items count for saved search id: {savedSearchId}.", savedSearchId);
		}

		public int GetFolderTotalDocsCount(int folderId, int viewId, bool includeSubFoldersTotals)
		{
			var query = new Query<Relativity.Client.DTOs.Document>
			{
				Condition = new CompositeCondition(new ObjectCondition("Folder Name", GetConditionOperator(includeSubFoldersTotals), folderId),
					CompositeConditionEnum.And, new ViewCondition(viewId)),

				Fields = FieldValue.NoFields
			};
			return QueryForTotals(query, "Failed to retrieve total export items count for folder: {folderId} and view: {viewId}.", folderId, viewId);
		}

		public int GetProductionDocsCount(int productionSetId)
		{//TODO
			throw new NotImplementedException();
		}

		private int QueryForTotals(Query<Relativity.Client.DTOs.Document> query, string errMsgTemplate, params object[] parmeters)
		{
			try
			{
				using (
					IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;
					return rsapiClient.Repositories.Document.Query(query).TotalCount;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, errMsgTemplate, parmeters);
				throw;
			}
		}

		private ObjectConditionEnum GetConditionOperator(bool includeChildren)
		{
			return includeChildren ? ObjectConditionEnum.AnyOfThese : ObjectConditionEnum.EqualTo;
		}

		#endregion //Methods
	}
}
