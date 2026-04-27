using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelSceneImageExportCoordinator
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelSceneImageExportCoordinator(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public bool CanExport
            => _owner.CreateActiveScopeDocumentSnapshot().Nodes.Count > 0;

        public bool TryExport(
            GraphEditorSceneImageExportFormat format,
            string? path,
            GraphEditorSceneImageExportOptions? options)
        {
            if (!CanExport)
            {
                _owner.CurrentStatusMessage = "Add at least one node before exporting a scene image.";
                return false;
            }

            var resolvedPath = string.IsNullOrWhiteSpace(path)
                ? _owner._sceneImageExportService.GetExportPath(format)
                : path.Trim();

            try
            {
                var exportScene = GraphEditorSceneImageExportScopeResolver.Resolve(_owner.GetSceneSnapshot(), options);
                var writtenPath = _owner._sceneImageExportService.Export(exportScene, format, path, options);
                var formatLabel = format switch
                {
                    GraphEditorSceneImageExportFormat.Png => "PNG",
                    GraphEditorSceneImageExportFormat.Jpeg => "JPEG",
                    _ => format.ToString().ToUpperInvariant(),
                };
                _owner.CurrentStatusMessage = $"Exported scene {formatLabel} to {writtenPath}.";
                _owner.DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                    "export.scene-image.succeeded",
                    "export.scene-image",
                    _owner.CurrentStatusMessage,
                    GraphEditorDiagnosticSeverity.Info));
                return true;
            }
            catch (Exception exception)
            {
                _owner.CurrentStatusMessage = $"Failed to export scene image to {resolvedPath}.";
                _owner.RecoverableFailureRaised?.Invoke(
                    _owner,
                    new GraphEditorRecoverableFailureEventArgs(
                        "export.scene-image.failed",
                        "export.scene-image",
                        _owner.CurrentStatusMessage,
                        exception));
                return false;
            }
        }
    }
}
