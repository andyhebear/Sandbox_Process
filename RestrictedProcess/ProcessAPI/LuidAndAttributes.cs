// <copyright file="LuidAndAttributes.cs" company="Nikolay Kostov (Nikolay.IT)">
// Copyright (c) Nikolay Kostov (Nikolay.IT). All Rights Reserved.
// Licensed under the Apache License. See LICENSE in the project root for license information.
// </copyright>

namespace Sandbox_Process.RestrictedProcess
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The LUID_AND_ATTRIBUTES structure represents a locally unique identifier (LUID) and its attributes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LuidAndAttributes
    {
        public Luid Luid;

        public uint Attributes;
    }
}
