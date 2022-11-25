﻿// <copyright file="TokenMandatoryLabel.cs" company="Nikolay Kostov (Nikolay.IT)">
// Copyright (c) Nikolay Kostov (Nikolay.IT). All Rights Reserved.
// Licensed under the Apache License. See LICENSE in the project root for license information.
// </copyright>

namespace Sandbox_Process.RestrictedProcess
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The structure specifies the mandatory integrity level for a token.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct TokenMandatoryLabel
    {
        public SidAndAttributes Label;
    }
}
