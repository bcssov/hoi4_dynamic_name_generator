// ***********************************************************************
// Assembly         : DynamicNameGenerator
// Author           : Mario
// Created          : 01-04-2022
//
// Last Modified By : Mario
// Last Modified On : 01-13-2022
// ***********************************************************************
// <copyright file="MainWindow.xaml.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DynamicNameGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The code exporter
        /// </summary>
        private CodeExporter codeExporter;

        /// <summary>
        /// The duplicates
        /// </summary>
        private HashSet<MainData> duplicates = null;

        /// <summary>
        /// The exclude type
        /// </summary>
        private string excludeType = string.Empty;

        /// <summary>
        /// The filtering all
        /// </summary>
        private bool filteringAll;

        /// <summary>
        /// The filtering provinces
        /// </summary>
        private bool filteringProvinces;

        /// <summary>
        /// The filtering states
        /// </summary>
        private bool filteringStates;

        /// <summary>
        /// The filter type
        /// </summary>
        private FilterType filterType;

        /// <summary>
        /// The grid data
        /// </summary>
        private ObservableCollection<MainData> gridData = new ObservableCollection<MainData>();

        /// <summary>
        /// The grid view source
        /// </summary>
        private CollectionViewSource gridViewSource;

        /// <summary>
        /// The lock form
        /// </summary>
        private object lockForm = new { };

        /// <summary>
        /// The types
        /// </summary>
        private ObservableCollection<string> types = new ObservableCollection<string>();

        /// <summary>
        /// The typing token
        /// </summary>
        private CancellationTokenSource typingToken;

        /// <summary>
        /// The visible items
        /// </summary>
        private int visibleItems = 0;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Window));
            DataContext = this;
            codeExporter = new CodeExporter(AppDomain.CurrentDomain.BaseDirectory);
            InitializeGrid();
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Enums

        /// <summary>
        /// Enum FilterType
        /// </summary>
        private enum FilterType
        {
            /// <summary>
            /// All
            /// </summary>
            All,

            /// <summary>
            /// The duplicate states
            /// </summary>
            DuplicateStates,

            /// <summary>
            /// The duplicate provinces
            /// </summary>
            DuplicateProvinces
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [filtering all].
        /// </summary>
        /// <value><c>true</c> if [filtering all]; otherwise, <c>false</c>.</value>
        public bool FilteringAll
        {
            get
            {
                return filteringAll;
            }
            set
            {
                filteringAll = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilteringAll)));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [filtering provinces].
        /// </summary>
        /// <value><c>true</c> if [filtering provinces]; otherwise, <c>false</c>.</value>
        public bool FilteringProvinces
        {
            get
            {
                return filteringProvinces;
            }
            set
            {
                filteringProvinces = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilteringProvinces)));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [filtering states].
        /// </summary>
        /// <value><c>true</c> if [filtering states]; otherwise, <c>false</c>.</value>
        public bool FilteringStates
        {
            get
            {
                return filteringStates;
            }
            set
            {
                filteringStates = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilteringStates)));
            }
        }

        /// <summary>
        /// Gets or sets the types.
        /// </summary>
        /// <value>The types.</value>
        public ObservableCollection<string> Types
        {
            get
            {
                return types;
            }
            set
            {
                var old = types;
                types = value;
                if (!(types != null && old != null && types.SequenceEqual(old)))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Types)));
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Handles the AddingNewItem event of the dataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AddingNewItemEventArgs" /> instance containing the event data.</param>
        private void dataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            filter.Text = string.Empty;
        }

        /// <summary>
        /// Handles the LoadingRow event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridRowEventArgs" /> instance containing the event data.</param>
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.GetIndex() < visibleItems)
            {
                e.Row.Header = (e.Row.GetIndex() + 1).ToString();
            }
            else
            {
                e.Row.Header = string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the DuplicateOff control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void DuplicateOff_Click(object sender, RoutedEventArgs e)
        {
            filterType = FilterType.All;
            SetFiltering();
            FilterResults(filter.Text);
        }

        /// <summary>
        /// Handles the Click event of the DuplicateProvince control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void DuplicateProvince_Click(object sender, RoutedEventArgs e)
        {
            filterType = FilterType.DuplicateProvinces;
            SetFiltering();
            FilterResults(filter.Text);
        }

        /// <summary>
        /// Handles the Click event of the DuplicateState control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void DuplicateState_Click(object sender, RoutedEventArgs e)
        {
            filterType = FilterType.DuplicateStates;
            SetFiltering();
            FilterResults(filter.Text);
        }

        /// <summary>
        /// Handles the Click event of the Exit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Filters the results.
        /// </summary>
        /// <param name="text">The text.</param>
        private void FilterResults(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                if (filterType == FilterType.All)
                {
                    gridViewSource.View.Filter = null;
                    visibleItems = gridData.Count;
                }
                else
                {
                    gridViewSource.View.Filter = new Predicate<object>(p =>
                    {
                        var model = (MainData)p;
                        return duplicates.Contains(model);
                    });
                }
            }
            else
            {
                string filter = string.Empty;
                string term = text;
                var split = text.Split(":");
                if (split.Count() == 2)
                {
                    filter = split[0];
                    term = split[1];
                }
                gridViewSource.View.Filter = new Predicate<object>(p =>
                {
                    var model = (MainData)p;
                    if (duplicates != null && !duplicates.Contains(model))
                    {
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        return model.Type.Contains(term, StringComparison.OrdinalIgnoreCase) || model.StateId.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            model.StateName.Contains(term, StringComparison.OrdinalIgnoreCase) || model.ProvincesText.Contains(term, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        switch (filter.ToLowerInvariant())
                        {
                            case "type":
                                return model.Type.Contains(term, StringComparison.OrdinalIgnoreCase);

                            case "stateid":
                                return model.StateId.ToString().Contains(term, StringComparison.OrdinalIgnoreCase);

                            case "statename":
                                return model.StateName.Contains(term, StringComparison.OrdinalIgnoreCase);

                            case "provinces":
                                return model.ProvincesText.Contains(term, StringComparison.OrdinalIgnoreCase);

                            default:
                                return model.Type.Contains(term, StringComparison.OrdinalIgnoreCase) || model.StateId.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                    model.StateName.Contains(term, StringComparison.OrdinalIgnoreCase) || model.ProvincesText.Contains(term, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                });
                visibleItems = gridViewSource.View.Cast<object>().Count() - 1;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        private void Grid_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Task.Run(() => SaveData());
        }

        /// <summary>
        /// Initializes the grid.
        /// </summary>
        private void InitializeGrid()
        {
            var json = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json"));
            var data = JsonConvert.DeserializeObject<List<MainData>>(json) ?? new List<MainData>();
            data = data.Select(p => new MainData(p.Type.ToLowerInvariant(), p.StateId, p.StateName, p.Provinces, true)).ToList().OrderBy(p => p.Type).ThenBy(p => p.StateId).ToList();
            gridData = new ObservableCollection<MainData>(data);

            gridViewSource = new CollectionViewSource() { Source = gridData };
            gridViewSource.View.Filter = null;
            visibleItems = gridData.Count;
            dataGrid.ItemsSource = gridViewSource.View;
            filter.Text = string.Empty;
            gridViewSource.View.CollectionChanged += View_CollectionChanged;
            foreach (var item in gridData)
            {
                if (item is INotifyPropertyChanged propertyChanged)
                {
                    propertyChanged.PropertyChanged += Grid_PropertyChanged;
                }
            }
            Types = new ObservableCollection<string>(gridData.GroupBy(p => p.Type).Select(p => p.Key));
            FilteringAll = true;
        }

        /// <summary>
        /// Handles the PreviewTextInput event of the NumberTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TextCompositionEventArgs" /> instance containing the event data.</param>
        private void NumberTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        private void SaveData()
        {
            lock (lockForm)
            {
                var data = gridData.Select(p => new MainData(p.Type.ToLowerInvariant(), p.StateId, p.StateName, p.Provinces, true)).ToList().OrderBy(p => p.Type).ThenBy(p => p.StateId).ToList();
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                var file = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json");
                var backup = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json.bak");
                if (File.Exists(file))
                {
                    if (File.Exists(backup))
                    {
                        File.Delete(backup);
                    }
                    File.Move(file, backup);
                }
                File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json"), json);
            }
        }

        /// <summary>
        /// Handles the Click event of the SaveItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            codeExporter.Export(gridData);
        }

        /// <summary>
        /// Sets the filtering.
        /// </summary>
        private void SetFiltering()
        {
            FilteringAll = false;
            FilteringProvinces = false;
            FilteringStates = false;
            switch (filterType)
            {
                case FilterType.All:
                    FilteringAll = true;
                    duplicates = null;
                    break;

                case FilterType.DuplicateStates:
                    FilteringStates = true;
                    duplicates = gridData.GroupBy(p => p.Type).Select(p => p.GroupBy(x => x.StateId).Where(x => x.Count() > 1)).SelectMany(p => p.SelectMany(x => x)).ToHashSet();
                    break;

                case FilterType.DuplicateProvinces:
                    FilteringProvinces = true;
                    duplicates = gridData.GroupBy(p => p.Type).Where(p => p.Any(p => p.Provinces.Count > 0)).Select(p => p.Where(x => x.Provinces.GroupBy(g => g.Id).Any(p => p.Count() > 1))).SelectMany(p => p).ToHashSet();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                FocusManager.SetFocusedElement(this, textBox);
                textBox.CaretIndex = textBox.Text.Length;
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs" /> instance containing the event data.</param>
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            async Task delay(string text, CancellationToken token)
            {
                await Task.Delay(250, token);
                if (!token.IsCancellationRequested)
                {
                    FilterResults(text);
                }
            }
            var text = ((TextBox)sender).Text;
            if (typingToken != null)
            {
                typingToken.Cancel();
            }
            typingToken = new CancellationTokenSource();
            delay(text, typingToken.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the TextChanged event of the Type control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void Type_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = ((TextBox)sender);
            if (textBox.IsFocused)
            {
                var text = textBox.Text;
                excludeType = text;
                if ((text ?? string.Empty).Contains(" "))
                {
                    text = text.Replace(" ", string.Empty);
                    excludeType = text;
                    textBox.Text = text;
                    textBox.CaretIndex = text.Length;
                }
                var groupped = gridData.GroupBy(p => p.Type);
                var singleEntries = groupped.Where(p => excludeType == p.Key && p.Count() <= 1);
                Types = new ObservableCollection<string>(gridData.GroupBy(p => p.Type).Where(p => !singleEntries.Any(a => a.Key.Equals(p.Key))).Select(p => p.Key));
            }
            excludeType = string.Empty;
        }

        /// <summary>
        /// Handles the CollectionChanged event of the View control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        private void View_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var oldValue = visibleItems;
            visibleItems = gridViewSource.View.Cast<object>().Count() - 1;
            try
            {
                if (visibleItems != oldValue)
                {
                    dataGrid.Items.Refresh();
                }
            }
            catch // Otherwise crashes the app, no any flags wnich can be used to determine if can or cannot refresh and am too lazy to fix properly
            {
            }
            Task.Run(() => SaveData());
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is INotifyPropertyChanged propertyChanged)
                    {
                        propertyChanged.PropertyChanged += Grid_PropertyChanged;
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is INotifyPropertyChanged propertyChanged)
                    {
                        propertyChanged.PropertyChanged -= Grid_PropertyChanged;
                    }
                }
            }
        }

        #endregion Methods
    }
}