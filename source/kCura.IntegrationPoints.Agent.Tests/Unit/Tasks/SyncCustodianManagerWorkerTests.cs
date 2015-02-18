using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.CustodianManager;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests.Unit.Tasks
{
	[TestFixture]
	public class SyncCustodianManagerWorkerTests
	{
		private string jsonParam1 =
			"{\"BatchInstance\":\"2b7bda1b-11c9-4349-b446-ae5c8ca2c408\",\"BatchParameters\":{\"CustodianManagerMap\":{\"9E6D57BEE28D8D4CA9A64765AE9510FB\":\"CN=Middle Manager,OU=Nested,OU=Testing - Users,DC=testing,DC=corp\",\"779561316F4CE44191B150453DE9A745\":\"CN=Top Manager,OU=Testing - Users,DC=testing,DC=corp\",\"2845DA5813991740BA2D6CC6C9765799\":\"CN=Bottom Manager,OU=NestedAgain,OU=Nested,OU=Testing - Users,DC=testing,DC=corp\"},\"CustodianManagerFieldMap\":[{\"SourceField\":{\"DisplayName\":\"CustodianIdentifier\",\"FieldIdentifier\":\"objectguid\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"ManagerIdentidier\",\"FieldIdentifier\":\"distinguishedName\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"FieldMapType\":1}],\"ManagerFieldIdIsBinary\":false,\"ManagerFieldMap\":[{\"SourceField\":{\"DisplayName\":\"mail\",\"FieldIdentifier\":\"mail\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"Email\",\"FieldIdentifier\":\"1040539\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"givenname\",\"FieldIdentifier\":\"givenname\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"First Name\",\"FieldIdentifier\":\"1040546\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":true},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"sn\",\"FieldIdentifier\":\"sn\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"Last Name\",\"FieldIdentifier\":\"1040547\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":true},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"manager\",\"FieldIdentifier\":\"manager\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"Manager\",\"FieldIdentifier\":\"1040548\",\"FieldType\":0,\"IsIdentifier\":false,\"IsRequired\":false},\"FieldMapType\":0},{\"SourceField\":{\"DisplayName\":\"objectguid\",\"FieldIdentifier\":\"objectguid\",\"FieldType\":0,\"IsIdentifier\":true,\"IsRequired\":false},\"DestinationField\":{\"DisplayName\":\"UniqueID\",\"FieldIdentifier\":\"1040555\",\"FieldType\":0,\"IsIdentifier\":true,\"IsRequired\":false},\"FieldMapType\":1}]}}";
		private string jsonParam2 =
			"{\"artifactTypeID\":1000051,\"ImportOverwriteMode\":\"AppendOverlay\",\"CaseArtifactId\":1019127,\"CustodianManagerFieldContainsLink\":\"true\"}";
		private kCura.Apps.Common.Utils.Serializers.ISerializer serializer;

		[TestFixtureSetUp]
		public void Setup()
		{
			serializer = new kCura.Apps.Common.Utils.Serializers.JSONSerializer();
		}

		[Test]
		public void GetParameters_Param1_CorrectValues()
		{
			//ARRANGE
			Job job = GetJob(jsonParam1);
			SyncCustodianManagerWorker task =
				new SyncCustodianManagerWorker(null, null, null, serializer, null, null, null, null);


			//ACT
			MethodInfo dynMethod = task.GetType().GetMethod("GetParameters",
				BindingFlags.NonPublic | BindingFlags.Instance);
			dynMethod.Invoke(task, new object[] { job });


			//ASSERT
			Assert.AreEqual(new Guid("2b7bda1b-11c9-4349-b446-ae5c8ca2c408"), task.GetType().GetProperty("BatchInstance", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task));
			
			List<CustodianManagerMap> _custodianManagerMap = (List<CustodianManagerMap>)task.GetType().GetField("_custodianManagerMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(3, _custodianManagerMap.Count);
			Assert.AreEqual("779561316F4CE44191B150453DE9A745", _custodianManagerMap[1].CustodianID);
			Assert.AreEqual("CN=Bottom Manager,OU=NestedAgain,OU=Nested,OU=Testing - Users,DC=testing,DC=corp", _custodianManagerMap[2].OldManagerID);

			List<FieldMap> _custodianManagerFieldMap = (List<FieldMap>)task.GetType().GetField("_custodianManagerFieldMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(1, _custodianManagerFieldMap.Count);
			Assert.AreEqual(FieldMapTypeEnum.Identifier, _custodianManagerFieldMap[0].FieldMapType);
			Assert.AreEqual("objectguid", _custodianManagerFieldMap[0].SourceField.FieldIdentifier);
			Assert.AreEqual("distinguishedName", _custodianManagerFieldMap[0].DestinationField.FieldIdentifier);

			List<FieldMap> _managerFieldMap = (List<FieldMap>)task.GetType().GetField("_managerFieldMap", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(5, _managerFieldMap.Count);

			bool _managerFieldIdIsBinary = (bool)task.GetType().GetField("_managerFieldIdIsBinary", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(task);
			Assert.AreEqual(false, _managerFieldIdIsBinary);
		}

		[Test]
		public void ReconfigureDestinationSettings_Param2_CorrectValues()
		{
			//ARRANGE
			SyncCustodianManagerWorker task =
				new SyncCustodianManagerWorker(null, null, null, serializer, null, null, null, null);
			task.GetType().GetField("_destinationConfiguration", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(task, jsonParam2);

			//ACT
			MethodInfo dynMethod = task.GetType().GetMethod("ReconfigureImportAPISettings",
				BindingFlags.NonPublic | BindingFlags.Instance);
			object newDestinationConfiguration = dynMethod.Invoke(task, new object[] { 1014321 });

			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(newDestinationConfiguration.ToString());


			//ASSERT
			Assert.AreEqual(1014321, importSettings.ObjectFieldIdListContainsArtifactId[0]);
			Assert.AreEqual(ImportOverwriteModeEnum.OverlayOnly, importSettings.ImportOverwriteMode);
			Assert.AreEqual(false, importSettings.CustodianManagerFieldContainsLink);
			Assert.AreEqual(1000051, importSettings.ArtifactTypeId);
			Assert.AreEqual(1019127, importSettings.CaseArtifactId);
		}

		private Job GetJob(string jobDetails)
		{
			return JobHelper.GetJob(1, 1, 1, 111, 222, TaskType.SyncCustodianManagerWorker, new DateTime(), null, jobDetails,
				0, new DateTime(), 1, null, null);
		}
	}
}
