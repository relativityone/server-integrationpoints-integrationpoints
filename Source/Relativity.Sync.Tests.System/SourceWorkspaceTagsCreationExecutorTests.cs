//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using kCura.Relativity.Client;
//using kCura.Relativity.Client.DTOs;
//using Moq;
//using NUnit.Framework;
//using Platform.Keywords.RSAPI;
//using Relativity.API;
//using Relativity.Services;
//using Relativity.Services.Permission;
//using Relativity.Sync.Authentication;
//using Relativity.Sync.Configuration;
//using Relativity.Sync.KeplerFactory;
//using Relativity.Sync.Tests.System.Stub;
//using TextCondition = kCura.Relativity.Client.TextCondition;
//using TextConditionEnum = kCura.Relativity.Client.TextConditionEnum;
//using User = kCura.Relativity.Client.DTOs.User;
//using UsernamePasswordCredentials = kCura.Relativity.Client.UsernamePasswordCredentials;

//namespace Relativity.Sync.Tests.System
//{
//	[TestFixture]
//	public sealed class SourceWorkspaceTagsCreationExecutorTests : IDisposable
//	{
//		private ServicesManagerStub _servicesManager;
//		private ProvideServiceUrisStub _provideServiceUris;
//		private IRSAPIClient _client;
//		private Workspace _workspace;

//		[OneTimeSetUp]
//		public void SuiteSetup()
//		{
//			_servicesManager = new ServicesManagerStub();
//			_provideServiceUris = new ProvideServiceUrisStub();
//			_client = new RSAPIClient(AppSettings.RelativityServicesUrl, new UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword));
//			_workspace = CreateWorkspaceAsync().Result;
//		}

//		[OneTimeTearDown]
//		public void SuiteTeardown()
//		{
//			DeleteWorkspace(_workspace.ArtifactID);
//			_client?.Dispose();
//			_client = null;
//		}
//		public void Dispose()
//		{
//			_client?.Dispose();
//		}
//	}
//}
