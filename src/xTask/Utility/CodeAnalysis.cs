// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Diagnostics.CodeAnalysis;

#if NETFRAMEWORK

/// <summary>Applied to a method that will never return under any circumstance.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class DoesNotReturnAttribute : Attribute { }

#endif
