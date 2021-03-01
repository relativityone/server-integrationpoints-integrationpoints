using System.Collections.Generic;

namespace Relativity.Sync.RDOs.Framework
{
    internal interface IRdoGuidProvider
    {
        /// <summary>
        /// Thread safe, cached service. Should be registered as a singleton
        /// </summary>
        /// <typeparam name="TRdoType"></typeparam>
        /// <returns></returns>
        RdoTypeInfo GetValue<TRdoType>() where TRdoType : IRdoType;
    }
}