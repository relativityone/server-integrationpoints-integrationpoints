﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.Agent;
using kCura.IntegrationPoints.Agent;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Telemetry.APM;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    public class FakeAgent : Agent
    {
        private readonly bool _shouldRunOnce;
        private readonly IWindsorContainer _container;

        public Func<Job, TaskResult> ProcessJobMockFunc { get; set; }

        public Func<IEnumerable<int>> GetResourceGroupIDsMockFunc { get; set; }

        public List<long> ProcessedJobIds { get; } = new List<long>();

        public FakeAgent(
            IWindsorContainer container,
            AgentTest agent,
            IAgentHelper helper,
            IAgentService agentService = null,
            IJobService jobService = null,
            IScheduleRuleFactory scheduleRuleFactory = null,
            IQueueJobValidator queueJobValidator = null,
            IQueueQueryManager queryManager = null,
            IKubernetesMode kubernetesMode = null,
            IAPILog logger = null,
            IDateTime dateTime = null,
            IConfig config = null,
            IAPM apm = null,
            IDbContextFactory dbContextFactory = null,
            IRelativityObjectManagerFactory relativityObjectManagerFactory = null,
            bool shouldRunOnce = true)
            : base(
                agent.AgentGuid,
                agentService,
                jobService,
                scheduleRuleFactory,
                queueJobValidator,
                queryManager,
                kubernetesMode,
                dateTime,
                logger,
                config,
                apm,
                dbContextFactory,
                relativityObjectManagerFactory)
        {
            _shouldRunOnce = shouldRunOnce;
            _container = container;

            // Agent ID setter is marked as private
            typeof(kCura.Agent.AgentBase).GetField("_agentID", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(this, agent.ArtifactId);

            // IAgentHelper setter is marked as private
            typeof(kCura.Agent.AgentBase).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(this, helper);

            // 'Enabled = true' triggered Execute() immediately. I needed to set the field only to enable getting job from the queue
            typeof(kCura.Agent.AgentBase).GetField("_enabled", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(this, true);

            GetResourceGroupIDsFunc = GetResourceGroupIDsMockFunc != null
                ? GetResourceGroupIDsMockFunc
                : () => Const.Agent.RESOURCE_GROUP_IDS;
        }

        public static FakeAgent Create(RelativityInstanceTest instance, IWindsorContainer container, bool shouldRunOnce = true)
        {
            AgentTest agent = instance.Helpers.AgentHelper.CreateIntegrationPointAgent();

            var fakeAgent = new FakeAgent(
                container,
                agent,
                container.Resolve<IAgentHelper>(),
                queryManager: container.Resolve<IQueueQueryManager>(),
                jobService: container.Resolve<IJobService>(),
                shouldRunOnce: shouldRunOnce,
                kubernetesMode: container.Resolve<IKubernetesMode>(),
                dateTime: container.Resolve<IDateTime>(),
                config: container.Resolve<IConfig>(),
                apm: container.Resolve<IAPM>(),
                dbContextFactory: container.Resolve<IDbContextFactory>(),
                relativityObjectManagerFactory: container.Resolve<IRelativityObjectManagerFactory>());

            container
                .Register(Component.For<IRemovableAgent>().UsingFactoryMethod(c => fakeAgent)
                .Named(Guid.NewGuid().ToString())
                .IsDefault());

            return fakeAgent;
        }

        public static FakeAgent CreateWithEmptyProcessJob(RelativityInstanceTest instance, IWindsorContainer container, bool shouldRunOnce = true)
        {
            FakeAgent agent = Create(instance, container, shouldRunOnce);

            agent.ProcessJobMockFunc = (Job job) => new TaskResult { Status = TaskStatusEnum.Success };

            return agent;
        }

        protected override IWindsorContainer CreateAgentLevelContainer()
        {
            return _container;
        }

        protected override TaskResult ProcessJob(Job job)
        {
            ProcessedJobIds.Add(job.JobId);

            bool shouldRunOnceWasSet = (bool)typeof(ScheduleQueueAgentBase).GetField("_shouldReadJobOnce", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(this);
            if (_shouldRunOnce && !shouldRunOnceWasSet)
            {
                typeof(AgentBase).GetMethod("StopTimer", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(this, null);

                typeof(ScheduleQueueAgentBase).GetField("_shouldReadJobOnce", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, _shouldRunOnce);
            }

            if (ProcessJobMockFunc != null)
            {
                return ProcessJobMockFunc(job);
            }

            _container.Register(Component.For<Job>().UsingFactoryMethod(k => job).Named(Guid.NewGuid().ToString()).IsDefault());

            return base.ProcessJob(job);
        }

        public void MarkAgentToBeRemoved()
        {
            ToBeRemoved = true;
        }
        #region Verification

        public void VerifyJobsWereProcessed(IEnumerable<long> jobs)
        {
            ProcessedJobIds.Should().Contain(jobs);
        }

        public void VerifyJobsWereNotProcessed(IEnumerable<long> jobs)
        {
            ProcessedJobIds.Should().NotContain(jobs);
        }

        public void VerifyJobWasProcessedAtFirst(long jobId)
        {
            ProcessedJobIds.First().Should().Be(jobId);
        }

        #endregion
    }
}
