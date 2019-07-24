namespace kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto
{
	public class FileTestDto
	{
		public string Filename { get; }
		public string Location { get; }
		public bool InRepository { get; }

		public FileTestDto(
			string filename,
			string location,
			bool inRepository)
		{
			Filename = filename;
			Location = location;
			InRepository = inRepository;
		}
	}
}
