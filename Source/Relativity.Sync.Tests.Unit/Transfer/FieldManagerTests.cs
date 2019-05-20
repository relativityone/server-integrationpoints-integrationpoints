using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;
using Enumerable = System.Linq.Enumerable;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	public class FieldManagerTests
	{
		private Mock<ISynchronizationConfiguration> _configuration;
		private Mock<IDocumentFieldRepository> _documentFieldRepository;
		private Mock<ISpecialFieldBuilder> _builder1;
		private Mock<ISpecialFieldBuilder> _builder2;
		private Sync.Transfer.FieldInfo _documentSpecialField;
		private Sync.Transfer.FieldInfo _nonDocumentSpecialField1;
		private Sync.Transfer.FieldInfo _nonDocumentSpecialField2;
		private FieldMap _mappedField1;
		private FieldMap _mappedField2;
		private FieldManager _instance;
		private const RelativityDataType _NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE = RelativityDataType.WholeNumber;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 123;
		private const string _DOCUMENT_SPECIAL_FIELD_NAME = "DocumentSpecialField";
		private const string _NON_DOCUMENT_SPECIAL_FIELD_1_NAME = "NonDocumentSpecialField1";
		private const string _NON_DOCUMENT_SPECIAL_FIELD_2_NAME = "NonDocumentSpecialField2";
		private const string _MAPPED_FIELD_1_NAME = "MappedField1";
		private const string _MAPPED_FIELD_2_NAME = "MappedField2";

		[SetUp]
		public void SetUp()
		{
			_documentSpecialField = new Sync.Transfer.FieldInfo {DisplayName = _DOCUMENT_SPECIAL_FIELD_NAME, IsDocumentField = true};
			_nonDocumentSpecialField1 = new Sync.Transfer.FieldInfo {DisplayName = _NON_DOCUMENT_SPECIAL_FIELD_1_NAME, IsDocumentField = false};
			_nonDocumentSpecialField2 = new Sync.Transfer.FieldInfo {DisplayName = _NON_DOCUMENT_SPECIAL_FIELD_2_NAME, IsDocumentField = false};
			_mappedField1 = new FieldMap {SourceField = new FieldEntry {DisplayName = _MAPPED_FIELD_1_NAME}};
			_mappedField2 = new FieldMap {SourceField = new FieldEntry {DisplayName = _MAPPED_FIELD_2_NAME}};

			_builder1 = new Mock<ISpecialFieldBuilder>();
			_builder1.Setup(b => b.BuildColumns()).Returns(new[] {_nonDocumentSpecialField1, _documentSpecialField});
			_builder2 = new Mock<ISpecialFieldBuilder>();
			_builder2.Setup(b => b.BuildColumns()).Returns(new[] {_nonDocumentSpecialField2});

			_configuration = new Mock<ISynchronizationConfiguration>();
			_configuration.Setup(c => c.FieldMappings).Returns(new[] { _mappedField1, _mappedField2 });
			_configuration.Setup(c => c.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ARTIFACT_ID);
			_documentFieldRepository = new Mock<IDocumentFieldRepository>();
			_documentFieldRepository.Setup(r => r.GetRelativityDataTypesForFieldsByFieldNameAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<ICollection<string>>(), CancellationToken.None))
				.Returns<int, ICollection<string>, CancellationToken>((workspaceId, fieldNames, token) => Task.FromResult(fieldNames.ToDictionary(f => f, _ => _NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE)));

			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, new[] {_builder1.Object, _builder2.Object});
		}

		[Test]
		public void ItShouldReturnSpecialFields()
		{
			IList<Sync.Transfer.FieldInfo> result = _instance.GetSpecialFields().ToList();

			result.Should().Contain(f => f.DisplayName == _DOCUMENT_SPECIAL_FIELD_NAME);
			result.Should().Contain(f => f.DisplayName == _NON_DOCUMENT_SPECIAL_FIELD_1_NAME);
			result.Should().Contain(f => f.DisplayName == _NON_DOCUMENT_SPECIAL_FIELD_2_NAME);
			result.Should().NotContain(f => f.DisplayName == _MAPPED_FIELD_1_NAME);
			result.Should().NotContain(f => f.DisplayName == _MAPPED_FIELD_2_NAME);
		}

		[Test]
		public async Task ItShouldReturnDocumentFields()
		{
			IList<Sync.Transfer.FieldInfo> result = await _instance.GetDocumentFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			result.Should().Contain(f => f.DisplayName == _DOCUMENT_SPECIAL_FIELD_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.DisplayName == _MAPPED_FIELD_1_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.DisplayName == _MAPPED_FIELD_2_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().NotContain(f => f.DisplayName == _NON_DOCUMENT_SPECIAL_FIELD_1_NAME);
			result.Should().NotContain(f => f.DisplayName == _NON_DOCUMENT_SPECIAL_FIELD_2_NAME);
			result.Select(f => f.DocumentFieldIndex).Should().ContainInOrder(Enumerable.Range(0, result.Count));
		}

		[Test]
		public async Task ItShouldReturnAllFields()
		{
			// Act
			IList<Sync.Transfer.FieldInfo> result = await _instance.GetAllFieldsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Should().Contain(f => f.DisplayName == _DOCUMENT_SPECIAL_FIELD_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);

			result.Should().Contain(f => f.DisplayName == _MAPPED_FIELD_1_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.DisplayName == _MAPPED_FIELD_1_NAME).Which.RelativityDataType.Should().Be(_NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE);
			
			result.Should().Contain(f => f.DisplayName == _MAPPED_FIELD_2_NAME).Which.DocumentFieldIndex.Should().BeGreaterOrEqualTo(0);
			result.Should().Contain(f => f.DisplayName == _MAPPED_FIELD_2_NAME).Which.RelativityDataType.Should().Be(_NON_SPECIAL_DOCUMENT_FIELD_RELATIVITY_DATA_TYPE);

			result.Should().Contain(f => f.DisplayName == _NON_DOCUMENT_SPECIAL_FIELD_1_NAME).Which.DocumentFieldIndex.Should().Be(-1);

			result.Should().Contain(f => f.DisplayName == _NON_DOCUMENT_SPECIAL_FIELD_2_NAME).Which.DocumentFieldIndex.Should().Be(-1);
		}

		[Test]
		public void ItShouldReturnSpecialFieldBuilders()
		{
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, new[] {_builder1.Object, _builder2.Object});
			
			_instance.SpecialFieldBuilders.Should().Contain(_builder1.Object);
			_instance.SpecialFieldBuilders.Should().Contain(_builder2.Object);
		}

		[Test]
		public async Task ItShouldNotThrowOnGetAllFieldsWhenNoSpecialFieldBuildersFound()
		{
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			Func<Task<List<Sync.Transfer.FieldInfo>>> action = () => _instance.GetAllFieldsAsync(CancellationToken.None);

			await action.Should().NotThrowAsync().ConfigureAwait(false);
		}

		[Test]
		public void ItShouldNotThrowOnGetSpecialFieldsWhenNoSpecialFieldBuildersFound()
		{
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			Func<IEnumerable<Sync.Transfer.FieldInfo>> action = () => _instance.GetSpecialFields();

			action.Should().NotThrow();
		}

		[Test]
		public async Task ItShouldNotThrowOnGetDocumentFieldsWhenNoSpecialFieldBuildersFound()
		{
			_instance = new FieldManager(_configuration.Object, _documentFieldRepository.Object, Enumerable.Empty<ISpecialFieldBuilder>());

			Func<Task<List<Sync.Transfer.FieldInfo>>> action = () => _instance.GetDocumentFieldsAsync(CancellationToken.None);

			await action.Should().NotThrowAsync().ConfigureAwait(false);
		}
	}
}
