using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.Data.RowDataGateway;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using kCura.ScheduleQueue.Core.TimeMachine;
using NSubstitute;
using NUnit.Framework;
using IDBContext = Relativity.API.IDBContext;
using kCura.Apps.Common.Config;

namespace kCura.ScheduleQueue.Core.Tests.Integration.Services
{
	[TestFixture]
	public class JobServiceTests
	{
		[Test]
		[Explicit]
		public void jobService_CreateUnscheduledJob()
		{
			int workspaceID = 1015641;
			int relatedObjectArtifactID = 1111111;
			string taskType = "MyTestTask";
			Guid agentGuid = new Guid("D65F5774-6572-49F0-91C4-28161A75DF0D");

			var agentHelper = NSubstitute.Substitute.For<Relativity.API.IAgentHelper>();
			IDBContext eddsContext = new TestDBContextHelper().GetEDDSDBContext();
			agentHelper.GetDBContext(Arg.Any<int>()).Returns(eddsContext);
			//AgentInformation ai = jobService.GetAgentInformation(new Guid("08C0CE2D-8191-4E8F-B037-899CEAEE493D")); //Integration Points agent
			IAgentService agentService = new AgentService(agentHelper, agentGuid); //RLH agent
			var jobService = new JobService(agentService, agentHelper);

			Job jobOld = jobService.GetJob(workspaceID, relatedObjectArtifactID, taskType);
			if (jobOld != null) jobService.DeleteJob(jobOld.JobId);
			Job job = jobService.CreateJob(workspaceID, relatedObjectArtifactID, taskType, DateTime.UtcNow, "My Test Job Detail", 1212121);
			Job job1 = jobService.GetJob(job.JobId);
			Job job2 = jobService.GetJob(workspaceID, job.RelatedObjectArtifactID, taskType);
			Job job3 = jobService.GetNextQueueJob(new int[] { 1015040 });
			jobService.UnlockJobs(agentService.AgentInformation.AgentID);
			jobService.DeleteJob(job3.JobId);
		}

		[Test]
		[Explicit]
		public void jobService_CreateScheduledJob()
		{
			int workspaceID = 1015641;
			int relatedObjectArtifactID = 1111111;
			string taskType = "MyTestTask";
			IScheduleRule sr = new PeriodicScheduleRule(ScheduleInterval.Immediate, DateTime.Parse("1/1/2001"), DateTime.Parse("1/1/2001 10:30:00").TimeOfDay);
			//Guid agentGuid = new Guid("D65F5774-6572-49F0-91C4-28161A75DF0D");//RLH
			Guid agentGuid = new Guid("08C0CE2D-8191-4E8F-B037-899CEAEE493D");//RIP

			var agentHelper = NSubstitute.Substitute.For<Relativity.API.IAgentHelper>();
			IDBContext eddsContext = new TestDBContextHelper().GetEDDSDBContext();
			agentHelper.GetDBContext(Arg.Any<int>()).Returns(eddsContext);

			IAgentService agentService = new AgentService(agentHelper, agentGuid);
			var jobService = new JobService(agentService, agentHelper);

			Job jobOld = jobService.GetJob(workspaceID, relatedObjectArtifactID, taskType);
			if (jobOld != null) jobService.DeleteJob(jobOld.JobId);
			Job job = jobService.CreateJob(workspaceID, relatedObjectArtifactID, taskType, sr, "My Test Job Detail", 1212121);
			Job job2 = null;
			DateTime dt = DateTime.Now;
			while (DateTime.Now.Subtract(dt).Minutes < 4)
			{
				job2 = jobService.GetNextQueueJob(new int[] { 1015040 });
				if (job2 != null) jobService.FinalizeJob(job2, new DefaultScheduleRuleFactory(), new TaskResult() { Status = TaskStatusEnum.Success, Exceptions = new List<Exception>() });
				Thread.Sleep(1000);
			}
			jobService.DeleteJob(job2.JobId);
		}

		[Test]
		[Explicit]
		public void AgentTimeMachineProvider_Test1()
		{
			int caseID1 = 1015641;
			var agentHelper = NSubstitute.Substitute.For<Relativity.API.IAgentHelper>();

			//	{"CaseID":1015641,"Date":"2010-10-10T10:10:10"}
			TimeMachineStruct tm = new TimeMachineStruct() { CaseID = caseID1, Date = DateTime.Parse("10/10/2010 10:10:10") };
			string serializedString = new JSONSerializer().Serialize(tm);

			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(agentHelper);
			IDBContext c1Context = new TestDBContextHelper().GetDBContext(caseID1);
			agentHelper.GetDBContext(Arg.Is(caseID1)).Returns(c1Context);

			AgentTimeMachineProvider.Current = new DefaultAgentTimeMachineProvider(Guid.Parse("08C0CE2D-8191-4E8F-B037-899CEAEE493D"));

			DateTime utcNow1 = AgentTimeMachineProvider.Current.UtcNow;
			Thread.Sleep(1000);
			DateTime utcNow2 = AgentTimeMachineProvider.Current.UtcNow;

		}

		[Test]
		[Explicit]
		public void xxx()
		{
			PeriodicScheduleRule psr = new PeriodicScheduleRule();
			psr.Interval = ScheduleInterval.Immediate;

			IScheduleRule sr = psr;
			string xml = sr.ToSerializedString();

			string AssemblyName = sr.GetType().Assembly.FullName;
			string AssemblyFileName = Path.GetFileName(sr.GetType().Assembly.Location);
			//			string AssemblyFileName = Path.GetFileName(sr.GetType().Assembly.Location);
			string typename = sr.GetType().AssemblyQualifiedName;
			//typename = sr.GetType().FullName;
			//typename = sr.GetType().ToString();
			//typename = sr.GetType().Name;
			//var myObject1 = (IScheduleRule)Activator.CreateInstance("AssemblyName", "TypeName");

			//var myObject2 = (IScheduleRule)Activator.CreateInstance(type2);

			Type type2 = Type.GetType(typename,
				(name) =>
				{
					// Returns the assembly of the type by enumerating loaded assemblies
					// in the app domain            
					return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName).FirstOrDefault();
				}, null, true);

			XMLSerializerFactory factory = new XMLSerializerFactory();
			ISerializer serializer = factory.GetDeserializer(type2);
			IScheduleRule possibleObject = (IScheduleRule)serializer.Deserialize(type2, xml);

		}

		[Test]
		[Explicit]
		public void xxx2()
		{
			PeriodicScheduleRule psr = new PeriodicScheduleRule();
			psr.Interval = ScheduleInterval.Immediate;

			IScheduleRule sr = psr;
			string xml = sr.ToSerializedString();

			string typename = sr.GetType().AssemblyQualifiedName;

			Type type2 = Type.GetType(typename,
				(name) =>
				{
					return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName).FirstOrDefault();
				}, null, true);

			XMLSerializerFactory factory = new XMLSerializerFactory();
			ISerializer serializer = factory.GetDeserializer(type2);
			IScheduleRule possibleObject = (IScheduleRule)serializer.Deserialize(type2, xml);

		}
	}
}
