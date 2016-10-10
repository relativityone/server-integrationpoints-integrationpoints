using System;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using NSubstitute;

namespace kCura.IntegrationPoints.EventHandlers.Tests.MassOperations
{
	public class MassCopyIntegrationPointHelper
	{
		public static Data.IntegrationPoint CreateIntegrationPoint(bool logErrors, string name, int destinationProvider, string destinationConfiguration,
			string sourceConfiguration, string fieldMappings, string emailNotificationRecipients, int sourceProvider, Choice overwriteFields)
		{
			var ip = Substitute.For<Data.IntegrationPoint>();
			ip.GetField<bool?>(new Guid(IntegrationPointFieldGuids.LogErrors)).Returns(logErrors);
			ip.GetField<string>(new Guid(IntegrationPointFieldGuids.Name)).Returns(name);
			ip.GetField<int?>(new Guid(IntegrationPointFieldGuids.DestinationProvider)).Returns(destinationProvider);
			ip.GetField<string>(new Guid(IntegrationPointFieldGuids.DestinationConfiguration)).Returns(destinationConfiguration);
			ip.GetField<string>(new Guid(IntegrationPointFieldGuids.SourceConfiguration)).Returns(sourceConfiguration);
			ip.GetField<string>(new Guid(IntegrationPointFieldGuids.FieldMappings)).Returns(fieldMappings);
			ip.GetField<string>(new Guid(IntegrationPointFieldGuids.EmailNotificationRecipients)).Returns(emailNotificationRecipients);
			ip.GetField<int?>(new Guid(IntegrationPointFieldGuids.SourceProvider)).Returns(sourceProvider);
			ip.GetField<Choice>(new Guid(IntegrationPointFieldGuids.OverwriteFields)).Returns(overwriteFields);

			return ip;
		}

		public static Data.IntegrationPoint CreateExampleIntegrationPoint()
		{
			return CreateIntegrationPoint(true, "example_name", 99, "example_destination_configuration", "example_source_configuration", "example_field_mappings",
				"example_email_notification", 55, new Choice(Guid.Empty, "Append Only"));
		}

		public static void MockIntegrationPointName(Data.IntegrationPoint integrationPoint, string name)
		{
			integrationPoint.GetField<string>(new Guid(IntegrationPointFieldGuids.Name)).Returns(name);
		}
	}
}