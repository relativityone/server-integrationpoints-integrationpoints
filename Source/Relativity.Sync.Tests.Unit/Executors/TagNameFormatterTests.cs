using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class TagNameFormatterTests
	{
		private Mock<IAPILog> _logger;
		private TagNameFormatter _sut;

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<IAPILog>();
			_sut = new TagNameFormatter(_logger.Object);
		}

		[Test]
		public void ItShouldShortenTagNameWhenCreating()
		{
			const int maxLength = 255;
			const string destinationInstanceName = "instance";
			const int destinationWorkspaceArtifactId = 3;
			const string tooLongDestinationWorkspaceName = "TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongName" +
					"TooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLongNameTooLo";

			// act
			string name = _sut.FormatWorkspaceDestinationTagName(destinationInstanceName, 
				tooLongDestinationWorkspaceName, destinationWorkspaceArtifactId);

			// assert
			Assert.LessOrEqual(name.Length, maxLength);
		}
	}
}