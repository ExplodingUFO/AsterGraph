using System.Collections.ObjectModel;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelStorageProjectionHost : IGraphEditorStorageProjectionHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelStorageProjectionHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        bool IGraphEditorStorageProjectionHost.IsDirty => _owner.IsDirty;

        string IGraphEditorStorageProjectionHost.WorkspacePath => _owner.WorkspacePath;

        ObservableCollection<FragmentTemplateViewModel> IGraphEditorStorageProjectionHost.FragmentTemplates => _owner.FragmentTemplates;

        IGraphFragmentWorkspaceService IGraphEditorStorageProjectionHost.FragmentWorkspaceService => _owner._fragmentWorkspaceService;

        IGraphFragmentLibraryService IGraphEditorStorageProjectionHost.FragmentLibraryService => _owner._fragmentLibraryService;

        void IGraphEditorStorageProjectionHost.SetSelectedFragmentTemplate(FragmentTemplateViewModel? value)
            => _owner.SelectedFragmentTemplate = value;

        string IGraphEditorStorageProjectionHost.LocalizeText(string key, string fallback)
            => _owner.LocalizeText(key, fallback);

        string IGraphEditorStorageProjectionHost.LocalizeFormat(string key, string fallback, params object?[] arguments)
            => _owner.LocalizeFormat(key, fallback, arguments);

        void IGraphEditorStorageProjectionHost.RaiseComputedPropertyChanges()
            => _owner.RaiseComputedPropertyChanges();
    }
}
