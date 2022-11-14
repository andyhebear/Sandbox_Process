﻿// <copyright file="BasicUiRestrictions.cs" company="Nikolay Kostov (Nikolay.IT)">
// Copyright (c) Nikolay Kostov (Nikolay.IT). All Rights Reserved.
// Licensed under the Apache License. See LICENSE in the project root for license information.
// </copyright>

namespace Sandbox_Process.JobObjects
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains basic user-interface restrictions for a job object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BasicUiRestrictions
    {
        /// <summary>
        /// Gets or sets the restriction class for the user interface.
        /// </summary>
        public uint UIRestrictionsClass { get; set; }
    }
}
