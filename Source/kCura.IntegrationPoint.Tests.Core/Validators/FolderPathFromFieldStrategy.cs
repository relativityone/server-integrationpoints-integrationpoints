using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using NUnit.Framework;

	public class FolderPathFromFieldStrategy : FolderPathStrategyWithCache
	{
		private readonly string _folderPathfieldName;

		public FolderPathFromFieldStrategy(string folderPathfieldName)
		{
			_folderPathfieldName = folderPathfieldName;
		}

		protected override string GetFolderPathInternal(Document document)
		{
			Assert.That(document[_folderPathfieldName].Value, Is.Not.Null, $"Document {document[IntegrationPoints.Data.DocumentFields.ControlNumber].Value} does not have folder path defined");

			return document[_folderPathfieldName].Value.ToString();
		}
	}
}