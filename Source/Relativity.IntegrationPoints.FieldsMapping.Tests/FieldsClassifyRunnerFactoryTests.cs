using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
	[TestFixture, Category("Unit")]
	public class FieldsClassifyRunnerFactoryTests
	{
		private IFieldsClassifyRunnerFactory _sut;

		private Mock<IImportApiFacade> _importApiFacadeFake;
		private Mock<IFieldsRepository> _fieldsRepositoryFake;

		[SetUp]
		public void SetUp()
		{
			_importApiFacadeFake = new Mock<IImportApiFacade>();
			_fieldsRepositoryFake = new Mock<IFieldsRepository>();

			_sut = new FieldsClassifyRunnerFactory(_importApiFacadeFake.Object, _fieldsRepositoryFake.Object);
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
