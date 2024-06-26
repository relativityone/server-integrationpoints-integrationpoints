﻿using System;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public interface ITestHelper : ICPHelper
    {
        string RelativityUserName { get; }

        string RelativityPassword { get; }

        T CreateProxy<T>() where T : IDisposable;

        T CreateProxy<T>(string username) where T : IDisposable;
    }
}
