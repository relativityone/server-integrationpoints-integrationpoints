﻿using System.Threading;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal interface ISourceWorkspaceDataReaderFactory
	{
		ISourceWorkspaceDataReader CreateNativeSourceWorkspaceDataReader(IBatch batch, CancellationToken token);
		ISourceWorkspaceDataReader CreateImageSourceWorkspaceDataReader(IBatch batch, CancellationToken token);
	}
}