using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Newtonsoft.Json;

using Windows.ApplicationModel.DataTransfer;

namespace RobloxStudioModManager
{
    public sealed partial class FlagEditorWindow : Window
    {
        internal class ClientSettings
        {
            public Dictionary<string, string> ApplicationSettings = null;
        }

        private static SortedDictionary<string, FVariable> flagRegistry => App.State.FlagEditor;
        private static VersionManifest versionRegistry => App.State.VersionData;

        private static readonly IReadOnlyDictionary<string, string> flagTypes = new Dictionary<string, string>()
        {
            {"Flag",   "false"},
            {"Int",    "0"},
            {"String", " "},
            {"Log",    "0"},
        };

        private const string OVERRIDE_STATUS_ON = "Values highlighted in red were overridden locally.";

        // Same red used for the button/tab strokes, as a translucent row fill.
        private static readonly SolidColorBrush overriddenBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(0x33, 0xB3, 0x24, 0x2C));
        private static readonly SolidColorBrush transparentBrush = new SolidColorBrush(Colors.Transparent);

        private List<FVariable> flags = new List<FVariable>();
        private List<FVariable> allFlags = new List<FVariable>();

        private readonly Dictionary<string, FVariable> overrideLookup = new Dictionary<string, FVariable>();
        private List<FVariable> overrides = new List<FVariable>();

        private static FVariable lastCustomFlag = null;
        private readonly Dictionary<string, int> flagLookup = new Dictionary<string, int>();

        private DispatcherTimer toastTimer;

        public FlagEditorWindow()
        {
            InitializeComponent();
            Title = "Flag Editor";

            this.ApplyBackdrop();
            WindowPositioning.ResizeAndCenter(AppWindow, 560, 760);

            Closed += FlagEditorWindow_Closed;
            Activated += FlagEditorWindow_Activated;
        }

        private bool loaded;

        private async void FlagEditorWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (loaded)
                return;

            loaded = true;

            // Grid/Panel has no IsEnabled, so we just block interaction.
            Tabs.IsHitTestVisible = false;
            LoadingRing.IsActive = true;

            try
            {
                await InitializeEditor();
            }
            finally
            {
                Tabs.IsHitTestVisible = true;
                LoadingRing.IsActive = false;
            }
        }

        private void FlagEditorWindow_Closed(object sender, WindowEventArgs args)
        {
            App.SaveState();
        }

        private static string getFlagKey(string type, string name) => type + name;

        private void showOverrideStatus()
        {
            OverrideStatusMessage.Text = OVERRIDE_STATUS_ON;
            OverrideStatusBar.Visibility = Visibility.Visible;
        }

        private void hideOverrideStatus()
        {
            OverrideStatusBar.Visibility = Visibility.Collapsed;
        }

        private void refreshOverrideList()
        {
            var sorted = overrideLookup.Values
                .OrderBy(flag => flag.Name)
                .ToList();

            overrides = sorted;
            OverrideListView.ItemsSource = null;
            OverrideListView.ItemsSource = overrides;
        }

        private void addFlagOverride(FVariable flag, bool init = false)
        {
            string key = flag.Key;

            if (!overrideLookup.ContainsKey(key))
            {
                if (!init && flag.Type.EndsWith("Flag", App.StringFormat))
                {
                    if (bool.TryParse(flag.Value, out bool value))
                    {
                        string str = (!value)
                            .ToString(App.Format)
                            .ToLower(App.Format);

                        flag.SetValue(str);
                    }
                }

                overrideLookup.Add(key, flag);
            }

            showOverrideStatus();

            if (!init)
            {
                refreshOverrideList();
                lastCustomFlag = null;
                Tabs.SelectedIndex = 1;
                flagRegistry[key] = flag;
            }
        }

        private void refreshFlags()
        {
            if (allFlags == null)
                return;

            string search = SearchBox.Text.ToLowerInvariant();

            flags = allFlags
                .Where(flag => flag.Name.ToLowerInvariant().Contains(search))
                .OrderBy(flag => flag.Name)
                .ToList();

            flagLookup.Clear();

            for (int i = 0; i < flags.Count; i++)
                flagLookup[flags[i].Key] = i;

            FlagListView.ItemsSource = null;
            FlagListView.ItemsSource = flags;
        }

        private async Task InitializeEditor()
        {
            var flagNames = new HashSet<string>();
            var studioPath = StudioBootstrapper.GetStudioPath();

            var studioDir = StudioBootstrapper.GetStudioDirectory();

            string lastFlagScanVersion = versionRegistry.LastFlagScanVersion;
            string versionGuid = versionRegistry.VersionGuid;

            var cppFlags = StudioFFlagDumper.DumpCppFlags(studioPath);
            var flagDump = Path.Combine(studioDir, "FFlags.json");
            var flagInfo = new FileInfo(flagDump);

            if (lastFlagScanVersion != versionGuid || !flagInfo.Exists)
            {
                cppFlags.ForEach(flag => flagNames.Add(flag));

                var newJson = JsonConvert.SerializeObject(flagNames);
                File.WriteAllText(flagDump, newJson);

                versionRegistry.LastFlagScanVersion = versionGuid;
                App.SaveState();
            }

            var rawFlagNames = File.ReadAllText(flagDump);
            var cachedFlagNames = JsonConvert.DeserializeObject<string[]>(rawFlagNames);

            foreach (var name in cachedFlagNames)
                flagNames.Add(name);

            // Initialize flag browser
            var json = new Dictionary<string, string>();

            using (var http = new WebClient())
            {
                var settings = await http.DownloadStringTaskAsync("https://clientsettingscdn.roblox.com/v2/settings/application/PCDesktopClient");
                var data = JsonConvert.DeserializeObject<ClientSettings>(settings);

                foreach (var pair in data.ApplicationSettings)
                {
                    string key = pair.Key;

                    if (key.EndsWith("_PlaceFilter"))
                        continue;

                    json.Add(key, pair.Value);
                }
            }

            foreach (var key in flagNames)
            {
                if (!json.ContainsKey(key))
                {
                    string flagClass = "";

                    if (key.StartsWith("SF"))
                        flagClass = "SF";
                    else if (key.StartsWith("DF"))
                        flagClass = "DF";
                    else if (key.StartsWith("F"))
                        flagClass = "F";

                    if (flagClass != "")
                    {
                        var prefix = key.Substring(flagClass.Length);

                        foreach (var pair in flagTypes)
                        {
                            if (prefix.StartsWith(pair.Key))
                            {
                                json.Add(key, pair.Value);
                                break;
                            }
                        }
                    }
                    else
                    {
                        json.Add(key, "??");
                    }
                }
            }

            int numFlags = json.Count;
            var flagSetup = new List<FVariable>(numFlags);

            foreach (string customFlag in flagNames)
            {
                if (!json.ContainsKey(customFlag))
                {
                    if (!flagRegistry.TryGetValue(customFlag, out var flag))
                        continue;

                    if (!flag.Custom)
                        continue;

                    flagSetup.Add(flag);
                }
            }

            foreach (var pair in json)
            {
                string key = pair.Key,
                       value = pair.Value;

                if (!flagRegistry.TryGetValue(key, out var flag))
                    flag = new FVariable(key, value);

                if (flag.Type == "")
                    continue;

                flagSetup.Add(flag);
            }

            allFlags = flagSetup
                .OrderBy(flag => flag.Key)
                .ToList();

            refreshFlags();

            // Initialize override list.
            foreach (var flagName in flagRegistry.Keys)
            {
                var flag = flagRegistry[flagName];
                addFlagOverride(flag, true);
            }

            refreshOverrideList();

            if (overrideLookup.Count == 0)
                hideOverrideStatus();

            // StudioFFlagDumper dumps the flags into a very large byte array,
            // and since this allocation is only collected on a full Gen2 pass,
            // this prevents this memory from being collected from an ordinary
            // GC.Collect() call. So we set the collection mode to aggressive,
            // to force .NET to return the memory back to the OS.
            GC.Collect(2, GCCollectionMode.Aggressive, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Aggressive, true, true);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshFlags();
        }

        private void OverrideSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (FlagListView.SelectedItem is FVariable selectedFlag)
                addFlagOverride(selectedFlag);
        }

        private void RemoveSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (OverrideListView.SelectedItem is FVariable flag)
            {
                flag.Clear();

                overrideLookup.Remove(flag.Key);
                flagRegistry.Remove(flag.Key);

                refreshOverrideList();
            }

            if (overrideLookup.Count == 0)
                hideOverrideStatus();
        }

        private async void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await DialogWindow.ShowAsync(
                "Confirmation",
                "Are you sure you would like to remove all flag overrides?\nThis will also delete any custom flags!",
                "Yes", "No", ContentDialogButton.Close, DialogIcon.Warning);

            if (result != ContentDialogResult.Primary)
                return;

            var flagNames = flagRegistry.Keys.ToList();

            foreach (string flagName in flagNames)
            {
                var flag = flagRegistry[flagName];
                flag.Clear();

                flagRegistry.Remove(flagName);
            }

            overrideLookup.Clear();
            refreshOverrideList();
            hideOverrideStatus();
        }

        private void ValueBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (!(textBox.Tag is FVariable flag))
                return;

            string value = textBox.Text;
            bool badInput = false;

            if (flag.Type.EndsWith("Int", App.StringFormat))
                badInput = !int.TryParse(value, out _);

            if (badInput)
                textBox.Text = flag.Reset;
            else
                flag.SetValue(value);
        }

        private async void AddCustomButton_Click(object sender, RoutedEventArgs e)
        {
            var form = new FlagCreatorDialog();

            var dialog = new DialogWindow
            {
                Title = "Add Custom Flag",
                DialogContent = form,
                PrimaryButtonText = "Create",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            dialog.PrimaryButtonClick += (s, args) => args.Cancel = !form.Validate();

            // No notification sound for this one since it's opened directly by a
            // button click, so the extra sound is unnecessary.
            var result = await dialog.ShowAsync(playSound: false);

            if (result != ContentDialogResult.Primary || form.Result == null)
                return;

            var customFlag = form.Result;
            string flagType = customFlag.Type;

            string flagValue = flagTypes
                .Select(pair => pair.Key)
                .Where(key => flagType.EndsWith(key, App.StringFormat))
                .Select(key => flagTypes[key])
                .FirstOrDefault();

            string flagName = flagType + customFlag.Name;
            var newFlag = new FVariable(flagName, flagValue, true);

            flagRegistry[flagName] = newFlag;
            lastCustomFlag = newFlag;

            addFlagOverride(newFlag);
        }

        private void FlagListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
                return;

            var flag = (FVariable)args.Item;

            // Outer layering Grid:
            // [0] = HighlightBorder, [1] = the actual 3-column text content
            var outer = (Grid)args.ItemContainer.ContentTemplateRoot;
            var highlightBorder = (Border)outer.Children[0];
            var root = (Grid)outer.Children[1];
            root.Tag = flag;

            var nameText = (TextBlock)root.Children[0];
            var typeText = (TextBlock)((Border)root.Children[1]).Child;
            var valueText = (TextBlock)((Border)root.Children[2]).Child;

            nameText.Text = flag.Name;
            typeText.Text = flag.Type;
            valueText.Text = flag.Value;

            highlightBorder.Background = flagRegistry.ContainsKey(flag.Key) ? overriddenBrush : transparentBrush;

            // Zero the container's own padding so the outer Grid
            // reaches the row's true edges, the visual
            // padding around the text now comes from the inner Grid's own
            // Padding instead.
            var container = (ListViewItem)args.ItemContainer;
            container.Padding = new Thickness(0);
            container.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            container.VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        private void OverrideListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
                return;

            var flag = (FVariable)args.Item;
            var root = (Grid)args.ItemContainer.ContentTemplateRoot;

            var nameText = (TextBlock)root.Children[0];
            var typeText = (TextBlock)((Border)root.Children[1]).Child;
            var valueHost = (Grid)((Border)root.Children[2]).Child;
            var valueBox = (TextBox)valueHost.Children[0];
            var valueCombo = (ComboBox)valueHost.Children[1];

            nameText.Text = flag.Name;
            typeText.Text = flag.Type;

            bool isBoolFlag = flag.Type.EndsWith("Flag", App.StringFormat);

            valueBox.Tag = flag;
            valueCombo.Tag = flag;

            if (isBoolFlag)
            {
                valueBox.Visibility = Visibility.Collapsed;
                valueCombo.Visibility = Visibility.Visible;

                bool.TryParse(flag.Value, out bool boolValue);
                valueCombo.SelectedIndex = boolValue ? 0 : 1; // ComboBoxItems are "true", "false" in that order
            }
            else
            {
                valueBox.Visibility = Visibility.Visible;
                valueCombo.Visibility = Visibility.Collapsed;
                valueBox.Text = flag.Value;
            }
        }

        private void ValueCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;

            if (!(combo.Tag is FVariable flag))
                return;

            if (combo.SelectedItem is ComboBoxItem item)
                flag.SetValue(item.Content.ToString().ToLowerInvariant());
        }

        private void FlagRow_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).Tag is FVariable flag))
                return;

            var dataPackage = new DataPackage();
            dataPackage.SetText(flag.Value);
            Clipboard.SetContent(dataPackage);

            showCopyToast();
        }

        private void showCopyToast()
        {
            CopyToast.Visibility = Visibility.Visible;

            toastTimer?.Stop();
            toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };

            toastTimer.Tick += (s, e) =>
            {
                toastTimer.Stop();
                CopyToast.Visibility = Visibility.Collapsed;
            };

            toastTimer.Start();
        }
    }
}
