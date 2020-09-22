namespace kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto
{
	public class FileTestDto
	{
		public string Filename { get; }
		public string Location { get; }
		public string Identifier { get; }
		public bool InRepository { get; }

		public FileTestDto(
			string filename,
			string location,
			string identifier,
			bool inRepository)
		{
			Filename = filename;
			Location = location;
			Identifier = identifier;
			InRepository = inRepository;
		}
	}
}
