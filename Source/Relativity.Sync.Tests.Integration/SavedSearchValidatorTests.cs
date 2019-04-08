using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class SavedSearchValidatorTests
	{
		private ConfigurationStub _configuration;
		private Mock<ISourceServiceFactoryForUser> _serviceFactory;
		private SavedSearchValidator _sut;
		private Mock<IObjectManager> _objectManager;
		private const int _WORKSPACE_ARTIFACT_ID = 123;
		private const int _SAVED_SEARCH_ARTIFACT_ID = 456;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			_serviceFactory.Setup(sf => sf.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			containerBuilder.RegisterInstance(_serviceFactory.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterType<SavedSearchValidator>();
			IContainer container = containerBuilder.Build();

			_configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _WORKSPACE_ARTIFACT_ID,
				SavedSearchArtifactId = _SAVED_SEARCH_ARTIFACT_ID
			};
			_sut = container.Resolve<SavedSearchValidator>();
		}

		[Test]
		public async Task ItShouldNotThrowWhenObjectManagerFails()
		{
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ARTIFACT_ID, It.IsAny<QueryRequest>(), 0, 1, CancellationToken.None, It.IsAny<IProgress<ProgressReport>>()))
					.Throws<InvalidOperationException>();

			// act
			ValidationResult result = await _sut.ValidateAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.IsValid.Should().Be(false);
			result.Messages.Count().Should().Be(1);
		}

		[Test]
		public async Task ItShouldReportFailedValidationResultWhenSavedSearchNotPublic()
		{
			_objectManager.Setup(x =>
					x.QueryAsync(
						_WORKSPACE_ARTIFACT_ID,
						It.Is<QueryRequest>(y => 
							y.Condition.Contains(_configuration.SavedSearchArtifactId.ToString(CultureInfo.InvariantCulture))),
							It.IsAny<int>(),
							It.IsAny<int>(),
							CancellationToken.None,
							It.IsAny<IProgress<ProgressReport>>()))
								.Returns(Task.FromResult(new QueryResult
							{
								Objects = new List<RelativityObject>
								{
									new RelativityObject()
									{
										FieldValues = new List<FieldValuePair>()
										{
											new FieldValuePair()
											{
												Field = new Field()
												{
													Name = "Owner"
												},
												Value = "some user"
											}
										}
									}
								}
							}));

			// act
			ValidationResult result = await _sut.ValidateAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.IsValid.Should().Be(false);
			result.Messages.First().ShortMessage.Should().Be("The saved search must be public.");
		}

		[Test]
		public async Task ItShouldReportFailedValidationResultWhenSavedSearchNotFound()
		{
			_objectManager.Setup(x =>
					x.QueryAsync(
						_WORKSPACE_ARTIFACT_ID,
						It.Is<QueryRequest>(y =>
							y.Condition.Contains(_configuration.SavedSearchArtifactId.ToString(CultureInfo.InvariantCulture))),
							It.IsAny<int>(),
							It.IsAny<int>(),
							CancellationToken.None,
							It.IsAny<IProgress<ProgressReport>>()))
								.Returns(Task.FromResult(new QueryResult()));

			// act
			ValidationResult result = await _sut.ValidateAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.IsValid.Should().Be(false);
			result.Messages.First().ErrorCode.Should().Be("20.004");
			result.Messages.First().ShortMessage.Should().Be("Saved search is not available or has been secured from this user. Contact your system administrator.");
		}
	}
}