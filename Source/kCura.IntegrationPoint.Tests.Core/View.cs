using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Field;
using Relativity.Services.ItemListView;
using Relativity.Services.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class View
	{
		private static ITestHelper Helper => new TestHelper();

		public static int QueryView(int workspaceID, string viewName)
		{
			using (IRSAPIClient rsapiClient = Rsapi.CreateRsapiClient())
			{
				rsapiClient.APIOptions.WorkspaceID = workspaceID;
				var viewQuery = new Query<Relativity.Client.DTOs.View>
				{
					Condition = new TextCondition(ViewFieldNames.Name, TextConditionEnum.EqualTo, viewName)
				};
				return rsapiClient.Repositories.View.Query(viewQuery).Results[0].Artifact.ArtifactID;
			}
		}

		public static async Task<int> CreateViewAsync(int workspaceID, string viewName, int objectTypeId, IEnumerable<Guid> fieldsGuids)
		{
			List<FieldRef> viewFields = await GetViewFieldsReferences(workspaceID, objectTypeId, fieldsGuids).ConfigureAwait(false);

			var viewToCreate = new global::Relativity.Services.View.View
			{
				ArtifactTypeID = objectTypeId,
				Name = viewName,
				Fields = viewFields,
				VisibleInDropdown = true,
				Order = 0
			};

			using (IViewManager viewManager = Helper.CreateAdminProxy<IViewManager>())
			{
				return await viewManager.CreateSingleAsync(workspaceID, viewToCreate).ConfigureAwait(false);
			}
		}

		private static async Task<List<FieldRef>> GetViewFieldsReferences(int workspaceID, int objectTypeID, IEnumerable<Guid> fieldsGuids)
		{
			using (IItemListViewManager viewManager = Helper.CreateAdminProxy<IItemListViewManager>())
			{
				Dictionary<int, ItemListFieldRef> objectTypeViewFields = await viewManager
					.GetFieldsAsync(workspaceID, objectTypeID)
					.ConfigureAwait(false);

				return objectTypeViewFields.Values
					.Where(viewField => viewField.Guids.Intersect(fieldsGuids).Any())
					.Select(viewField => viewField as FieldRef)
					.ToList();
			}
		}
	}
}