using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;

namespace kCura.IntegrationPoints.Core.Tests
{
    [TestFixture, Category("Unit")]
    public class RdoFilterTest : TestBase
    {
        private Mock<ICaseServiceContext> _context;
        private Mock<IObjectTypeQuery> _objectTypeQuery;
        private Mock<IServicesMgr> _servicesMgr;
        private Mock<IObjectTypeManager> _objectTypeManager;
        private RdoFilter _sut;
        private const int _USER_ID = 123;
        private readonly List<string> _systemRdo = new List<string>
        {
            "History", "Event Handler", "Install Event Handler", "Source Provider", "Integration Point", "Relativity Source Case", "Destination Workspace", "Relativity Source Job"
        };

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _context = new Mock<ICaseServiceContext>();
            _objectTypeQuery = new Mock<IObjectTypeQuery>();
            _servicesMgr = new Mock<IServicesMgr>();
            _objectTypeManager = new Mock<IObjectTypeManager>();

            _context.SetupGet(x => x.WorkspaceUserID).Returns(_USER_ID);
            _servicesMgr.Setup(x => x.CreateProxy<IObjectTypeManager>(ExecutionIdentity.CurrentUser)).Returns(_objectTypeManager.Object);
            _objectTypeManager.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(
                new ObjectTypeResponse()
                {
                    RelativityApplications = new SecurableList<DisplayableObjectIdentifier>()
                    {
                        ViewableItems = new List<DisplayableObjectIdentifier>()
                    }
                });

            _sut = new RdoFilter(_objectTypeQuery.Object, _context.Object, _servicesMgr.Object);
        }

        [Test]
        public async Task GetAllViewableRdosAsync_ShouldReturnRdosExceptSystemRdos()
        {
            // Arrange
            string[] accessibleObjectTypes = new[] { "Document", "Entity" };

            List<ObjectTypeDTO> allObjectTypes = _systemRdo
                .Concat(accessibleObjectTypes)
                .Select(x => new ObjectTypeDTO()
                {
                    Name = x
                })
                .ToList();

            _objectTypeQuery.Setup(x => x.GetAllTypes(_USER_ID)).Returns(allObjectTypes);

            // Act
            IEnumerable<ObjectTypeDTO> objectTypes = await _sut.GetAllViewableRdosAsync();

            // Assert
            objectTypes.Select(x => x.Name).ShouldAllBeEquivalentTo(accessibleObjectTypes);
        }

        [Test]
        public async Task GetAllViewableRdosAsync_ShouldReturnViewableRdosWithApplication()
        {
            // Arrange
            string[] accessibleObjectTypes = new[] { "Document", "Entity" };

            List<ObjectTypeDTO> allObjectTypes = accessibleObjectTypes
                .Select(x => new ObjectTypeDTO()
                {
                    Name = x
                })
                .ToList();

            _objectTypeManager.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(
                new ObjectTypeResponse()
                {
                    RelativityApplications = new SecurableList<DisplayableObjectIdentifier>()
                    {
                        ViewableItems = new List<DisplayableObjectIdentifier>()
                        {
                            new DisplayableObjectIdentifier()
                        }
                    }
                });

            _objectTypeQuery.Setup(x => x.GetAllTypes(_USER_ID)).Returns(allObjectTypes);

            // Act
            IEnumerable<ObjectTypeDTO> objectTypes = await _sut.GetAllViewableRdosAsync();

            // Assert
            objectTypes.Select(x => x.BelongsToApplication).ShouldAllBeEquivalentTo(true);
        }
    }
}
