using System.Threading;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    internal interface IGraphEditorFragmentCommandHost
    {
        GraphEditorCommandPermissions CommandPermissions { get; }

        GraphEditorBehaviorOptions BehaviorOptions { get; }

        IEnumerable<NodeViewModel> SelectedNodes { get; }

        string? SelectedNodeId { get; }

        string? SelectedNodeTitle { get; }

        IEnumerable<ConnectionViewModel> Connections { get; }

        string? SelectedFragmentTemplatePath { get; }

        string? StatusMessage { get; }

        IGraphTextClipboardBridge? TextClipboardBridge { get; }

        IGraphClipboardPayloadSerializer ClipboardPayloadSerializer { get; }

        IGraphFragmentWorkspaceService FragmentWorkspaceService { get; }

        IGraphFragmentLibraryService FragmentLibraryService { get; }

        void StoreSelectionClipboard(GraphSelectionFragment fragment);

        GraphSelectionFragment? PeekSelectionClipboard();

        GraphPoint GetNextPasteOrigin();

        string CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey);

        string CreateConnectionId();

        void ApplyNodePresentation(NodeViewModel node);

        void AddNode(NodeViewModel node);

        void AddConnection(ConnectionViewModel connection);

        void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode);

        void RefreshFragmentTemplates();

        void RaiseComputedPropertyChanges();

        string StatusText(string key, string fallback, params object?[] arguments);

        void SetStatus(string key, string fallback, params object?[] arguments);

        void MarkDirty(string status);

        void PublishRuntimeDiagnostic(string code, string operation, string message, GraphEditorDiagnosticSeverity severity, Exception? exception = null);

        void RaiseFragmentExported(string path, GraphSelectionFragment fragment);

        void RaiseFragmentImported(string path, GraphSelectionFragment fragment);
    }

    internal sealed class GraphEditorFragmentCommands
    {
        private readonly IGraphEditorFragmentCommandHost _host;

        internal GraphEditorFragmentCommands(IGraphEditorFragmentCommandHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        internal GraphSelectionFragment? CreateSelectionFragment()
        {
            var selectedNodes = _host.SelectedNodes.ToList();
            if (selectedNodes.Count == 0)
            {
                return null;
            }

            // 复制时只保留当前选择诱导出的子图，避免粘贴结果依赖外部未复制节点。
            var selectedIds = selectedNodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
            var copiedNodes = selectedNodes
                .Select(node => node.ToModel())
                .ToList();

            var copiedConnections = _host.Connections
                .Where(connection =>
                    selectedIds.Contains(connection.SourceNodeId)
                    && selectedIds.Contains(connection.TargetNodeId))
                .Select(connection => connection.ToModel())
                .ToList();

            var origin = new GraphPoint(
                copiedNodes.Min(node => node.Position.X),
                copiedNodes.Min(node => node.Position.Y));

            return new GraphSelectionFragment(
                copiedNodes,
                copiedConnections,
                origin,
                _host.SelectedNodeId);
        }

        internal bool PasteFragment(GraphSelectionFragment fragment, string actionPrefix)
        {
            if (!_host.CommandPermissions.Nodes.AllowCreate)
            {
                _host.SetStatus("editor.status.fragment.insert.disabledByPermissions", "Fragment insertion is disabled by host permissions.");
                return false;
            }

            if (fragment.Connections.Count > 0 && !_host.CommandPermissions.Connections.AllowCreate)
            {
                _host.SetStatus("editor.status.fragment.insert.connectionCreateDisabled", "This fragment contains connections, but connection creation is disabled by host permissions.");
                return false;
            }

            _host.StoreSelectionClipboard(fragment);
            var targetOrigin = _host.GetNextPasteOrigin();
            var nodeIdMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var pastedNodes = new List<NodeViewModel>(fragment.Nodes.Count);

            foreach (var copiedNode in fragment.Nodes)
            {
                var newId = _host.CreateNodeId(copiedNode.DefinitionId, copiedNode.Id);
                nodeIdMap[copiedNode.Id] = newId;

                var relativePosition = copiedNode.Position - fragment.Origin;
                var pastedNode = new NodeViewModel(copiedNode with
                {
                    Id = newId,
                    Position = targetOrigin + relativePosition,
                });

                _host.ApplyNodePresentation(pastedNode);
                _host.AddNode(pastedNode);
                pastedNodes.Add(pastedNode);
            }

            foreach (var copiedConnection in fragment.Connections)
            {
                if (!nodeIdMap.TryGetValue(copiedConnection.SourceNodeId, out var sourceNodeId)
                    || !nodeIdMap.TryGetValue(copiedConnection.TargetNodeId, out var targetNodeId))
                {
                    continue;
                }

                _host.AddConnection(new ConnectionViewModel(
                    _host.CreateConnectionId(),
                    sourceNodeId,
                    copiedConnection.SourcePortId,
                    targetNodeId,
                    copiedConnection.TargetPortId,
                    copiedConnection.Label,
                    copiedConnection.AccentHex,
                    copiedConnection.ConversionId));
            }

            if (pastedNodes.Count == 0)
            {
                return false;
            }

            NodeViewModel? primaryNode = null;
            if (!string.IsNullOrWhiteSpace(fragment.PrimaryNodeId)
                && nodeIdMap.TryGetValue(fragment.PrimaryNodeId, out var remappedPrimaryNodeId))
            {
                primaryNode = pastedNodes.FirstOrDefault(node => node.Id == remappedPrimaryNodeId);
            }

            _host.SetSelection(pastedNodes, primaryNode ?? pastedNodes[^1]);
            _host.MarkDirty(pastedNodes.Count == 1
                ? _host.StatusText("editor.status.fragment.action.single", "{0} {1}.", actionPrefix, pastedNodes[0].Title)
                : _host.StatusText("editor.status.fragment.action.multiple", "{0} {1} nodes.", actionPrefix, pastedNodes.Count));
            return true;
        }

        internal async Task<GraphSelectionFragment?> GetBestAvailableClipboardFragmentAsync()
        {
            if (_host.TextClipboardBridge is not null)
            {
                var clipboardText = await _host.TextClipboardBridge.ReadTextAsync(CancellationToken.None);
                // 优先读取系统剪贴板 JSON，但仍保留进程内剪贴板作为可靠回退。
                if (_host.ClipboardPayloadSerializer.TryDeserialize(clipboardText, out var systemFragment))
                {
                    return systemFragment;
                }
            }

            return _host.PeekSelectionClipboard();
        }

        internal async Task CopySelectionAsync()
        {
            if (!_host.CommandPermissions.Clipboard.AllowCopy)
            {
                _host.SetStatus("editor.status.clipboard.copy.disabledByPermissions", "Copy is disabled by host permissions.");
                return;
            }

            var fragment = CreateSelectionFragment();
            if (fragment is null)
            {
                _host.SetStatus("editor.status.clipboard.copy.selectNodeFirst", "Select at least one node before copying.");
                return;
            }

            _host.StoreSelectionClipboard(fragment);
            var clipboardJson = _host.ClipboardPayloadSerializer.Serialize(fragment);
            if (_host.TextClipboardBridge is not null)
            {
                await _host.TextClipboardBridge.WriteTextAsync(clipboardJson, CancellationToken.None);
            }

            _host.RaiseComputedPropertyChanges();
            if (fragment.Nodes.Count == 1)
            {
                _host.SetStatus("editor.status.clipboard.copy.single", "Copied {0}.", fragment.Nodes[0].Title);
            }
            else
            {
                _host.SetStatus("editor.status.clipboard.copy.multiple", "Copied {0} nodes.", fragment.Nodes.Count);
            }
        }

        internal void ExportSelectionFragment()
        {
            if (!_host.CommandPermissions.Fragments.AllowExport)
            {
                _host.SetStatus("editor.status.fragment.export.disabledByPermissions", "Fragment export is disabled by host permissions.");
                return;
            }

            var fragment = CreateSelectionFragment();
            if (fragment is null)
            {
                _host.SetStatus("editor.status.fragment.export.selectNodeFirst", "Select at least one node before exporting a fragment.");
                return;
            }

            _host.FragmentWorkspaceService.Save(fragment);
            _host.RaiseComputedPropertyChanges();
            _host.SetStatus("editor.status.fragment.export.savedToPath", "Exported fragment to {0}.", _host.FragmentWorkspaceService.FragmentPath);
            _host.PublishRuntimeDiagnostic(
                "fragment.export.succeeded",
                "fragment.export",
                _host.StatusMessage ?? _host.FragmentWorkspaceService.FragmentPath,
                GraphEditorDiagnosticSeverity.Info);
            _host.RaiseFragmentExported(_host.FragmentWorkspaceService.FragmentPath, fragment);
        }

        internal void ExportSelectionAsTemplate()
        {
            if (!_host.CommandPermissions.Fragments.AllowTemplateManagement || !_host.CommandPermissions.Fragments.AllowExport)
            {
                _host.SetStatus("editor.status.fragmentTemplate.export.disabledByPermissions", "Template export is disabled by host permissions.");
                return;
            }

            if (!_host.BehaviorOptions.Fragments.EnableFragmentLibrary)
            {
                _host.SetStatus("editor.status.fragmentTemplate.library.disabled", "Fragment template library is disabled.");
                return;
            }

            var fragment = CreateSelectionFragment();
            if (fragment is null)
            {
                _host.SetStatus("editor.status.fragmentTemplate.export.selectNodeFirst", "Select at least one node before exporting a fragment template.");
                return;
            }

            var templateName = _host.SelectedNodeTitle ?? $"selection-{fragment.Nodes.Count}";
            var path = _host.FragmentLibraryService.SaveTemplate(fragment, templateName);
            _host.RefreshFragmentTemplates();
            _host.SetStatus("editor.status.fragmentTemplate.export.savedToPath", "Exported fragment template to {0}.", path);
            _host.RaiseFragmentExported(path, fragment);
        }

        internal bool ExportSelectionFragmentTo(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!_host.CommandPermissions.Fragments.AllowExport)
            {
                _host.SetStatus("editor.status.fragment.export.disabledByPermissions", "Fragment export is disabled by host permissions.");
                return false;
            }

            var fragment = CreateSelectionFragment();
            if (fragment is null)
            {
                _host.SetStatus("editor.status.fragment.export.selectNodeFirst", "Select at least one node before exporting a fragment.");
                return false;
            }

            _host.FragmentWorkspaceService.Save(fragment, path);
            _host.SetStatus("editor.status.fragment.export.savedToPath", "Exported fragment to {0}.", path);
            _host.RaiseFragmentExported(path, fragment);
            return true;
        }

        internal async Task PasteSelectionAsync()
        {
            if (!_host.CommandPermissions.Clipboard.AllowPaste)
            {
                _host.SetStatus("editor.status.clipboard.paste.disabledByPermissions", "Paste is disabled by host permissions.");
                return;
            }

            var fragment = await GetBestAvailableClipboardFragmentAsync();
            if (fragment is null || fragment.Nodes.Count == 0)
            {
                _host.SetStatus("editor.status.clipboard.paste.nothingCopied", "Nothing copied yet.");
                return;
            }

            PasteFragment(fragment, "Pasted");
        }

        internal void ImportFragment()
        {
            if (!_host.CommandPermissions.Fragments.AllowImport)
            {
                _host.SetStatus("editor.status.fragment.import.disabledByPermissions", "Fragment import is disabled by host permissions.");
                return;
            }

            if (!_host.FragmentWorkspaceService.Exists())
            {
                _host.SetStatus("editor.status.fragment.import.noExportedFile", "No exported fragment file is available yet.");
                _host.PublishRuntimeDiagnostic(
                    "fragment.import.missing",
                    "fragment.import",
                    _host.StatusMessage ?? "No exported fragment file is available yet.",
                    GraphEditorDiagnosticSeverity.Warning);
                return;
            }

            var fragment = _host.FragmentWorkspaceService.Load();
            _host.StoreSelectionClipboard(fragment);
            _host.RaiseComputedPropertyChanges();

            if (!PasteFragment(fragment, "Imported"))
            {
                _host.SetStatus("editor.status.fragment.import.noNodesInFile", "Fragment file did not contain any nodes.");
                _host.PublishRuntimeDiagnostic(
                    "fragment.import.empty",
                    "fragment.import",
                    _host.StatusMessage ?? "Fragment file did not contain any nodes.",
                    GraphEditorDiagnosticSeverity.Warning);
            }
            else
            {
                _host.PublishRuntimeDiagnostic(
                    "fragment.import.succeeded",
                    "fragment.import",
                    _host.StatusMessage ?? _host.FragmentWorkspaceService.FragmentPath,
                    GraphEditorDiagnosticSeverity.Info);
                _host.RaiseFragmentImported(_host.FragmentWorkspaceService.FragmentPath, fragment);
            }
        }

        internal void ClearFragment()
        {
            if (!_host.CommandPermissions.Fragments.AllowClearWorkspaceFragment)
            {
                _host.SetStatus("editor.status.fragment.clear.disabledByPermissions", "Fragment clearing is disabled by host permissions.");
                return;
            }

            if (!_host.FragmentWorkspaceService.Exists())
            {
                _host.SetStatus("editor.status.fragment.import.noExportedFile", "No exported fragment file is available yet.");
                return;
            }

            _host.FragmentWorkspaceService.Delete();
            _host.RaiseComputedPropertyChanges();
            _host.SetStatus("editor.status.fragment.clear.cleared", "Cleared the saved fragment file.");
        }

        internal void ImportSelectedTemplate()
        {
            if (!_host.CommandPermissions.Fragments.AllowTemplateManagement || !_host.CommandPermissions.Fragments.AllowImport)
            {
                _host.SetStatus("editor.status.fragmentTemplate.import.disabledByPermissions", "Template import is disabled by host permissions.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_host.SelectedFragmentTemplatePath))
            {
                _host.SetStatus("editor.status.fragmentTemplate.selectTemplateFirst", "Select a fragment template first.");
                return;
            }

            ImportFragmentFrom(_host.SelectedFragmentTemplatePath);
        }

        internal void DeleteSelectedTemplate()
        {
            if (!_host.CommandPermissions.Fragments.AllowTemplateManagement)
            {
                _host.SetStatus("editor.status.fragmentTemplate.delete.disabledByPermissions", "Template deletion is disabled by host permissions.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_host.SelectedFragmentTemplatePath))
            {
                _host.SetStatus("editor.status.fragmentTemplate.selectTemplateFirst", "Select a fragment template first.");
                return;
            }

            var deletedPath = _host.SelectedFragmentTemplatePath;
            _host.FragmentLibraryService.DeleteTemplate(deletedPath);
            _host.RefreshFragmentTemplates();
            _host.SetStatus(
                "editor.status.fragmentTemplate.deleted",
                "Deleted fragment template {0}.",
                Path.GetFileNameWithoutExtension(deletedPath));
        }

        internal bool ImportFragmentFrom(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!_host.CommandPermissions.Fragments.AllowImport)
            {
                _host.SetStatus("editor.status.fragment.import.disabledByPermissions", "Fragment import is disabled by host permissions.");
                return false;
            }

            if (!_host.FragmentWorkspaceService.Exists(path))
            {
                _host.SetStatus("editor.status.fragment.import.fileNotFound", "Fragment file '{0}' was not found.", path);
                _host.PublishRuntimeDiagnostic(
                    "fragment.import.fileMissing",
                    "fragment.import",
                    _host.StatusMessage ?? path,
                    GraphEditorDiagnosticSeverity.Warning);
                return false;
            }

            var fragment = _host.FragmentWorkspaceService.Load(path);
            _host.StoreSelectionClipboard(fragment);
            _host.RaiseComputedPropertyChanges();
            var imported = PasteFragment(fragment, "Imported");
            if (imported)
            {
                _host.PublishRuntimeDiagnostic(
                    "fragment.import.succeeded",
                    "fragment.import",
                    _host.StatusMessage ?? path,
                    GraphEditorDiagnosticSeverity.Info);
                _host.RaiseFragmentImported(path, fragment);
            }

            return imported;
        }
    }
}
