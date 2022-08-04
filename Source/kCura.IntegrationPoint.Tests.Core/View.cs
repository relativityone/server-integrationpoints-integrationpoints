using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.ItemListView;
using Relativity.Services.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using FieldRef = Relativity.Services.Field.FieldRef;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class View
    {
        private static ITestHelper Helper => new TestHelper();

        public static int QueryView(int workspaceID, string viewName)
        {
            using (IObjectManager objectManager = Helper.CreateProxy<IObjectManager>())
            {
                QueryResult queryResult = objectManager.QueryAsync(workspaceID, new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int)ArtifactType.View
                    },
                    Condition = $"'Name' == '{viewName}'"
                }, 0, int.MaxValue).GetAwaiter().GetResult();

                if (!queryResult.Objects.Any())
                {
                    throw new NotFoundException($"Cannot find view: '{viewName}'");
                }

                return queryResult.Objects.First().ArtifactID;
            }
        }

        public static async Task<int> CreateViewAsync(int workspaceID, string viewName, int objectTypeID, IEnumerable<Guid> fieldsGuids)
        {
            List<FieldRef> viewFields = await GetViewFieldsReferencesAsync(workspaceID, objectTypeID, fieldsGuids).ConfigureAwait(false);

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

        private static async Task<List<FieldRef>> GetViewFieldsReferencesAsync(int workspaceID, int objectTypeID, IEnumerable<Guid> fieldsGuids)
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
