using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
    internal class SavedSearchesTreeTestHelper
    {
        public static SearchContainerItemCollection GetSampleContainerCollection()
        {
            var collection = new SearchContainerItemCollection();

            #region SearchContainerItems

            collection.SearchContainerItems = new List<SearchContainerItem>
            {
                new SearchContainerItem // root
                {
                    SearchContainer = new SearchContainerRef { ArtifactID = 1035243, Name = "Kierkegaard" },
                    Secured = false,
                    Permissions = new SearchContainerItemPermissions { AddSearch = true, AddSearchFolder = true, DeleteSearchFolder = true, EditSearch = true, EditSearchFolder = true, SecureSearchFolder = true },
                    HasChildren = true,
                    ParentContainer = new SearchContainerRef { ArtifactID = 1003663, Name = default(string) }
                },
                new SearchContainerItem // first level subfolder
                {
                    SearchContainer = new SearchContainerRef { ArtifactID = 1038840, Name = "Search Folder 1" },
                    Secured = false,
                    Permissions = new SearchContainerItemPermissions { AddSearch = true, AddSearchFolder = true, DeleteSearchFolder = true, EditSearch = true, EditSearchFolder = true, SecureSearchFolder = true },
                    HasChildren = true,
                    ParentContainer = new SearchContainerRef { ArtifactID = 1035243, Name = default(string) }
                },
                new SearchContainerItem // first level subfolder
                {
                    SearchContainer = new SearchContainerRef { ArtifactID = 1038841, Name = "Search Folder 2" },
                    Secured = false,
                    Permissions = new SearchContainerItemPermissions { AddSearch = true, AddSearchFolder = true, DeleteSearchFolder = true, EditSearch = true, EditSearchFolder = true, SecureSearchFolder = true },
                    HasChildren = true,
                    ParentContainer = new SearchContainerRef { ArtifactID = 1035243, Name = default(string) }
                },
                new SearchContainerItem // second level subfolder
                {
                    SearchContainer = new SearchContainerRef { ArtifactID = 1038842, Name = "Search Folder 3" },
                    Secured = false,
                    Permissions = new SearchContainerItemPermissions { AddSearch = true, AddSearchFolder = true, DeleteSearchFolder = true, EditSearch = true, EditSearchFolder = true, SecureSearchFolder = true },
                    HasChildren = true,
                    ParentContainer = new SearchContainerRef { ArtifactID = 1038840, Name = default(string) }
                }
            };

            #endregion

            #region SavedSearchContainerItems

            collection.SavedSearchContainerItems = new List<SavedSearchContainerItem>
            {
                new SavedSearchContainerItem // root level search
                {
                    ParentContainer = new SearchContainerRef { ArtifactID = 1035243, Name = default(string) },
                    Permissions = new SavedSearchContainerItemPermissions { AddSearch = true, DeleteSearch = true, EditSearch = true, SecureSearch = true },
                    Personal = false,
                    SavedSearch = new SavedSearchRef { ArtifactID = 1038812, Name = "Saved Search 1", SearchType = "KeywordSearch" },
                    Secured = false
                },
                new SavedSearchContainerItem // first level search
                {
                    ParentContainer = new SearchContainerRef { ArtifactID = 1038842, Name = default(string) },
                    Permissions = new SavedSearchContainerItemPermissions { AddSearch = true, DeleteSearch = true, EditSearch = true, SecureSearch = true },
                    Personal = false,
                    SavedSearch = new SavedSearchRef { ArtifactID = 1038843, Name = "Saved Search 2", SearchType = "KeywordSearch" },
                    Secured = false
                },
                new SavedSearchContainerItem // first level search (personal|secured)
                {
                    ParentContainer = new SearchContainerRef { ArtifactID = 1038841, Name = default(string) },
                    Permissions = new SavedSearchContainerItemPermissions { AddSearch = true, DeleteSearch = true, EditSearch = true, SecureSearch = false },
                    Personal = true,
                    SavedSearch = new SavedSearchRef { ArtifactID = 1038860, Name = "Saved Search 3", SearchType = "KeywordSearch" },
                    Secured = true
                }
            };

            #endregion

            return collection;
        }

        public static SearchContainerItemCollection GetSampleToSanitizeContainerCollection()
        {
            var collection = new SearchContainerItemCollection();

            #region SearchContainerItems

            collection.SearchContainerItems = new List<SearchContainerItem>
            {
                new SearchContainerItem // root
                {
                    SearchContainer = new SearchContainerRef { ArtifactID = 1035243, Name = "<p>Kierkegaard</p>Platon" },
                    Secured = false,
                    Permissions = new SearchContainerItemPermissions { AddSearch = true, AddSearchFolder = true, DeleteSearchFolder = true, EditSearch = true, EditSearchFolder = true, SecureSearchFolder = true },
                    HasChildren = true,
                    ParentContainer = new SearchContainerRef { ArtifactID = 1003663, Name = default(string) }
                },
                new SearchContainerItem // first level subfolder
                {
                    SearchContainer = new SearchContainerRef { ArtifactID = 1038840, Name = "<img src=x onerror=alert(Saved Search 1) />Nitsche" },
                    Secured = false,
                    Permissions = new SearchContainerItemPermissions { AddSearch = true, AddSearchFolder = true, DeleteSearchFolder = true, EditSearch = true, EditSearchFolder = true, SecureSearchFolder = true },
                    HasChildren = true,
                    ParentContainer = new SearchContainerRef { ArtifactID = 1035243, Name = default(string) }
                },
                new SearchContainerItem // first level subfolder
                {
                    SearchContainer = new SearchContainerRef { ArtifactID = 1038841, Name = "<em>No-one survive</em>" },
                    Secured = false,
                    Permissions = new SearchContainerItemPermissions { AddSearch = true, AddSearchFolder = true, DeleteSearchFolder = true, EditSearch = true, EditSearchFolder = true, SecureSearchFolder = true },
                    HasChildren = true,
                    ParentContainer = new SearchContainerRef { ArtifactID = 1035243, Name = default(string) }
                },
                new SearchContainerItem // second level subfolder
                {
                    SearchContainer = new SearchContainerRef { ArtifactID = 1038842, Name = "Search Folder 3" },
                    Secured = false,
                    Permissions = new SearchContainerItemPermissions { AddSearch = true, AddSearchFolder = true, DeleteSearchFolder = true, EditSearch = true, EditSearchFolder = true, SecureSearchFolder = true },
                    HasChildren = true,
                    ParentContainer = new SearchContainerRef { ArtifactID = 1038840, Name = default(string) }
                }
            };

            #endregion

            #region SavedSearchContainerItems

            collection.SavedSearchContainerItems = new List<SavedSearchContainerItem>
            {
                new SavedSearchContainerItem // root level search
                {
                    ParentContainer = new SearchContainerRef { ArtifactID = 1035243, Name = default(string) },
                    Permissions = new SavedSearchContainerItemPermissions { AddSearch = true, DeleteSearch = true, EditSearch = true, SecureSearch = true },
                    Personal = false,
                    SavedSearch = new SavedSearchRef { ArtifactID = 1038812, Name = "<i>Saved </i>Search 1", SearchType = "KeywordSearch" },
                    Secured = false
                },
                new SavedSearchContainerItem // first level search
                {
                    ParentContainer = new SearchContainerRef { ArtifactID = 1038842, Name = default(string) },
                    Permissions = new SavedSearchContainerItemPermissions { AddSearch = true, DeleteSearch = true, EditSearch = true, SecureSearch = true },
                    Personal = false,
                    SavedSearch = new SavedSearchRef { ArtifactID = 1038843, Name = "<king>Search </king>2", SearchType = "KeywordSearch" },
                    Secured = false
                },
                new SavedSearchContainerItem // first level search (personal|secured)
                {
                    ParentContainer = new SearchContainerRef { ArtifactID = 1038841, Name = default(string) },
                    Permissions = new SavedSearchContainerItemPermissions { AddSearch = true, DeleteSearch = true, EditSearch = true, SecureSearch = false },
                    Personal = true,
                    SavedSearch = new SavedSearchRef { ArtifactID = 1038860, Name = "<em>Saved Search 3</em>", SearchType = "KeywordSearch" },
                    Secured = true
                }
            };

            #endregion

            return collection;
        }

        public static JsTreeItemDTO GetSampleTree()
        {
            var root = new JsTreeItemWithParentIdDTO
            {
                Id = "1035243",
                Text = "Platon",
                ParentId = "1003663"
            };

            var folder1 = new JsTreeItemWithParentIdDTO
            {
                Id = "1038840",
                Text = "Nitsche",
                ParentId = root.Id
            };
            folder1.Children.Add(new JsTreeItemWithParentIdDTO
            {
                Id = "1038842",
                Text = "Search Folder 3",
                ParentId = folder1.Id
            });

            var folder2 = new JsTreeItemWithParentIdDTO
            {
                Id = "1038841",
                Text = "Sanitized Search Name",
                ParentId = root.Id
            };

            root.Children.AddRange(new List<JsTreeItemWithParentIdDTO> { folder1, folder2 });

            return root;
        }

        public static JsTreeItemDTO GetSampleTreeWithSearches()
        {
            var root = new JsTreeItemWithParentIdDTO
            {
                Id = "1035243",
                Text = "Platon",
                ParentId = "1003663"
            };

            var search1 = new JsTreeItemWithParentIdDTO
            {
                Id = "1038812",
                Text = "Search 1",
                ParentId = root.Id
            };

            var folder1 = new JsTreeItemWithParentIdDTO
            {
                Id = "1038840",
                Text = "Nitsche",
                ParentId = root.Id
            };
            folder1.Children.Add(new JsTreeItemWithParentIdDTO
            {
                Id = "1038842",
                Text = "Search Folder 3",
                ParentId = folder1.Id
            });
            folder1.Children[0].Children.Add(new JsTreeItemWithParentIdDTO
            {
                Id = "1038843",
                Text = "2",
                ParentId = folder1.Children[0].Id
            });

            var folder2 = new JsTreeItemWithParentIdDTO
            {
                Id = "1038841",
                Text = "Sanitized Search Name",
                ParentId = root.Id
            };
            folder2.Children.Add(new JsTreeItemWithParentIdDTO
            {
                Id = "1038860",
                Text = "Sanitized Search Name",
                ParentId = folder2.Id
            });

            root.Children.AddRange(new List<JsTreeItemWithParentIdDTO> { search1, folder1, folder2 });

            return root;
        }
    }
}
