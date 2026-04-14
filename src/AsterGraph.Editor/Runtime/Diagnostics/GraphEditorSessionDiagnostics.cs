using System.Diagnostics;
using AsterGraph.Editor;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    private const int RecentDiagnosticsCapacity = 32;
    private readonly List<GraphEditorDiagnostic> _recentDiagnostics = [];
    private ILogger? _logger;
    private ActivitySource? _activitySource;

    public IReadOnlyList<GraphEditorDiagnostic> GetRecentDiagnostics(int maxCount = 20)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxCount);
        if (maxCount == 0 || _recentDiagnostics.Count == 0)
        {
            return [];
        }

        var skip = Math.Max(0, _recentDiagnostics.Count - maxCount);
        return _recentDiagnostics.Skip(skip).ToList();
    }

    internal void PublishRecoverableFailure(GraphEditorRecoverableFailureEventArgs failure)
    {
        var diagnostic = new GraphEditorDiagnostic(
            failure.Code,
            failure.Operation,
            failure.Message,
            GraphEditorDiagnosticSeverity.Error,
            failure.Exception);

        if (IsBatching)
        {
            _pendingRecoverableFailures.Add(failure);
            PublishDiagnostic(diagnostic);
            return;
        }

        PublishDiagnostic(diagnostic);
        RecoverableFailure?.Invoke(this, failure);
    }

    internal void ConfigureInstrumentation(GraphEditorInstrumentationOptions? instrumentation)
    {
        _logger = instrumentation?.LoggerFactory?.CreateLogger(typeof(GraphEditorSession).FullName ?? nameof(GraphEditorSession));
        _activitySource = instrumentation?.ActivitySource;
    }

    internal void PublishDiagnostic(GraphEditorDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        if (_recentDiagnostics.Count == RecentDiagnosticsCapacity)
        {
            _recentDiagnostics.RemoveAt(0);
        }

        _recentDiagnostics.Add(diagnostic);
        _diagnosticsSink?.Publish(diagnostic);
        EmitInstrumentation(diagnostic);
    }

    private void EmitInstrumentation(GraphEditorDiagnostic diagnostic)
    {
        using var activity = _activitySource?.StartActivity(diagnostic.Operation, ActivityKind.Internal);
        if (activity is not null)
        {
            activity.SetTag("astergraph.diagnostic.code", diagnostic.Code);
            activity.SetTag("astergraph.diagnostic.operation", diagnostic.Operation);
            activity.SetTag("astergraph.diagnostic.severity", diagnostic.Severity.ToString());
            activity.SetTag("astergraph.diagnostic.message", diagnostic.Message);

            if (diagnostic.Exception is not null)
            {
                activity.SetTag("exception.type", diagnostic.Exception.GetType().FullName);
                activity.SetTag("exception.message", diagnostic.Exception.Message);
            }

            activity.SetStatus(
                diagnostic.Severity == GraphEditorDiagnosticSeverity.Error
                    ? ActivityStatusCode.Error
                    : ActivityStatusCode.Ok,
                diagnostic.Exception?.Message ?? diagnostic.Message);
        }

        _logger?.Log(
            ToLogLevel(diagnostic.Severity),
            diagnostic.Exception,
            "AsterGraph diagnostic {Code} ({Operation}): {Message}",
            diagnostic.Code,
            diagnostic.Operation,
            diagnostic.Message);
    }

    private static LogLevel ToLogLevel(GraphEditorDiagnosticSeverity severity)
        => severity switch
        {
            GraphEditorDiagnosticSeverity.Info => LogLevel.Information,
            GraphEditorDiagnosticSeverity.Warning => LogLevel.Warning,
            GraphEditorDiagnosticSeverity.Error => LogLevel.Error,
            _ => LogLevel.None,
        };

    private void HandleRecoverableFailureRaised(object? sender, GraphEditorRecoverableFailureEventArgs args)
        => PublishRecoverableFailure(args);

    private void HandleDiagnosticPublished(GraphEditorDiagnostic diagnostic)
        => PublishDiagnostic(diagnostic);
}
