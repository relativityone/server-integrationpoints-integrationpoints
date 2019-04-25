using FluentAssertions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
	public class CoreServiceFactoryTests
	{
		private CoreServiceFactory _sut;
		private Mock<IRepositoryFactory> _repositoryFactory;
		private Mock<IViewFieldRepository> _viewFieldRepository;
		private Mock<IFileFieldRepository> _fileFieldRepository;
		private Mock<IFileRepository> _fileRepository;
		private ExportFile _exportFile;
		private Mock<IServiceFactory> _webApiServiceFactory;
		private const int _CONTEXT_USER_ID = 41231;
		[SetUp]
		public void SetUp()
		{
			_repositoryFactory = new Mock<IRepositoryFactory>();
			_viewFieldRepository = new Mock<IViewFieldRepository>();
			_fileFieldRepository = new Mock<IFileFieldRepository>();
			_fileRepository = new Mock<IFileRepository>();
			_exportFile = new ExportFile(123);
			_webApiServiceFactory = new Mock<IServiceFactory>();
		}

		[Test]
		public void CreateProductionManagerShouldReturnProperImplementation()
		{
			//arrange
			Mock<IProductionManager> productionManagerMock = new Mock<IProductionManager>();
			_webApiServiceFactory.Setup(x => x.CreateProductionManager()).Returns(productionManagerMock.Object);
			_sut = CreateCoreServiceFactory();

			//act & assert
			_sut.CreateProductionManager().Should().Be(productionManagerMock.Object);
		}

		private CoreServiceFactory CreateCoreServiceFactory()
		{
			return new CoreServiceFactory(
				_repositoryFactory.Object,
				_viewFieldRepository.Object,
				_fileFieldRepository.Object,
				_fileRepository.Object,
				_exportFile,
				_webApiServiceFactory.Object,
				_CONTEXT_USER_ID);
		}
	}
}
