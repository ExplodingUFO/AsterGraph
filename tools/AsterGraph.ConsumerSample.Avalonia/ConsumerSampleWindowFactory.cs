using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.ConsumerSample;

public static class ConsumerSampleWindowFactory
{
    public static Window Create()
        => Create(ConsumerSampleHost.Create(), ownsHost: true);

    public static Window Create(ConsumerSampleHost host)
        => Create(host, ownsHost: false);

    private static Window Create(ConsumerSampleHost host, bool ownsHost)
    {
        ArgumentNullException.ThrowIfNull(host);
        return new ConsumerSampleWindow(host, ownsHost);
    }

    private sealed class ConsumerSampleWindow : Window
    {
        private readonly ConsumerSampleHost _host;
        private readonly bool _ownsHost;
        private readonly MenuItem _actionsMenu;
        private readonly StackPanel _actionRailItems;
        private readonly ItemsControl _parameterItems;
        private readonly ItemsControl _pluginCandidateItems;
        private readonly ItemsControl _pluginGalleryItems;
        private readonly ItemsControl _pluginSnapshotItems;
        private readonly ItemsControl _allowlistItems;
        private readonly TextBlock _runtimeSummaryText;
        private readonly ComboBox _runtimeLogFilter;
        private readonly ItemsControl _runtimeLogItems;
        private readonly TextBox _graphSearchBox;
        private readonly ComboBox _graphSearchScope;
        private readonly ItemsControl _graphSearchItems;
        private readonly TextBlock _navigationHistorySummaryText;
        private readonly ItemsControl _scopeBreadcrumbItems;
        private Button? _navigationBackButton;
        private Button? _navigationForwardButton;
        private Button? _restoreViewportButton;
        private readonly TextBlock _layoutPreviewSummaryText;
        private readonly TextBlock _selectionSummaryText;
        private readonly TextBlock _trustBoundaryText;
        private string _selectedRuntimeLogFilter = "All";
        private string _graphSearchQuery = "review";
        private ConsumerSampleGraphSearchScope _selectedGraphSearchScope = ConsumerSampleGraphSearchScope.All;

        public ConsumerSampleWindow(ConsumerSampleHost host, bool ownsHost)
        {
            _host = host;
            _ownsHost = ownsHost;
            Title = "AsterGraph Consumer Sample";
            Width = 1560;
            Height = 980;
            var presentation = ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions();

            var editorView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
            {
                Editor = _host.Editor,
                ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
                EnableDefaultContextMenu = true,
                CommandShortcutPolicy = AsterGraphCommandShortcutPolicy.Default,
                Presentation = presentation,
            });
            editorView.Name = "PART_EditorView";
            var edgeOverlay = ConsumerSampleAuthoringSurfaceRecipe.CreateEdgeOverlay(_host.Session);

            _actionsMenu = new MenuItem
            {
                Name = "PART_ActionsMenu",
                Header = "Actions",
            };

            var menu = new Menu
            {
                Name = "PART_MainMenu",
                ItemsSource = new[] { _actionsMenu },
            };

            _actionRailItems = new StackPanel
            {
                Spacing = 10,
                Name = "PART_ActionRail",
            };

            var leftRail = new StackPanel
            {
                Spacing = 10,
                Width = 250,
                Children =
                {
                    _actionRailItems,
                    new Border
                    {
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(12),
                        Background = Brush.Parse("#111827"),
                        Child = new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            Text = "This sample sits between HelloWorld and Demo. It keeps one realistic host window, one host-owned action rail, one custom parameter panel, and one trusted plugin contribution path.",
                        },
                    },
                },
            };

            _selectionSummaryText = new TextBlock
            {
                Name = "PART_SelectionSummaryText",
                TextWrapping = TextWrapping.Wrap,
            };

            _parameterItems = new ItemsControl
            {
                Name = "PART_ParameterItems",
            };

            _pluginCandidateItems = new ItemsControl
            {
                Name = "PART_PluginCandidateItems",
            };

            _pluginGalleryItems = new ItemsControl
            {
                Name = "PART_LocalPluginGalleryItems",
            };

            _pluginSnapshotItems = new ItemsControl
            {
                Name = "PART_PluginSnapshotItems",
            };

            _allowlistItems = new ItemsControl
            {
                Name = "PART_AllowlistItems",
            };

            _runtimeSummaryText = new TextBlock
            {
                Name = "PART_RuntimeSummaryText",
                TextWrapping = TextWrapping.Wrap,
            };

            _runtimeLogItems = new ItemsControl
            {
                Name = "PART_RuntimeLogItems",
            };
            _graphSearchBox = new TextBox
            {
                Name = "PART_GraphSearchBox",
                Text = _graphSearchQuery,
                Watermark = "Search graph",
            };
            _graphSearchBox.TextChanged += (_, _) =>
            {
                _graphSearchQuery = _graphSearchBox.Text ?? string.Empty;
                RebuildGraphSearchPanel();
            };

            _graphSearchScope = new ComboBox
            {
                Name = "PART_GraphSearchScope",
                ItemsSource = Enum.GetValues<ConsumerSampleGraphSearchScope>(),
                SelectedItem = _selectedGraphSearchScope,
            };
            _graphSearchScope.SelectionChanged += (_, _) =>
            {
                if (_graphSearchScope.SelectedItem is ConsumerSampleGraphSearchScope scope)
                {
                    _selectedGraphSearchScope = scope;
                }

                RebuildGraphSearchPanel();
            };

            _graphSearchItems = new ItemsControl
            {
                Name = "PART_GraphSearchItems",
            };
            _navigationHistorySummaryText = new TextBlock
            {
                Name = "PART_NavigationHistorySummaryText",
                TextWrapping = TextWrapping.Wrap,
            };
            _scopeBreadcrumbItems = new ItemsControl
            {
                Name = "PART_ScopeBreadcrumbItems",
            };
            _layoutPreviewSummaryText = new TextBlock
            {
                Name = "PART_LayoutPreviewSummaryText",
                TextWrapping = TextWrapping.Wrap,
            };

            _runtimeLogFilter = new ComboBox
            {
                Name = "PART_RuntimeLogFilter",
                ItemsSource = new[] { "All", "Selected Node", "Current Scope" },
                SelectedItem = _selectedRuntimeLogFilter,
            };
            _runtimeLogFilter.SelectionChanged += (_, _) =>
            {
                _selectedRuntimeLogFilter = _runtimeLogFilter.SelectedItem?.ToString() ?? "All";
                RebuildRuntimePanel();
            };

            _trustBoundaryText = new TextBlock
            {
                Name = "PART_TrustBoundaryText",
                TextWrapping = TextWrapping.Wrap,
                Text = _host.TrustBoundaryText,
            };

            var rightPanel = new StackPanel
            {
                Width = 320,
                Spacing = 14,
                Children =
                {
                    CreateSection("Selection", _selectionSummaryText),
                    CreateSection("Graph Search", CreateGraphSearchPanel()),
                    CreateSection("Navigation", CreateNavigationPanel()),
                    CreateSection("Selected Parameters", _parameterItems),
                    CreateSection("Plugin Candidates", _pluginCandidateItems),
                    CreateSection("Local Plugin Gallery", _pluginGalleryItems),
                    CreateSection("Plugin Load Snapshots", _pluginSnapshotItems),
                    CreateSection("Allowlist Decisions", CreateAllowlistPanel()),
                    CreateSection("Runtime", CreateRuntimePanel()),
                    CreateSection("Layout", CreateLayoutPanel()),
                    CreateSection("Trust Boundary", _trustBoundaryText),
                },
            };

            var editorSurface = new Grid
            {
                Children =
                {
                    editorView,
                    edgeOverlay,
                },
            };

            var body = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("260,*,340"),
                RowDefinitions = new RowDefinitions("*"),
                Margin = new Thickness(16),
                ColumnSpacing = 16,
                Children =
                {
                    leftRail,
                    editorSurface,
                    rightPanel,
                },
            };

            Grid.SetColumn(leftRail, 0);
            Grid.SetColumn(editorSurface, 1);
            Grid.SetColumn(rightPanel, 2);

            Content = new DockPanel
            {
                LastChildFill = true,
                Children =
                {
                    menu,
                    body,
                },
            };
            DockPanel.SetDock(menu, Dock.Top);

            _host.StateChanged += HandleHostStateChanged;
            RefreshPanels();
        }

        protected override void OnClosed(EventArgs e)
        {
            _host.StateChanged -= HandleHostStateChanged;
            if (_ownsHost)
            {
                _host.Dispose();
            }

            base.OnClosed(e);
        }

        private void HandleHostStateChanged(object? sender, EventArgs e)
            => RefreshPanels();

        private void RefreshPanels()
        {
            RebuildActionSurfaces();

            var selection = _host.Session.Queries.GetSelectionSnapshot();
            _selectionSummaryText.Text = selection.PrimarySelectedNodeId is null
                ? "No node is selected."
                : $"Primary node: {selection.PrimarySelectedNodeId}\nSelected nodes: {selection.SelectedNodeIds.Count}";

            RebuildParameterItems();
            RebuildPluginCandidateItems();
            RebuildPluginGalleryItems();
            RebuildPluginSnapshotItems();
            RebuildGraphSearchPanel();
            RebuildNavigationPanel();
            RebuildRuntimePanel();
            RebuildLayoutPanel();
            _trustBoundaryText.Text = _host.TrustBoundaryText;
            _allowlistItems.ItemsSource = _host.PluginAllowlistLines
                .Select(line => new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = line,
                })
                .ToList();
        }

        private void RebuildActionSurfaces()
        {
            var projection = BuildHostedActionProjection();
            _actionRailItems.Children.Clear();
            foreach (var action in projection.Actions)
            {
                _actionRailItems.Children.Add(CreateActionButton(action));
            }

            _actionsMenu.ItemsSource = projection.Actions.Select(CreateMenuItem).ToList();
        }

        private void RebuildParameterItems()
        {
            var items = new List<Control>();

            foreach (var snapshot in _host.GetSelectedParameterSnapshots())
            {
                items.Add(CreateParameterEditor(snapshot));
            }

            if (items.Count == 0)
            {
                items.Add(new TextBlock { Text = "Select one review node to edit its shared parameters." });
            }

            _parameterItems.ItemsSource = items;
        }

        private void RebuildPluginSnapshotItems()
        {
            var items = new List<Control>();

            foreach (var snapshot in _host.PluginLoadSnapshots)
            {
                items.Add(new Border
                {
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Background = Brush.Parse("#111827"),
                    Child = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text =
                            $"{snapshot.Manifest.DisplayName}\n" +
                            $"Status: {snapshot.Status}\n" +
                            $"Trust: {snapshot.TrustEvaluation.Decision} ({snapshot.TrustEvaluation.ReasonCode})\n" +
                            $"Contributions: nodes={snapshot.Contributions.NodeDefinitionProviderCount}, commands={snapshot.Contributions.CommandContributorCount}, presentation={snapshot.Contributions.NodePresentationProviderCount}",
                        },
                });
            }

            _pluginSnapshotItems.ItemsSource = items;
        }

        private void RebuildPluginCandidateItems()
        {
            var items = new List<Control>();

            foreach (var entry in _host.PluginCandidateEntries)
            {
                items.Add(new Border
                {
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Background = Brush.Parse("#111827"),
                    Child = new StackPanel
                    {
                        Spacing = 6,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = entry.DisplayName,
                                FontWeight = FontWeight.SemiBold,
                            },
                            new TextBlock
                            {
                                TextWrapping = TextWrapping.Wrap,
                                Text = entry.SummaryLine,
                            },
                            new TextBlock
                            {
                                TextWrapping = TextWrapping.Wrap,
                                Opacity = 0.78,
                                Text = entry.ProvenanceLine,
                            },
                            new TextBlock
                            {
                                TextWrapping = TextWrapping.Wrap,
                                Opacity = 0.86,
                                Text = entry.TrustLine,
                            },
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Spacing = 8,
                                Children =
                                {
                                    CreateTrustButton("Trust", entry.PluginId, entry.IsBlocked),
                                    CreateBlockButton("Block", entry.PluginId, entry.IsAllowed),
                                },
                            },
                        },
                    },
                });
            }

            _pluginCandidateItems.ItemsSource = items;
        }

        private void RebuildPluginGalleryItems()
        {
            var items = new List<Control>();

            foreach (var entry in _host.LocalPluginGalleryEntries)
            {
                items.Add(new Border
                {
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Background = Brush.Parse("#111827"),
                    Child = new StackPanel
                    {
                        Spacing = 6,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = entry.GalleryLine,
                                FontWeight = FontWeight.SemiBold,
                                TextWrapping = TextWrapping.Wrap,
                            },
                            new TextBlock
                            {
                                TextWrapping = TextWrapping.Wrap,
                                Opacity = 0.86,
                                Text = entry.ManifestLine,
                            },
                            new TextBlock
                            {
                                TextWrapping = TextWrapping.Wrap,
                                Opacity = 0.78,
                                Text = entry.ProvenanceLine,
                            },
                            new TextBlock
                            {
                                TextWrapping = TextWrapping.Wrap,
                                Opacity = 0.86,
                                Text = entry.TrustLine,
                            },
                        },
                    },
                });
            }

            _pluginGalleryItems.ItemsSource = items;
        }

        private Control CreateRuntimePanel()
            => new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    _runtimeSummaryText,
                    _runtimeLogFilter,
                    _runtimeLogItems,
                    CreateRuntimeLogExportButton(),
                },
            };

        private Control CreateGraphSearchPanel()
            => new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    _graphSearchBox,
                    _graphSearchScope,
                    _graphSearchItems,
                },
            };

        private Control CreateNavigationPanel()
            => new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    _navigationHistorySummaryText,
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 6,
                        Children =
                        {
                            CreateNavigationBackButton(),
                            CreateNavigationForwardButton(),
                        },
                    },
                    CreateFocusCurrentScopeButton(),
                    CreateRestoreViewportButton(),
                    _scopeBreadcrumbItems,
                },
            };

        private void RebuildGraphSearchPanel()
        {
            _graphSearchItems.ItemsSource = _host.SearchGraph(_graphSearchQuery, _selectedGraphSearchScope)
                .Select(CreateGraphSearchResultButton)
                .ToList();
        }

        private void RebuildNavigationPanel()
        {
            _navigationHistorySummaryText.Text =
                $"History: {_host.NavigationHistory.Count}\nBack: {_host.CanNavigateBack}\nForward: {_host.CanNavigateForward}";
            if (_navigationBackButton is not null)
            {
                _navigationBackButton.IsEnabled = _host.CanNavigateBack;
            }

            if (_navigationForwardButton is not null)
            {
                _navigationForwardButton.IsEnabled = _host.CanNavigateForward;
            }

            if (_restoreViewportButton is not null)
            {
                _restoreViewportButton.IsEnabled = _host.CanRestoreViewport;
            }

            _scopeBreadcrumbItems.ItemsSource = _host.ScopeBreadcrumbs
                .Select(CreateScopeBreadcrumbButton)
                .ToList();
        }

        private Button CreateNavigationBackButton()
        {
            _navigationBackButton = new Button
            {
                Name = "PART_NavigationBackButton",
                Content = "Back",
                IsEnabled = _host.CanNavigateBack,
            };
            _navigationBackButton.Click += (_, _) =>
            {
                _host.TryNavigateBack();
                RefreshPanels();
            };
            return _navigationBackButton;
        }

        private Button CreateNavigationForwardButton()
        {
            _navigationForwardButton = new Button
            {
                Name = "PART_NavigationForwardButton",
                Content = "Forward",
                IsEnabled = _host.CanNavigateForward,
            };
            _navigationForwardButton.Click += (_, _) =>
            {
                _host.TryNavigateForward();
                RefreshPanels();
            };
            return _navigationForwardButton;
        }

        private Button CreateFocusCurrentScopeButton()
        {
            var button = new Button
            {
                Name = "PART_FocusCurrentScopeButton",
                Content = "Focus scope",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.Click += (_, _) =>
            {
                _host.FocusCurrentScopeForReview();
                RefreshPanels();
            };
            return button;
        }

        private Button CreateRestoreViewportButton()
        {
            _restoreViewportButton = new Button
            {
                Name = "PART_RestoreViewportButton",
                Content = "Restore viewport",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsEnabled = _host.CanRestoreViewport,
            };
            _restoreViewportButton.Click += (_, _) =>
            {
                _host.RestorePreviousViewport();
                RefreshPanels();
            };
            return _restoreViewportButton;
        }

        private Button CreateScopeBreadcrumbButton(ConsumerSampleScopeBreadcrumbEntry entry)
        {
            var button = new Button
            {
                Name = $"PART_ScopeBreadcrumb_{entry.ScopeId.Replace(':', '_')}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = entry.IsCurrent ? $"{entry.Title} (current)" : entry.Title,
                IsEnabled = !entry.IsCurrent,
            };
            button.Click += (_, _) =>
            {
                _host.TryNavigateToScopeBreadcrumb(entry.ScopeId);
                RefreshPanels();
            };
            return button;
        }

        private Button CreateGraphSearchResultButton(ConsumerSampleGraphSearchResult result)
        {
            var button = new Button
            {
                Name = $"PART_GraphSearchResult_{result.Kind}_{result.Id.Replace(':', '_')}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = $"{result.Kind} | {result.Title}\n{result.MatchText}",
            };
            button.Click += (_, _) =>
            {
                _host.TryLocateGraphSearchResult(result);
                RefreshPanels();
            };
            return button;
        }

        private void RebuildRuntimePanel()
        {
            var overlay = _host.RuntimeOverlay;
            _runtimeSummaryText.Text = overlay.IsAvailable
                ? $"Nodes: {overlay.NodeOverlays.Count}\nConnections: {overlay.ConnectionOverlays.Count}\nLogs: {overlay.RecentLogs.Count}"
                : "Runtime overlay provider is not configured.";

            var selection = _host.Session.Queries.GetSelectionSnapshot();
            var filteredLogs = FilterRuntimeLogs(overlay.RecentLogs, selection);
            _runtimeLogItems.ItemsSource = filteredLogs
                .Select(CreateRuntimeLogButton)
                .ToList();
        }

        private IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> FilterRuntimeLogs(
            IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> logs,
            GraphEditorSelectionSnapshot selection)
            => _selectedRuntimeLogFilter switch
            {
                "Selected Node" when selection.PrimarySelectedNodeId is not null
                    => logs.Where(log => string.Equals(log.NodeId, selection.PrimarySelectedNodeId, StringComparison.Ordinal)).ToArray(),
                "Current Scope" => logs.Where(log => string.Equals(log.ScopeId, "root", StringComparison.Ordinal)).ToArray(),
                _ => logs,
            };

        private Button CreateRuntimeLogButton(GraphEditorRuntimeLogEntrySnapshot log)
        {
            var button = new Button
            {
                Name = $"PART_RuntimeLog_{log.Id}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = $"{log.Status} | node={log.NodeId ?? "-"} | scope={log.ScopeId ?? "-"}\n{log.Message}",
            };
            button.Click += (_, _) =>
            {
                _host.TryNavigateToRuntimeLog(log);
                RefreshPanels();
            };
            return button;
        }

        private Button CreateRuntimeLogExportButton()
        {
            var button = new Button
            {
                Name = "PART_ExportRuntimeLogsButton",
                Content = "Export runtime logs",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.Click += (_, _) =>
            {
                _host.ExportRuntimeLogs();
                RefreshPanels();
            };
            return button;
        }

        private Control CreateLayoutPanel()
            => new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    _layoutPreviewSummaryText,
                    CreateLayoutPreviewButton(),
                    CreateLayoutApplyButton(),
                    CreateLayoutCancelButton(),
                },
            };

        private void RebuildLayoutPanel()
        {
            var preview = _host.LayoutPreview;
            _layoutPreviewSummaryText.Text = preview is null
                ? "No layout preview."
                : $"Preview nodes: {preview.NodePositions.Count}\nReset routes: {preview.ResetManualRoutes}";
        }

        private Button CreateLayoutPreviewButton()
        {
            var button = new Button
            {
                Name = "PART_PreviewLayoutButton",
                Content = "Preview layout",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.Click += (_, _) =>
            {
                _host.PreviewLayout();
                RefreshPanels();
            };
            return button;
        }

        private Button CreateLayoutApplyButton()
        {
            var button = new Button
            {
                Name = "PART_ApplyLayoutPreviewButton",
                Content = "Apply layout",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.Click += (_, _) =>
            {
                _host.ApplyLayoutPreview();
                RefreshPanels();
            };
            return button;
        }

        private Button CreateLayoutCancelButton()
        {
            var button = new Button
            {
                Name = "PART_CancelLayoutPreviewButton",
                Content = "Cancel layout",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.Click += (_, _) =>
            {
                _host.CancelLayoutPreview();
                RefreshPanels();
            };
            return button;
        }

        private Control CreateParameterEditor(GraphEditorNodeParameterSnapshot snapshot)
        {
            var content = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"{snapshot.Definition.DisplayName} ({snapshot.Definition.Key})",
                        FontWeight = FontWeight.SemiBold,
                    },
                },
            };

            switch (snapshot.Definition.EditorKind)
            {
                case AsterGraph.Abstractions.Definitions.ParameterEditorKind.Boolean:
                    var checkBox = new CheckBox
                    {
                        IsChecked = snapshot.CurrentValue as bool?,
                        IsEnabled = snapshot.CanEdit,
                    };
                    checkBox.IsCheckedChanged += (_, _) =>
                    {
                        _host.Session.Commands.TrySetSelectedNodeParameterValue(snapshot.Definition.Key, checkBox.IsChecked ?? false);
                        RefreshPanels();
                    };
                    content.Children.Add(checkBox);
                    break;

                case AsterGraph.Abstractions.Definitions.ParameterEditorKind.Enum:
                    var options = snapshot.Definition.Constraints.AllowedOptions;
                    var comboBox = new ComboBox
                    {
                        ItemsSource = options.ToList(),
                        SelectedItem = options.FirstOrDefault(option =>
                            string.Equals(option.Value, snapshot.CurrentValue?.ToString(), StringComparison.Ordinal)),
                        IsEnabled = snapshot.CanEdit,
                    };
                    comboBox.SelectionChanged += (_, _) =>
                    {
                        if (comboBox.SelectedItem is AsterGraph.Abstractions.Definitions.ParameterOptionDefinition option)
                        {
                            _host.Session.Commands.TrySetSelectedNodeParameterValue(snapshot.Definition.Key, option.Value);
                            RefreshPanels();
                        }
                    };
                    content.Children.Add(comboBox);
                    break;

                default:
                    var textBox = new TextBox
                    {
                        Text = snapshot.CurrentValue?.ToString(),
                        IsEnabled = snapshot.CanEdit,
                    };
                    textBox.LostFocus += (_, _) =>
                    {
                        _host.Session.Commands.TrySetSelectedNodeParameterValue(snapshot.Definition.Key, textBox.Text);
                        RefreshPanels();
                    };
                    content.Children.Add(textBox);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(snapshot.Definition.Description))
            {
                content.Children.Add(new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.72,
                    Text = snapshot.Definition.Description,
                });
            }

            return new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Background = Brush.Parse("#0F172A"),
                Child = content,
            };
        }

        private static Border CreateSection(string title, Control content)
            => new()
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Background = Brush.Parse("#0B1220"),
                Child = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontWeight = FontWeight.SemiBold,
                        },
                        content,
                    },
                },
            };

        private Control CreateAllowlistPanel()
        {
            var exportButton = new Button
            {
                Name = "PART_ExportAllowlistButton",
                Content = "Export allowlist",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            exportButton.Click += (_, _) =>
            {
                _host.ExportPluginAllowlist();
                RefreshPanels();
            };

            var importButton = new Button
            {
                Name = "PART_ImportAllowlistButton",
                Content = "Import allowlist",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            importButton.Click += (_, _) =>
            {
                _host.ImportPluginAllowlist();
                RefreshPanels();
            };

            return new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            exportButton,
                            importButton,
                        },
                    },
                    _allowlistItems,
                },
            };
        }

        private AsterGraphHostedActionProjection BuildHostedActionProjection()
        {
            var selection = _host.Session.Queries.GetSelectionSnapshot();
        var commandActions = AsterGraphHostedActionFactory.CreateCommandActions(
            _host.Session,
            [ConsumerSampleHost.PluginCommandId, "workspace.save", "workspace.load", "history.undo", "history.redo", "viewport.fit"]);
        var authoringActions = AsterGraphAuthoringToolActionFactory.CreateCommandSurfaceActions(_host.Session);

        return AsterGraphHostedActionFactory.CreateProjection(
            [
                AsterGraphHostedActionFactory.CreateHostAction(
                    new GraphEditorCommandDescriptorSnapshot(
                        "consumer.add-review",
                        "Add Review Node",
                        "consumer",
                        "add",
                        null,
                        GraphEditorCommandSourceKind.Host,
                        isEnabled: true),
                    _host.AddHostReviewNode),
                AsterGraphHostedActionFactory.CreateHostAction(
                    new GraphEditorCommandDescriptorSnapshot(
                        "consumer.approve-selection",
                        "Approve Selection",
                        "consumer",
                        "approve",
                        null,
                        GraphEditorCommandSourceKind.Host,
                        isEnabled: selection.PrimarySelectedNodeId is not null,
                        disabledReason: selection.PrimarySelectedNodeId is null
                            ? "Select one review node before approving."
                            : null),
                    _host.ApproveSelection),
                .. commandActions,
                .. authoringActions,
            ]);
    }

        private MenuItem CreateMenuItem(AsterGraphHostedActionDescriptor action)
        {
            var item = new MenuItem
            {
                Name = $"PART_ActionMenuItem_{action.Id}",
                Header = action.Title,
                IsEnabled = action.CanExecute,
            };
            item.Click += (_, _) =>
            {
                action.TryExecute();
                RefreshPanels();
            };
            return item;
        }

        private Button CreateActionButton(AsterGraphHostedActionDescriptor action)
        {
            var button = new Button
            {
                Name = ResolveActionButtonName(action),
                Content = action.Title,
                IsEnabled = action.CanExecute,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.Click += (_, _) =>
            {
                action.TryExecute();
                RefreshPanels();
            };
            return button;
        }

        private Button CreateTrustButton(string title, string pluginId, bool isEnabled)
        {
            var button = new Button
            {
                Content = title,
                IsEnabled = isEnabled,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.Click += (_, _) =>
            {
                _host.TrustPluginCandidate(pluginId);
                RefreshPanels();
            };
            return button;
        }

        private Button CreateBlockButton(string title, string pluginId, bool isEnabled)
        {
            var button = new Button
            {
                Content = title,
                IsEnabled = isEnabled,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.Click += (_, _) =>
            {
                _host.BlockPluginCandidate(pluginId);
                RefreshPanels();
            };
            return button;
        }

        private static string ResolveActionButtonName(AsterGraphHostedActionDescriptor action)
            => action.Id switch
            {
                "consumer.add-review" => "PART_AddReviewNodeButton",
                ConsumerSampleHost.PluginCommandId => "PART_AddPluginNodeButton",
                "consumer.approve-selection" => "PART_ApproveSelectionButton",
                "viewport.fit" => "PART_FitViewButton",
                _ => $"PART_Action_{action.Id}",
            };
    }
}
