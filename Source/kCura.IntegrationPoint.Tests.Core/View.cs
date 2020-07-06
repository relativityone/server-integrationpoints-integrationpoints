#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
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

		public static async Task<int> CreateViewAsync(int workspaceID, string viewName, int objectTypeID, IEnumerable<Guid> fieldsGuids)
		{
			List<FieldRef> viewFields = await GetViewFieldsReferences(workspaceID, objectTypeID, fieldsGuids).ConfigureAwait(false);

			var viewToCreate = new global::Relativity.Services.View.View
			{
				ArtifactTypeID = objectTypeID,
				Name = viewName,
				Fields = viewFields,
				VisibleInDropdown = true,
				Order = 0
			};

			using (IViewManager viewManager = Helper.CreateProxy<IViewManager>())
			{
				return await viewManager.CreateSingleAsync(workspaceID, viewToCreate).ConfigureAwait(false);
			}
		}

		private static async Task<List<FieldRef>> GetViewFieldsReferences(int workspaceID, int objectTypeID, IEnumerable<Guid> fieldsGuids)
		{
			using (IItemListViewManager viewManager = Helper.CreateProxy<IItemListViewManager>())
			{
				Dictionary<int, ItemListFieldRef> objectTypeViewFields = await viewManager
					.GetFieldsAsync(workspaceID, objectTypeID)
					.ConfigureAwait(false);

				return objectTypeViewFields.Values
					.Where(viewField => viewField.Guids.Intersect(fieldsGuids).Any())
					.Select(viewField => new FieldRef(viewField.ArtifactID))
					.ToList();
			}
		}
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
