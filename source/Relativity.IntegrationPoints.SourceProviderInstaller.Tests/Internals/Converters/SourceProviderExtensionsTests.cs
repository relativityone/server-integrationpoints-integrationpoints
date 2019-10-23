using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Converters;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Tests.Internals.Converters
{
	[TestFixture]
	public class SourceProviderExtensionsTests
	{
		[Test]
		public void ShouldConvertValidObject()
		{
			// arrange
			SourceProviderConfiguration configuration = new SourceProviderConfiguration()
			{
				AlwaysImportNativeFileNames = true,
				AlwaysImportNativeFiles = true,
				OnlyMapIdentifierToIdentifier = false,
				CompatibleRdoTypes = new List<Guid> { Guid.NewGuid() },
				AvailableImportSettings = new ImportSettingVisibility
				{
					AllowUserToMapNativeFileField = true
				}
			};

			const int applicationID = 8439;
			var sourceProvider = new SourceProvider
			{
				Name = "Source provider name",
				Url = "http://rip.test.com",
				ViewDataUrl = "http://rip.test.com/viewData",
				GUID = Guid.NewGuid(),
				ApplicationGUID = Guid.NewGuid(),
				ApplicationID = applicationID,
				Configuration = configuration
			};

			// act
			InstallProviderDto result = sourceProvider.ToInstallProviderDto();

			// assert
			AssertAreEquivalent(sourceProvider, result);
		}

		[Test]
		public void ShouldConvertObjectWithNullConfiguration()
		{
			// arrange
			const int applicationID = 8439;
			var sourceProvider = new SourceProvider
			{
				Name = "Source provider name",
				Url = "http://rip.test.com",
				ViewDataUrl = "http://rip.test.com/viewData",
				GUID = Guid.NewGuid(),
				ApplicationGUID = Guid.NewGuid(),
				ApplicationID = applicationID,
				Configuration = null
			};

			// act
			InstallProviderDto result = sourceProvider.ToInstallProviderDto();

			// assert
			AssertAreEquivalent(sourceProvider, result);
		}

		[Test]
		public void ShouldConvertNullObject()
		{
			// arrange
			SourceProvider sourceProvider = null;

			// act
			InstallProviderDto result = sourceProvider.ToInstallProviderDto();

			// assert
			result.Should().BeNull("because source object was null");
		}

		private void AssertAreEquivalent(
			SourceProvider expectedObject,
			InstallProviderDto actualObject)
		{
			if (actualObject == null)
			{
				expectedObject.Should().BeNull("because actual object was null");
				return;
			}

			actualObject.Name.Should()
				.Be(expectedObject.Name);
			actualObject.Url.Should()
				.Be(expectedObject.Url);
			actualObject.ViewDataUrl.Should()
				.Be(expectedObject.ViewDataUrl);
			actualObject.ApplicationID.Should()
				.Be(expectedObject.ApplicationID);
			actualObject.ApplicationGUID.Should()
				.Be(expectedObject.ApplicationGUID);
			actualObject.GUID.Should()
				.Be(expectedObject.GUID);

			AssertAreEquivalent(expectedObject.Configuration, actualObject.Configuration);
		}

		private void AssertAreEquivalent(
			SourceProviderConfiguration expectedObject,
			InstallProviderConfigurationDto actualObject)
		{
			if (actualObject == null)
			{
				expectedObject.Should().BeNull("because actual object was null");
				return;
			}

			actualObject.AlwaysImportNativeFileNames.Should()
				.Be(expectedObject.AlwaysImportNativeFileNames);
			actualObject.AlwaysImportNativeFiles.Should()
				.Be(expectedObject.AlwaysImportNativeFiles);
			actualObject.OnlyMapIdentifierToIdentifier.Should()
				.Be(expectedObject.OnlyMapIdentifierToIdentifier);
			actualObject.CompatibleRdoTypes.Should()
				.BeEquivalentTo(expectedObject.CompatibleRdoTypes);

			actualObject.AllowUserToMapNativeFileField.Should()
				.Be(expectedObject.AvailableImportSettings.AllowUserToMapNativeFileField);
		}
	}
}
