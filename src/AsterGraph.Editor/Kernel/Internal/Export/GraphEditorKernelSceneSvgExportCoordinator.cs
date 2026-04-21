using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelSceneSvgExportCoordinator
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelSceneSvgExportCoordinator(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public bool CanExport
            => _owner.CreateActiveScopeDocumentSnapshot().Nodes.Count > 0;

        public bool TryExport(string? path)
        {
            if (!CanExport)
            {
                _owner.CurrentStatusMessage = "Add at least one node before exporting an SVG scene.";
                return false;
            }

            var resolvedPath = string.IsNullOrWhiteSpace(path)
                ? _owner._sceneSvgExportService.ExportPath
                : path.Trim();

            try
            {
                var writtenPath = _owner._sceneSvgExportService.Export(_owner.GetSceneSnapshot(), path);
                _owner.CurrentStatusMessage = $"Exported scene SVG to {writtenPath}.";
                _owner.DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                    "export.scene-svg.succeeded",
                    "export.scene-svg",
                    _owner.CurrentStatusMessage,
                    GraphEditorDiagnosticSeverity.Info));
                return true;
            }
            catch (Exception exception)
            {
                _owner.CurrentStatusMessage = $"Failed to export scene SVG to {resolvedPath}.";
                _owner.RecoverableFailureRaised?.Invoke(
                    _owner,
                    new GraphEditorRecoverableFailureEventArgs(
                        "export.scene-svg.failed",
                        "export.scene-svg",
                        _owner.CurrentStatusMessage,
                        exception));
                return false;
            }
        }
    }
}
