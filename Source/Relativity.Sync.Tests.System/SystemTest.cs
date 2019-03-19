using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Moq;
using NUnit.Framework;
using Platform.Keywords.RSAPI;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Permission;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.System.Stub;
using BearerTokenCredentials = Relativity.Services.ServiceProxy.BearerTokenCredentials;
using TextCondition = kCura.Relativity.Client.TextCondition;
using TextConditionEnum = kCura.Relativity.Client.TextConditionEnum;
using User = kCura.Relativity.Client.DTOs.User;
using UsernamePasswordCredentials = kCura.Relativity.Client.UsernamePasswordCredentials;

namespace Relativity.Sync.Tests.System
{
	public abstract class SystemTest : IDisposable 
	{
		private readonly List<Workspace> _workspaces = new List<Workspace>();
		protected IRSAPIClient Client { get; private set; }

		[OneTimeSetUp]
		public void SuiteSetup()
		{
			Client = new RSAPIClient(AppSettings.RelativityServicesUrl, new UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword));
			ChildSuiteSetup();
		}

		[OneTimeTearDown]
		public void SuiteTeardown()
		{
			ChildSuiteTeardown();
			Client.Repositories.Workspace.Delete(_workspaces);
			Client?.Dispose();
			Client = null;
		}

		protected virtual void ChildSuiteSetup()
		{
		}

		protected virtual void ChildSuiteTeardown()
		{
		}

		protected async Task<Workspace> CreateWorkspaceAsync()
		{
			string name = $"{Guid.NewGuid().ToString()}";
			Workspace newWorkspace = new Workspace { Name = name };
			Workspace template = FindWorkspace("Relativity Starter Template");
			ProcessOperationResult createWorkspaceResult = Client.Repositories.Workspace.CreateAsync(template.ArtifactID, newWorkspace);

			if (!createWorkspaceResult.Success)
			{
				throw new InvalidOperationException($"Failed to create workspace '{newWorkspace.Name}': {createWorkspaceResult.Message}");
			}

			ProcessInformation processInfo;
			do
			{
				processInfo = Client.GetProcessState(Client.APIOptions, createWorkspaceResult.ProcessID);
				const int millisecondsDelay = 100;
				await Task.Delay(millisecondsDelay).ConfigureAwait(false);
			}
			while (processInfo.State == ProcessStateValue.Running || processInfo.State == ProcessStateValue.RunningWarning);

			if (processInfo.State != ProcessStateValue.Completed)
			{
				throw new InvalidOperationException($"Workspace creation did not completed successfully: {processInfo.Message}");
			}

			Workspace workspace = FindWorkspace(name);
			_workspaces.Add(workspace);
			return workspace;
		}

		protected Workspace FindWorkspace(string name)
		{
			var workspaceNameCondition = new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.EqualTo, name);
			var query = new Query<Workspace>
			{
				Condition = workspaceNameCondition
			};
			query.Fields.Add(new FieldValue("*"));
			Workspace workspace = Client.Repositories.Workspace.Query(query).Results[0].Artifact;
			return workspace;
		}

		protected ServiceFactory CreateServideFactory()
		{
			Credentials credentials = new Services.ServiceProxy.UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword);
			ServiceFactorySettings settings = new ServiceFactorySettings(AppSettings.RelativityServicesUrl, AppSettings.RelativityRestUrl, credentials);
			return new ServiceFactory(settings);
		}

		protected virtual void Dispose(bool disposing)
		{
			Client?.Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
