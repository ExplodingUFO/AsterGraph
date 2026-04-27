using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorSessionDescriptorSupportBuilder
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorSessionDescriptorSupportBuilder(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorSessionDescriptorSupport Build()
            => new(
                _owner._nodeCatalog,
                _owner._compatibilityService,
                _owner._presentationLocalizationCoordinator.LocalizeText,
                _owner,
                hasFragmentWorkspaceService: _owner._fragmentWorkspaceService is not null,
                hasFragmentLibraryService: _owner._fragmentLibraryService is not null,
                hasSceneSvgExportService: _owner._sceneSvgExportService is not null,
                hasSceneImageExportService: _owner._sceneImageExportService is not null,
                hasClipboardPayloadSerializer: _owner._clipboardPayloadSerializer is not null,
                hasPluginLoader: true,
                hasContextMenuAugmentor: _owner._contextMenuAugmentor is not null,
                canEditNodeParameters: () => _owner.CanEditNodeParameters,
                hasNodePresentationProvider: () => _owner._presentationLocalizationCoordinator.HasNodePresentationProvider,
                hasLocalizationProvider: () => _owner._presentationLocalizationCoordinator.HasLocalizationProvider);
    }
}
