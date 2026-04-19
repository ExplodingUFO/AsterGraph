using System.Reflection;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorDiagnosticsContractsTests
{
    private static readonly Assembly EditorAssembly = typeof(IGraphEditorSession).Assembly;

    [Fact]
    public void IGraphEditorSession_ExposesDiagnosticsProperty()
    {
        var sessionType = typeof(IGraphEditorSession);
        var diagnosticsType = GetRequiredType("AsterGraph.Editor.Diagnostics.IGraphEditorDiagnostics");

        var property = sessionType.GetProperty("Diagnostics", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(diagnosticsType, property!.PropertyType);
        Assert.Null(property.SetMethod);
    }

    [Fact]
    public void IGraphEditorDiagnostics_DefinesInspectionAndRecentDiagnosticsReads()
    {
        var diagnosticsType = GetRequiredType("AsterGraph.Editor.Diagnostics.IGraphEditorDiagnostics");
        var inspectionSnapshotType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorInspectionSnapshot");
        var diagnosticType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorDiagnostic");

        Assert.True(diagnosticsType.IsPublic);
        Assert.True(diagnosticsType.IsInterface);

        var captureMethod = diagnosticsType.GetMethod("CaptureInspectionSnapshot", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(captureMethod);
        Assert.Equal(inspectionSnapshotType, captureMethod!.ReturnType);

        var recentMethod = diagnosticsType.GetMethod("GetRecentDiagnostics", BindingFlags.Public | BindingFlags.Instance, [typeof(int)]);
        Assert.NotNull(recentMethod);
        Assert.Equal(typeof(IReadOnlyList<>).MakeGenericType(diagnosticType), recentMethod!.ReturnType);
    }

    [Fact]
    public void GraphEditorInspectionSnapshot_ReusesExistingSnapshotContracts()
    {
        var inspectionSnapshotType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorInspectionSnapshot");
        var pendingConnectionType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorPendingConnectionSnapshot");
        var statusSnapshotType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorStatusSnapshot");
        var selectionSnapshotType = GetRequiredType("AsterGraph.Editor.Runtime.GraphEditorSelectionSnapshot");
        var viewportSnapshotType = GetRequiredType("AsterGraph.Editor.Runtime.GraphEditorViewportSnapshot");
        var capabilitySnapshotType = GetRequiredType("AsterGraph.Editor.Runtime.GraphEditorCapabilitySnapshot");
        var featureDescriptorType = GetRequiredType("AsterGraph.Editor.Runtime.GraphEditorFeatureDescriptorSnapshot");
        var nodePositionSnapshotType = GetRequiredType("AsterGraph.Editor.Models.NodePositionSnapshot");
        var documentType = GetRequiredType("AsterGraph.Core.Models.GraphDocument");
        var diagnosticType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorDiagnostic");

        Assert.True(inspectionSnapshotType.IsPublic);

        AssertProperty(inspectionSnapshotType, "Document", documentType);
        AssertProperty(inspectionSnapshotType, "Selection", selectionSnapshotType);
        AssertProperty(inspectionSnapshotType, "Viewport", viewportSnapshotType);
        AssertProperty(inspectionSnapshotType, "Capabilities", capabilitySnapshotType);
        AssertProperty(inspectionSnapshotType, "PendingConnection", pendingConnectionType);
        AssertProperty(inspectionSnapshotType, "Status", statusSnapshotType);
        AssertProperty(inspectionSnapshotType, "NodePositions", typeof(IReadOnlyList<>).MakeGenericType(nodePositionSnapshotType));
        AssertProperty(inspectionSnapshotType, "FeatureDescriptors", typeof(IReadOnlyList<>).MakeGenericType(featureDescriptorType));
        AssertProperty(inspectionSnapshotType, "RecentDiagnostics", typeof(IReadOnlyList<>).MakeGenericType(diagnosticType));
    }

    [Fact]
    public void GraphEditorFeatureDescriptorSnapshot_IsPublicAndImmutable()
    {
        var descriptorType = GetRequiredType("AsterGraph.Editor.Runtime.GraphEditorFeatureDescriptorSnapshot");

        Assert.True(descriptorType.IsPublic);
        AssertProperty(descriptorType, "Id", typeof(string));
        AssertProperty(descriptorType, "Category", typeof(string));
        AssertProperty(descriptorType, "IsAvailable", typeof(bool));
    }

    [Fact]
    public void CommandAndMenuDescriptorSnapshots_ArePublicAndDataOnly()
    {
        var commandArgumentType = GetRequiredType("AsterGraph.Editor.Runtime.GraphEditorCommandArgumentSnapshot");
        var commandInvocationType = GetRequiredType("AsterGraph.Editor.Runtime.GraphEditorCommandInvocationSnapshot");
        var commandDescriptorType = GetRequiredType("AsterGraph.Editor.Runtime.GraphEditorCommandDescriptorSnapshot");
        var commandSourceType = GetRequiredType("AsterGraph.Editor.Runtime.GraphEditorCommandSourceKind");
        var menuDescriptorType = GetRequiredType("AsterGraph.Editor.Menus.GraphEditorMenuItemDescriptorSnapshot");

        Assert.True(commandArgumentType.IsPublic);
        Assert.True(commandInvocationType.IsPublic);
        Assert.True(commandDescriptorType.IsPublic);
        Assert.True(commandSourceType.IsPublic);
        Assert.True(commandSourceType.IsEnum);
        Assert.True(menuDescriptorType.IsPublic);

        AssertProperty(commandArgumentType, "Name", typeof(string));
        AssertProperty(commandArgumentType, "Value", typeof(string));
        AssertProperty(commandInvocationType, "CommandId", typeof(string));
        AssertProperty(commandInvocationType, "Arguments", typeof(IReadOnlyList<>).MakeGenericType(commandArgumentType));
        AssertProperty(commandDescriptorType, "Id", typeof(string));
        AssertProperty(commandDescriptorType, "Title", typeof(string));
        AssertProperty(commandDescriptorType, "Group", typeof(string));
        AssertProperty(commandDescriptorType, "IconKey", typeof(string));
        AssertProperty(commandDescriptorType, "DefaultShortcut", typeof(string));
        AssertProperty(commandDescriptorType, "CanExecute", typeof(bool));
        AssertProperty(commandDescriptorType, "IsEnabled", typeof(bool));
        AssertProperty(commandDescriptorType, "Source", commandSourceType);
        AssertProperty(menuDescriptorType, "Command", commandInvocationType);
        AssertProperty(menuDescriptorType, "Children", typeof(IReadOnlyList<>).MakeGenericType(menuDescriptorType));
        Assert.Null(menuDescriptorType.GetProperty("Command")!.PropertyType.GetProperty("Execute"));
    }

    [Fact]
    public void PendingConnectionAndStatusSnapshots_ArePublicAndImmutable()
    {
        var pendingConnectionType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorPendingConnectionSnapshot");
        var statusSnapshotType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorStatusSnapshot");

        Assert.True(pendingConnectionType.IsPublic);
        Assert.True(statusSnapshotType.IsPublic);

        AssertProperty(pendingConnectionType, "HasPendingConnection", typeof(bool));
        AssertProperty(pendingConnectionType, "SourceNodeId", typeof(string));
        AssertProperty(pendingConnectionType, "SourcePortId", typeof(string));

        AssertProperty(statusSnapshotType, "Message", typeof(string));
    }

    [Fact]
    public void AsterGraphEditorOptions_ExposesOptionalInstrumentationConfiguration()
    {
        var instrumentationType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorInstrumentationOptions");
        var optionsType = typeof(AsterGraphEditorOptions);
        var property = optionsType.GetProperty("Instrumentation", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(instrumentationType, property!.PropertyType);
    }

    [Fact]
    public void GraphEditorInstrumentationOptions_UsesHostStandardLoggingAndTracingTypes()
    {
        var instrumentationType = GetRequiredType("AsterGraph.Editor.Diagnostics.GraphEditorInstrumentationOptions");

        Assert.True(instrumentationType.IsPublic);
        AssertProperty(instrumentationType, "LoggerFactory", "Microsoft.Extensions.Logging.ILoggerFactory");
        AssertProperty(instrumentationType, "ActivitySource", typeof(System.Diagnostics.ActivitySource));
    }

    private static Type GetRequiredType(string fullName)
        => AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(fullName, throwOnError: false))
            .FirstOrDefault(type => type is not null)
            ?? throw new Xunit.Sdk.XunitException($"Expected type '{fullName}' to be loaded for contract validation.");

    private static void AssertProperty(Type declaringType, string propertyName, Type propertyType)
    {
        var property = declaringType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property!.PropertyType);
        Assert.Null(property.SetMethod);
    }

    private static void AssertProperty(Type declaringType, string propertyName, string propertyTypeFullName)
    {
        var property = declaringType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(propertyTypeFullName, property!.PropertyType.FullName);
        Assert.Null(property.SetMethod);
    }
}
