using FluentAssertions;
using kCura.Relativity.ImportAPI;
using Moq;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
	[TestFixture, Category("Unit")]
	public class FieldsClassifyRunnerFactoryTests
	{
		private IFieldsClassifyRunnerFactory _sut;

        private Mock<IImportAPI> _importApiFake;
		private Mock<IFieldsRepository> _fieldsRepositoryFake;

		[SetUp]
		public void SetUp()
		{
            _importApiFake = new Mock<IImportAPI>();
			_fieldsRepositoryFake = new Mock<IFieldsRepository>();

            _sut = new FieldsClassifyRunnerFactory(_importApiFake.Object, _fieldsRepositoryFake.Object);
		}

		[Test]
		public void CreateForDestinationWorkspace_CreatesFieldsClassifierRunnerForDestinationWorkspace()
		{
			// Act
			var classifierRunner = _sut.CreateForDestinationWorkspace();

			// Assert
			classifierRunner.Should().NotBeNull();
		}

		[Test]
		public void CreateForSourceWorkspace_CreatesFieldsClassifierRunnerForSourceWorkspace()
		{
			// Act
			var classifierRunner = _sut.CreateForSourceWorkspace();

			// Assert
			classifierRunner.Should().NotBeNull();
		}
	}
}
