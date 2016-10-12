using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Artifact = kCura.Relativity.Client.Artifact;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
	[TestFixture]
	public class RdoSynchronizerPushTests
	{
		private RdoSynchronizerPush _rdoSynchronizerPush;
		private IRelativityFieldQuery _relativityFieldQuery;
		private IImportApiFactory _importApiFactory;
		private IImportAPI _importApi;


		[SetUp]
		public void Setup()
		{
			_relativityFieldQuery = NSubstitute.Substitute.For<IRelativityFieldQuery>();
			_importApiFactory = NSubstitute.Substitute.For<IImportApiFactory>();
			_importApi = NSubstitute.Substitute.For<IExtendedImportAPI>();
			var helper = Substitute.For<IHelper>();
			_rdoSynchronizerPush = new RdoSynchronizerPush(_relativityFieldQuery, _importApiFactory, helper);
		}

		/// <summary>
		/// Test whether options are parsed correctly when getting the mappable fields
		/// </summary>
		[Test]
		public void GetFields_CorrectOptionsPassed()
		{
			int artifactTypeId = 123;
			int caseArtifactId = 456;

			string options = String.Format("{{Provider:'relativity', WebServiceUrl:'WebServiceUrl', ArtifactTypeId:{0}, CaseArtifactId:{1}}}", artifactTypeId, caseArtifactId);
			List<Artifact> fields = new List<Artifact>();
			IEnumerable<Field> mappableFields = new List<Field>();

			_relativityFieldQuery.GetFieldsForRdo(Arg.Is(artifactTypeId))
				.Returns(fields);

			_importApiFactory.GetImportAPI(Arg.Any<ImportSettings>())
				.Returns(_importApi);

			_importApi.GetWorkspaceFields(caseArtifactId, artifactTypeId).Returns(mappableFields);

			IEnumerable<FieldEntry> results = _rdoSynchronizerPush.GetFields(options);

			_relativityFieldQuery
				.Received(1)
				.GetFieldsForRdo(Arg.Is(artifactTypeId));
			_importApiFactory
				.Received(1)
				.GetImportAPI(Arg.Any<ImportSettings>());
			_importApi
				.Received(1)
				.GetWorkspaceFields(caseArtifactId, artifactTypeId);
		}
	}
}
