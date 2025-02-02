﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx.Core.Functions
{
    // A Texl function capable of async invokes. 
    // This only lives in Core to enable Fx.Json funcs impl (which doesn't depend on interpreter).    
    internal interface IAsyncTexlFunction
    {
        Task<FormulaValue> InvokeAsync(FormulaValue[] args, CancellationToken cancellationToken);
    }
}
