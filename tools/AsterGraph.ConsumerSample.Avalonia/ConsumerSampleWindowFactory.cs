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
        private readonly ItemsControl _parameterItems;
        private readonly ItemsControl _pluginSnapshotItems;
        private readonly TextBlock _selectionSummaryText;

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

            var menu = new Menu
            {
                Name = "PART_MainMenu",
                ItemsSource = new[]
                {
                    CreateMenuItem("Actions",
                    new[]
                    {
                        CreateMenuItem("Add Review Node", (_, _) => { _host.AddHostReviewNode(); RefreshPanels(); }),
                        CreateMenuItem("Add Plugin Audit Node", (_, _) => { _host.AddPluginAuditNode(); RefreshPanels(); }),
                        CreateMenuItem("Approve Selection", (_, _) => { _host.ApproveSelection(); RefreshPanels(); }),
                        CreateMenuItem("Fit View", (_, _) => _host.FitView()),
                    }),
                },
            };

            var leftRail = new StackPanel
            {
                Spacing = 10,
                Width = 250,
                Children =
                {
                    CreateActionButton("PART_AddReviewNodeButton", "Add Review Node", () => _host.AddHostReviewNode()),
                    CreateActionButton("PART_AddPluginNodeButton", "Add Plugin Audit Node", () => _host.AddPluginAuditNode()),
                    CreateActionButton("PART_ApproveSelectionButton", "Approve Selection", _host.ApproveSelection),
                    CreateActionButton("PART_FitViewButton", "Fit View", () => { _host.FitView(); return true; }),
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

            _pluginSnapshotItems = new ItemsControl
            {
                Name = "PART_PluginSnapshotItems",
            };

            var trustBoundary = new TextBlock
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
                    CreateSection("Plugin Load Snapshots", _pluginSnapshotItems),
                    CreateSection("Trust Boundary", trustBoundary),
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
            var selection = _host.Session.Queries.GetSelectionSnapshot();
            _selectionSummaryText.Text = selection.PrimarySelectedNodeId is null
                ? "No node is selected."
                : $"Primary node: {selection.PrimarySelectedNodeId}\nSelected nodes: {selection.SelectedNodeIds.Count}";

            RebuildParameterItems();
            RebuildPluginSnapshotItems();
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
                            $"Contributions: nodes={snapshot.Contributions.NodeDefinitionProviderCount}, menu={snapshot.Contributions.ContextMenuAugmentorCount}, presentation={snapshot.Contributions.NodePresentationProviderCount}",
                        },
                });
            }

            _pluginSnapshotItems.ItemsSource = items;
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

        private static MenuItem CreateMenuItem(string header, IReadOnlyList<MenuItem> items)
            => new()
            {
                Header = header,
                ItemsSource = items,
            };

        private static MenuItem CreateMenuItem(string header, EventHandler<RoutedEventArgs> onClick)
        {
            var item = new MenuItem
            {
                Header = header,
            };
            item.Click += onClick;
            return item;
        }

        private Button CreateActionButton(string name, string label, Func<bool> action)
        {
            var button = new Button
            {
                Name = name,
                Content = label,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            button.Click += (_, _) =>
            {
                action();
                RefreshPanels();
            };
            return button;
        }
    }
}
