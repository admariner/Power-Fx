﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.PowerFx.Core.Localization;

namespace Microsoft.PowerFx.Connectors.Localization
{
    internal static class ConnectorStringResources
    {
        internal static readonly IExternalStringResources ConnectorResources = new PowerFxStringResources("Microsoft.PowerFx.Connectors.strings.PowerFxConnectorResources", typeof(ConnectorStringResources).Assembly);

        internal static ErrorResourceKey New(string key) => new (key, ConnectorResources);

        internal static readonly ErrorResourceKey WarnDeprecatedFunction = ConnectorStringResources.New("WarnDeprecatedFunction");
        internal static readonly ErrorResourceKey WarnFunctionUseBinary = ConnectorStringResources.New("WarnFunctionUseBinary");
        internal static readonly ErrorResourceKey WarnFunctionUseByte = ConnectorStringResources.New("WarnFunctionUseByte");
        internal static readonly ErrorResourceKey WarnInvalidProperty = ConnectorStringResources.New("WarnInvalidProperty");
        internal static readonly ErrorResourceKey WarnInvalidFunction = ConnectorStringResources.New("WarnInvalidFunction");
        internal static readonly ErrorResourceKey WarnUnsupportedODataVersion = ConnectorStringResources.New("WarnUnsupportedODataVersion");
        internal static readonly ErrorResourceKey WarnInvalidPagingOption = ConnectorStringResources.New("WarnInvalidPagingOption");
    }
}
