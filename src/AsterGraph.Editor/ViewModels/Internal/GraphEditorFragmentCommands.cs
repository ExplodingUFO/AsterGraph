using System.Threading;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    internal sealed class GraphEditorFragmentCommands
    {
        private readonly GraphEditorViewModel _owner;

        internal GraphEditorFragmentCommands(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        internal GraphSelectionFragment? CreateSelectionFragment()
        {
            if (_owner.SelectedNodes.Count == 0)
            {
                return null;
            }

            // 复制时只保留当前选择诱导出的子图，避免粘贴结果依赖外部未复制节点。
            var selectedIds = _owner.SelectedNodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
            var copiedNodes = _owner.SelectedNodes
                .Select(node => node.ToModel())
                .ToList();

            var copiedConnections = _owner.Connections
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
                _owner.SelectedNode?.Id);
        }

        internal bool PasteFragment(GraphSelectionFragment fragment, string actionPrefix)
        {
            if (!_owner.CommandPermissions.Nodes.AllowCreate)
            {
                _owner.SetStatus("editor.status.fragment.insert.disabledByPermissions", "Fragment insertion is disabled by host permissions.");
                return false;
            }

            if (fragment.Connections.Count > 0 && !_owner.CommandPermissions.Connections.AllowCreate)
            {
                _owner.SetStatus("editor.status.fragment.insert.connectionCreateDisabled", "This fragment contains connections, but connection creation is disabled by host permissions.");
                return false;
            }

            _owner._selectionClipboard.Store(fragment);
            var targetOrigin = _owner._selectionClipboard.GetNextPasteOrigin(_owner.GetViewportCenter());
            var nodeIdMap = new Dictionary<string, string>(StringComparer.Ordinal);
            var pastedNodes = new List<NodeViewModel>(fragment.Nodes.Count);

            foreach (var copiedNode in fragment.Nodes)
            {
                var newId = _owner.CreateNodeId(copiedNode.DefinitionId, copiedNode.Id);
                nodeIdMap[copiedNode.Id] = newId;

                var relativePosition = copiedNode.Position - fragment.Origin;
                var pastedNode = new NodeViewModel(copiedNode with
                {
                    Id = newId,
                    Position = targetOrigin + relativePosition,
                });

                _owner.ApplyNodePresentation(pastedNode);
                _owner.Nodes.Add(pastedNode);
                pastedNodes.Add(pastedNode);
            }

            foreach (var copiedConnection in fragment.Connections)
            {
                if (!nodeIdMap.TryGetValue(copiedConnection.SourceNodeId, out var sourceNodeId)
                    || !nodeIdMap.TryGetValue(copiedConnection.TargetNodeId, out var targetNodeId))
                {
                    continue;
                }

                _owner.Connections.Add(new ConnectionViewModel(
                    _owner.CreateConnectionId(),
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

            _owner.SetSelection(pastedNodes, primaryNode ?? pastedNodes[^1]);
            _owner.MarkDirty(pastedNodes.Count == 1
                ? _owner.StatusText("editor.status.fragment.action.single", "{0} {1}.", actionPrefix, pastedNodes[0].Title)
                : _owner.StatusText("editor.status.fragment.action.multiple", "{0} {1} nodes.", actionPrefix, pastedNodes.Count));
            return true;
        }

        internal async Task<GraphSelectionFragment?> GetBestAvailableClipboardFragmentAsync()
        {
            if (_owner._textClipboardBridge is not null)
            {
                var clipboardText = await _owner._textClipboardBridge.ReadTextAsync(CancellationToken.None);
                // 优先读取系统剪贴板 JSON，但仍保留进程内剪贴板作为可靠回退。
                if (_owner._clipboardPayloadSerializer.TryDeserialize(clipboardText, out var systemFragment))
                {
                    return systemFragment;
                }
            }

            return _owner._selectionClipboard.Peek();
        }

        internal async Task CopySelectionAsync()
        {
            if (!_owner.CommandPermissions.Clipboard.AllowCopy)
            {
                _owner.SetStatus("editor.status.clipboard.copy.disabledByPermissions", "Copy is disabled by host permissions.");
                return;
            }

            var fragment = CreateSelectionFragment();
            if (fragment is null)
            {
                _owner.SetStatus("editor.status.clipboard.copy.selectNodeFirst", "Select at least one node before copying.");
                return;
            }

            _owner._selectionClipboard.Store(fragment);
            var clipboardJson = _owner._clipboardPayloadSerializer.Serialize(fragment);
            if (_owner._textClipboardBridge is not null)
            {
                await _owner._textClipboardBridge.WriteTextAsync(clipboardJson, CancellationToken.None);
            }

            _owner.RaiseComputedPropertyChanges();
            _owner.SetStatus(
                fragment.Nodes.Count == 1
                    ? ("editor.status.clipboard.copy.single", "Copied {0}.", new object?[] { fragment.Nodes[0].Title })
                    : ("editor.status.clipboard.copy.multiple", "Copied {0} nodes.", new object?[] { fragment.Nodes.Count }));
        }

        internal void ExportSelectionFragment()
        {
            if (!_owner.CommandPermissions.Fragments.AllowExport)
            {
                _owner.SetStatus("editor.status.fragment.export.disabledByPermissions", "Fragment export is disabled by host permissions.");
                return;
            }

            var fragment = CreateSelectionFragment();
            if (fragment is null)
            {
                _owner.SetStatus("editor.status.fragment.export.selectNodeFirst", "Select at least one node before exporting a fragment.");
                return;
            }

            _owner._fragmentWorkspaceService.Save(fragment);
            _owner.RaiseComputedPropertyChanges();
            _owner.SetStatus("editor.status.fragment.export.savedToPath", "Exported fragment to {0}.", _owner._fragmentWorkspaceService.FragmentPath);
            _owner.PublishRuntimeDiagnostic(
                "fragment.export.succeeded",
                "fragment.export",
                _owner.StatusMessage ?? _owner._fragmentWorkspaceService.FragmentPath,
                GraphEditorDiagnosticSeverity.Info);
            _owner.FragmentExported?.Invoke(
                _owner,
                new GraphEditorFragmentEventArgs(
                    _owner._fragmentWorkspaceService.FragmentPath,
                    fragment.Nodes.Count,
                    fragment.Connections.Count));
        }

        internal void ExportSelectionAsTemplate()
        {
            if (!_owner.CommandPermissions.Fragments.AllowTemplateManagement || !_owner.CommandPermissions.Fragments.AllowExport)
            {
                _owner.SetStatus("editor.status.fragmentTemplate.export.disabledByPermissions", "Template export is disabled by host permissions.");
                return;
            }

            if (!_owner.BehaviorOptions.Fragments.EnableFragmentLibrary)
            {
                _owner.SetStatus("editor.status.fragmentTemplate.library.disabled", "Fragment template library is disabled.");
                return;
            }

            var fragment = CreateSelectionFragment();
            if (fragment is null)
            {
                _owner.SetStatus("editor.status.fragmentTemplate.export.selectNodeFirst", "Select at least one node before exporting a fragment template.");
                return;
            }

            var templateName = _owner.SelectedNode?.Title ?? $"selection-{fragment.Nodes.Count}";
            var path = _owner._fragmentLibraryService.SaveTemplate(fragment, templateName);
            _owner.RefreshFragmentTemplates();
            _owner.SetStatus("editor.status.fragmentTemplate.export.savedToPath", "Exported fragment template to {0}.", path);
            _owner.FragmentExported?.Invoke(
                _owner,
                new GraphEditorFragmentEventArgs(
                    path,
                    fragment.Nodes.Count,
                    fragment.Connections.Count));
        }

        internal bool ExportSelectionFragmentTo(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!_owner.CommandPermissions.Fragments.AllowExport)
            {
                _owner.SetStatus("editor.status.fragment.export.disabledByPermissions", "Fragment export is disabled by host permissions.");
                return false;
            }

            var fragment = CreateSelectionFragment();
            if (fragment is null)
            {
                _owner.SetStatus("editor.status.fragment.export.selectNodeFirst", "Select at least one node before exporting a fragment.");
                return false;
            }

            _owner._fragmentWorkspaceService.Save(fragment, path);
            _owner.SetStatus("editor.status.fragment.export.savedToPath", "Exported fragment to {0}.", path);
            _owner.FragmentExported?.Invoke(
                _owner,
                new GraphEditorFragmentEventArgs(
                    path,
                    fragment.Nodes.Count,
                    fragment.Connections.Count));
            return true;
        }

        internal async Task PasteSelectionAsync()
        {
            if (!_owner.CommandPermissions.Clipboard.AllowPaste)
            {
                _owner.SetStatus("editor.status.clipboard.paste.disabledByPermissions", "Paste is disabled by host permissions.");
                return;
            }

            var fragment = await GetBestAvailableClipboardFragmentAsync();
            if (fragment is null || fragment.Nodes.Count == 0)
            {
                _owner.SetStatus("editor.status.clipboard.paste.nothingCopied", "Nothing copied yet.");
                return;
            }

            PasteFragment(fragment, "Pasted");
        }

        internal void ImportFragment()
        {
            if (!_owner.CommandPermissions.Fragments.AllowImport)
            {
                _owner.SetStatus("editor.status.fragment.import.disabledByPermissions", "Fragment import is disabled by host permissions.");
                return;
            }

            if (!_owner._fragmentWorkspaceService.Exists())
            {
                _owner.SetStatus("editor.status.fragment.import.noExportedFile", "No exported fragment file is available yet.");
                _owner.PublishRuntimeDiagnostic(
                    "fragment.import.missing",
                    "fragment.import",
                    _owner.StatusMessage ?? "No exported fragment file is available yet.",
                    GraphEditorDiagnosticSeverity.Warning);
                return;
            }

            var fragment = _owner._fragmentWorkspaceService.Load();
            _owner._selectionClipboard.Store(fragment);
            _owner.RaiseComputedPropertyChanges();

            if (!PasteFragment(fragment, "Imported"))
            {
                _owner.SetStatus("editor.status.fragment.import.noNodesInFile", "Fragment file did not contain any nodes.");
                _owner.PublishRuntimeDiagnostic(
                    "fragment.import.empty",
                    "fragment.import",
                    _owner.StatusMessage ?? "Fragment file did not contain any nodes.",
                    GraphEditorDiagnosticSeverity.Warning);
            }
            else
            {
                _owner.PublishRuntimeDiagnostic(
                    "fragment.import.succeeded",
                    "fragment.import",
                    _owner.StatusMessage ?? _owner._fragmentWorkspaceService.FragmentPath,
                    GraphEditorDiagnosticSeverity.Info);
                _owner.FragmentImported?.Invoke(
                    _owner,
                    new GraphEditorFragmentEventArgs(
                        _owner._fragmentWorkspaceService.FragmentPath,
                        fragment.Nodes.Count,
                        fragment.Connections.Count));
            }
        }

        internal void ClearFragment()
        {
            if (!_owner.CommandPermissions.Fragments.AllowClearWorkspaceFragment)
            {
                _owner.SetStatus("editor.status.fragment.clear.disabledByPermissions", "Fragment clearing is disabled by host permissions.");
                return;
            }

            if (!_owner._fragmentWorkspaceService.Exists())
            {
                _owner.SetStatus("editor.status.fragment.import.noExportedFile", "No exported fragment file is available yet.");
                return;
            }

            _owner._fragmentWorkspaceService.Delete();
            _owner.RaiseComputedPropertyChanges();
            _owner.SetStatus("editor.status.fragment.clear.cleared", "Cleared the saved fragment file.");
        }

        internal void ImportSelectedTemplate()
        {
            if (!_owner.CommandPermissions.Fragments.AllowTemplateManagement || !_owner.CommandPermissions.Fragments.AllowImport)
            {
                _owner.SetStatus("editor.status.fragmentTemplate.import.disabledByPermissions", "Template import is disabled by host permissions.");
                return;
            }

            if (_owner.SelectedFragmentTemplate is null)
            {
                _owner.SetStatus("editor.status.fragmentTemplate.selectTemplateFirst", "Select a fragment template first.");
                return;
            }

            ImportFragmentFrom(_owner.SelectedFragmentTemplate.Path);
        }

        internal void DeleteSelectedTemplate()
        {
            if (!_owner.CommandPermissions.Fragments.AllowTemplateManagement)
            {
                _owner.SetStatus("editor.status.fragmentTemplate.delete.disabledByPermissions", "Template deletion is disabled by host permissions.");
                return;
            }

            if (_owner.SelectedFragmentTemplate is null)
            {
                _owner.SetStatus("editor.status.fragmentTemplate.selectTemplateFirst", "Select a fragment template first.");
                return;
            }

            var deletedPath = _owner.SelectedFragmentTemplate.Path;
            _owner._fragmentLibraryService.DeleteTemplate(deletedPath);
            _owner.RefreshFragmentTemplates();
            _owner.SetStatus(
                "editor.status.fragmentTemplate.deleted",
                "Deleted fragment template {0}.",
                Path.GetFileNameWithoutExtension(deletedPath));
        }

        internal bool ImportFragmentFrom(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!_owner.CommandPermissions.Fragments.AllowImport)
            {
                _owner.SetStatus("editor.status.fragment.import.disabledByPermissions", "Fragment import is disabled by host permissions.");
                return false;
            }

            if (!_owner._fragmentWorkspaceService.Exists(path))
            {
                _owner.SetStatus("editor.status.fragment.import.fileNotFound", "Fragment file '{0}' was not found.", path);
                _owner.PublishRuntimeDiagnostic(
                    "fragment.import.fileMissing",
                    "fragment.import",
                    _owner.StatusMessage ?? path,
                    GraphEditorDiagnosticSeverity.Warning);
                return false;
            }

            var fragment = _owner._fragmentWorkspaceService.Load(path);
            _owner._selectionClipboard.Store(fragment);
            _owner.RaiseComputedPropertyChanges();
            var imported = PasteFragment(fragment, "Imported");
            if (imported)
            {
                _owner.PublishRuntimeDiagnostic(
                    "fragment.import.succeeded",
                    "fragment.import",
                    _owner.StatusMessage ?? path,
                    GraphEditorDiagnosticSeverity.Info);
                _owner.FragmentImported?.Invoke(
                    _owner,
                    new GraphEditorFragmentEventArgs(
                        path,
                        fragment.Nodes.Count,
                        fragment.Connections.Count));
            }

            return imported;
        }
    }
}
