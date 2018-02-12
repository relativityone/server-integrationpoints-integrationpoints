namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using System.Data;
	using System.Data.SqlClient;
	using global::Relativity.API;
	using NUnit.Framework;
	using Relativity.Client.DTOs;

	public class DocumentNativesAndInRepositoryValidator : DocumentNativesValidator
	{
		private const string _FILE_QUERY = "SELECT [File].* FROM [File] WHERE Type = 0 AND DocumentArtifactID = @DocumentArtifactID";

		private readonly IDBContext _targetDbContext;
		private readonly bool _expectInRepository;

		public DocumentNativesAndInRepositoryValidator(IDBContext targetDbContext, bool expectHasNatives, bool expectInRepository) : base(expectHasNatives)
		{
			_targetDbContext = targetDbContext;
			_expectInRepository = expectInRepository;
		}

		public override void ValidateDocument(Document actualDocument, Document expectedDocument)
		{
			base.ValidateDocument(actualDocument, expectedDocument);

			DataTable actualDocumentFilesDataTable = _targetDbContext.ExecuteSqlStatementAsDataTable(_FILE_QUERY, new[] { new SqlParameter("@DocumentArtifactID", actualDocument.ArtifactID) });

			if (!ShouldExpectNativesForDocument(expectedDocument))
			{
				Assert.That(actualDocumentFilesDataTable.Rows.Count, Is.Zero);
				return;
			}

			Assert.That(actualDocumentFilesDataTable.Rows.Count, Is.EqualTo(1));

			DataRow actualDocumentFileRow = actualDocumentFilesDataTable.Rows[0];
			Assert.That(actualDocumentFileRow["InRepository"], Is.EqualTo(_expectInRepository));
		}
	}
}