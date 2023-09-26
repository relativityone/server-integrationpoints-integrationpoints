using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class KeywordSearchManagerStub : KeplerStubBase<IKeywordSearchManager>
    {
        public void SetupKeywordSearchManagerStub()
        {
            Mock.Setup(x => x.ReadSingleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int workspaceArtifactId, int searchArtifactId) =>
                    Task.FromResult(GetKeywordSearch(workspaceArtifactId, searchArtifactId)));
        }

        private KeywordSearch GetKeywordSearch(int workspaceArtifactId, int searchArtifactId)
        {
            SavedSearchFake savedSearch;
            try
            {
                WorkspaceFake workspace = Relativity.Workspaces.First(x => x.ArtifactId == workspaceArtifactId);
                savedSearch = workspace.SavedSearches.First(x => x.ArtifactId == searchArtifactId);
            }
            catch
            {
                return new KeywordSearch();
            }

            KeywordSearch keywordSearch = new KeywordSearch
            {
                ArtifactID = searchArtifactId,
                Name = savedSearch.Name,
                Fields = savedSearch.Values.Values
                    .Select(x =>
                {
                    FieldFake fieldFake = (FieldFake) x;
                    return new FieldRef
                    {
                        Name = fieldFake.Name,
                        ArtifactID = fieldFake.ArtifactId
                    };
                }).ToList()
            };

            return keywordSearch;
        }
    }
}
