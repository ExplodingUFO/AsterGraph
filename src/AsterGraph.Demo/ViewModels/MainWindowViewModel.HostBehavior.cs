using CommunityToolkit.Mvvm.Input;
using AsterGraph.Editor.Configuration;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    partial void OnIsGridSnappingEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnIsAlignmentGuidesEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnIsReadOnlyEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnAreWorkspaceCommandsEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnAreFragmentCommandsEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnAreHostMenuExtensionsEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnIsHeaderChromeVisibleChanged(bool value)
        => RefreshRuntimeProjection();

    partial void OnIsLibraryChromeVisibleChanged(bool value)
        => RefreshRuntimeProjection();

    partial void OnIsInspectorChromeVisibleChanged(bool value)
        => RefreshRuntimeProjection();

    partial void OnIsStatusChromeVisibleChanged(bool value)
        => RefreshRuntimeProjection();

    partial void OnIsHostPaneOpenChanged(bool value)
        => RefreshRuntimeProjection();

    [RelayCommand]
    public void SelectCapability(CapabilityShowcaseItem capability)
    {
        ArgumentNullException.ThrowIfNull(capability);
        SelectedCapability = capability;
    }

    [RelayCommand]
    public void OpenHostMenuGroup(string groupTitle)
    {
        if (string.IsNullOrWhiteSpace(groupTitle))
        {
            return;
        }

        SelectedHostMenuGroup = NormalizeHostMenuGroup(groupTitle);
        IsHostPaneOpen = true;
    }

    [RelayCommand]
    public void CloseHostPane()
        => IsHostPaneOpen = false;

    private void ApplyHostOptions(string? status = "Host behavior updated.")
    {
        Editor.UpdateBehaviorOptions(
            Editor.BehaviorOptions with
            {
                DragAssist = Editor.BehaviorOptions.DragAssist with
                {
                    EnableGridSnapping = IsGridSnappingEnabled,
                    EnableAlignmentGuides = IsAlignmentGuidesEnabled,
                    SnapTolerance = DemoSnapTolerance,
                },
                Commands = BuildCommandPermissions(),
            },
            status);

        RefreshRuntimeProjection();
    }

    private GraphEditorCommandPermissions BuildCommandPermissions()
    {
        return GraphEditorCommandPermissions.Default with
        {
            Workspace = new WorkspaceCommandPermissions
            {
                AllowSave = AreWorkspaceCommandsEnabled && !IsReadOnlyEnabled,
                AllowLoad = AreWorkspaceCommandsEnabled && !IsReadOnlyEnabled,
            },
            History = new HistoryCommandPermissions
            {
                AllowUndo = !IsReadOnlyEnabled,
                AllowRedo = !IsReadOnlyEnabled,
            },
            Nodes = new NodeCommandPermissions
            {
                AllowCreate = !IsReadOnlyEnabled,
                AllowDelete = !IsReadOnlyEnabled,
                AllowMove = !IsReadOnlyEnabled,
                AllowDuplicate = !IsReadOnlyEnabled,
                AllowEditParameters = !IsReadOnlyEnabled,
            },
            Connections = new ConnectionCommandPermissions
            {
                AllowCreate = !IsReadOnlyEnabled,
                AllowDelete = !IsReadOnlyEnabled,
                AllowDisconnect = !IsReadOnlyEnabled,
            },
            Clipboard = new ClipboardCommandPermissions
            {
                AllowCopy = !IsReadOnlyEnabled,
                AllowPaste = !IsReadOnlyEnabled,
            },
            Layout = new LayoutCommandPermissions
            {
                AllowAlign = !IsReadOnlyEnabled,
                AllowDistribute = !IsReadOnlyEnabled,
            },
            Fragments = new FragmentCommandPermissions
            {
                AllowImport = AreFragmentCommandsEnabled && !IsReadOnlyEnabled,
                AllowExport = AreFragmentCommandsEnabled,
                AllowClearWorkspaceFragment = AreFragmentCommandsEnabled && !IsReadOnlyEnabled,
                AllowTemplateManagement = AreFragmentCommandsEnabled && !IsReadOnlyEnabled,
            },
            Host = new HostCommandPermissions
            {
                AllowContextMenuExtensions = AreHostMenuExtensionsEnabled,
            },
        };
    }

    private string BoolText(bool value)
        => value ? T("是", "Yes") : T("否", "No");
}
