using Moq;
using NUnit.Framework;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class TagNameFormatterTests
	{
		private TagNameFormatter _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new TagNameFormatter(new EmptyLogger());
		}

		[Test]
		public void ItShouldShortenTagNameWhenCreating()
		{
			const int maxLength = 255;
			const string destinationInstanceName = "instance";
			const int destinationWorkspaceArtifactId = 3;
			const string tooLongDestinationWorkspaceName =
				"TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongName" +
				"TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLo";

			// act
			string name = _sut.FormatWorkspaceDestinationTagName(destinationInstanceName,
				tooLongDestinationWorkspaceName, destinationWorkspaceArtifactId);

			// assert
			Assert.LessOrEqual(name.Length, maxLength);
		}
	}
}