using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Tests.Unit
{
    internal class FakeRdoManager : IRdoManager
    {
        private readonly IRdoGuidProvider _guidProvider = new RdoGuidProvider();
        private readonly Queue<IRdoType> _rdosForGetAsync = new Queue<IRdoType>();
        private readonly Queue<int> _artifactIdsForCreate = new Queue<int>();

        private readonly int _defaultArtifactId;

        public FakeRdoManager(int defaultArtifactId)
        {
            _defaultArtifactId = defaultArtifactId;

            Mock.Setup(x => x.GetAsync<SyncBatchRdo>(It.IsAny<int>(), defaultArtifactId))
                .ReturnsAsync(() => new SyncBatchRdo{ArtifactId = defaultArtifactId});
        }

        public Mock<IRdoManager> Mock { get; } = new Mock<IRdoManager>();
        
        
        
        public Task EnsureTypeExistsAsync<TRdo>(int workspaceId) where TRdo : IRdoType
        {
            Mock.Object.EnsureTypeExistsAsync<TRdo>(workspaceId);
            return Task.CompletedTask;
        }

        public void SetResultForGetAsync(IRdoType rdo)
        {
            _rdosForGetAsync.Enqueue(rdo);
        }
        
        public Task<TRdo> GetAsync<TRdo>(int workspaceId, int artifactId, params Expression<Func<TRdo, object>>[] fields) where TRdo : IRdoType, new()
        {
            Task<TRdo> result = Mock.Object.GetAsync(workspaceId, artifactId, fields);
            
            if (_rdosForGetAsync.Any())
            {
                return Task.FromResult((TRdo)_rdosForGetAsync.Dequeue());
            }
            
            return result;
        }

        public void EnqueueArtifactIdForCreate(int artifactId)
        {
            _artifactIdsForCreate.Enqueue(artifactId);
        }
        
        public Task CreateAsync<TRdo>(int workspaceId, TRdo rdo, int? parentObjectId = null) where TRdo : IRdoType
        {
            if (_artifactIdsForCreate.Any())
            {
                rdo.ArtifactId = _artifactIdsForCreate.Dequeue();
            }
            else
            {
                rdo.ArtifactId = _defaultArtifactId;
            }
            return Mock.Object.CreateAsync(workspaceId, rdo, parentObjectId);
        }

        public Task SetValuesAsync<TRdo>(int workspaceId, TRdo rdo) where TRdo : IRdoType
        {
            return Mock.Object.SetValuesAsync(workspaceId, rdo);
        }

        public Task SetValueAsync<TRdo, TValue>(int workspaceId, TRdo rdo, Expression<Func<TRdo, TValue>> expression, TValue value) where TRdo : IRdoType
        {
            var fieldGuid = _guidProvider.GetGuidFromFieldExpression(expression);
            var typeInfo = _guidProvider.GetValue<TRdo>();
            typeInfo.Fields[fieldGuid].PropertyInfo.SetValue(rdo, value);

            Mock.Object.SetValueAsync(workspaceId, rdo, expression, value);
            
            return Task.CompletedTask;
        }
    }
}