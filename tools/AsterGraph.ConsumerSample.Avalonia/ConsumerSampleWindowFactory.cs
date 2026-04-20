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
        private readonly ItemsControl _pluginSnapshotItems;
        private readonly ItemsControl _allowlistItems;
        private readonly TextBlock _selectionSummaryText;
        private readonly TextBlock _trustBoundaryText;

        public ConsumerSampleWindow(ConsumerSampleHost host, bool ownsHost)
        {
            _host = host;
            _ownsHost = ownsHost;
            Title = "AsterGraph Consumer Sample";
            Width = 1560;
            Height = 980;

            var editorView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
            {
                Editor = _host.Editor,
                ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
                EnableDefaultContextMenu = true,
                EnableDefaultCommandShortcuts = true,
            });
            editorView.Name = "PART_EditorView";

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

            _pluginSnapshotItems = new ItemsControl
            {
                Name = "PART_PluginSnapshotItems",
            };

            _allowlistItems = new ItemsControl
            {
                Name = "PART_AllowlistItems",
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
                    CreateSection("Selected Parameters", _parameterItems),
                    CreateSection("Plugin Candidates", _pluginCandidateItems),
                    CreateSection("Plugin Load Snapshots", _pluginSnapshotItems),
                    CreateSection("Allowlist Decisions", CreateAllowlistPanel()),
                    CreateSection("Trust Boundary", _trustBoundaryText),
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
                    editorView,
                    rightPanel,
                },
            };

            Grid.SetColumn(leftRail, 0);
            Grid.SetColumn(editorView, 1);
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
            RebuildPluginSnapshotItems();
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
            var actions = BuildHostedActions();
            _actionRailItems.Children.Clear();
            foreach (var action in actions)
            {
                _actionRailItems.Children.Add(CreateActionButton(action));
            }

            _actionsMenu.ItemsSource = actions.Select(CreateMenuItem).ToList();
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

        private IReadOnlyList<AsterGraphHostedActionDescriptor> BuildHostedActions()
        {
            var selection = _host.Session.Queries.GetSelectionSnapshot();
            var commandActions = AsterGraphHostedActionFactory.CreateCommandActions(
                _host.Session,
                [ConsumerSampleHost.PluginCommandId, "workspace.save", "workspace.load", "history.undo", "history.redo", "viewport.fit"]);

            return
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
            ];
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
