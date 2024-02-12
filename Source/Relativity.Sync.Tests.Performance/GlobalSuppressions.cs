

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
#pragma warning disable RG2001 // Character Per Line

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>", Scope = "module")]
[assembly: SuppressMessage("Design", "RG2013:Namespace Count", Justification = "<Pending>", Scope = "module")]
[assembly: SuppressMessage("Design", "RG2002:Class Count", Justification = "<Pending>", Scope = "module")]
[assembly: SuppressMessage("Design", "RG2007:Explicit Type Declaration", Justification = "<Pending>", Scope = "module")]
[assembly: SuppressMessage("Style", "RG2001:Characters Per Line", Justification = "Auto generated Refit classes", Scope = "module")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "In tests it's helpful to transform all exceptions to make debuggin easier", Scope = "module")]
#pragma warning restore RG2001 // Character Per Line
