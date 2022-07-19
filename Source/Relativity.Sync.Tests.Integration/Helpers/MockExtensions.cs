using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Tests.Integration.Helpers
{
    internal static class MockExtensions
    {
        /// <summary>
        /// Performs setup on an <see cref="IObjectManager"/> mock so the return value of
        /// <see cref="IObjectManager.QuerySlimAsync(int, QueryRequest, int, int, CancellationToken)"/>
        /// can be generated from the relevant <see cref="QueryRequest"/>.
        /// </summary>
        /// <param name="setup">Result of a <see cref="Mock{T}.Setup"/> invocation</param>
        /// <param name="queryConverter">Method to convert a request into a response</param>
        /// <returns>Expectation from the mock, for further processing</returns>
        public static IReturnsResult<IObjectManager> ReturnsQueryResultSlimAsync(
            this ISetup<IObjectManager, Task<QueryResultSlim>> setup,
            Func<QueryRequest, QueryResultSlim> queryConverter)
        {
            return setup.ReturnsAsync<int, QueryRequest, int, int, CancellationToken, IObjectManager, QueryResultSlim>(
                (ws, req, st, len, ct) => queryConverter(req));
        }

        /// <summary>
        /// Performs setup on an <see cref="IObjectManager"/> mock so the return value of
        /// <see cref="IObjectManager.QueryAsync(int, QueryRequest, int, int)"/>
        /// can be generated from the relevant <see cref="QueryRequest"/>.
        /// </summary>
        /// <param name="setup">Result of a <see cref="Mock{T}.Setup"/> invocation</param>
        /// <param name="queryConverter">Method to convert a request into a response</param>
        /// <returns>Expectation from the mock, for further processing</returns>
        public static IReturnsResult<IObjectManager> ReturnsQueryResultAsync(
            this ISetup<IObjectManager, Task<QueryResult>> setup,
            Func<QueryRequest, QueryResult> queryConverter)
        {
            return setup.ReturnsAsync<int, QueryRequest, int, int, IObjectManager, QueryResult>(
                (ws, req, st, len) => queryConverter(req));
        }
    }
}
