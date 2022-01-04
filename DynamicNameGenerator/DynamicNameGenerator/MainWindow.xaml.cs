﻿// ***********************************************************************
// Assembly         : DynamicNameGenerator
// Author           : Mario
// Created          : 01-04-2022
//
// Last Modified By : Mario
// Last Modified On : 01-04-2022
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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DynamicNameGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        /// <summary>
        /// The grid data
        /// </summary>
        private ObservableCollection<MainData> gridData = new ObservableCollection<MainData>();

        /// <summary>
        /// The grid view source
        /// </summary>
        private CollectionViewSource gridViewSource;

        /// <summary>
        /// The visible items
        /// </summary>
        private int visibleItems = 0;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Window));
            DataContext = this;
            InitializeGrid();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Handles the AddingNewItem event of the dataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AddingNewItemEventArgs"/> instance containing the event data.</param>
        private void dataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            filter.Text = string.Empty;
        }

        /// <summary>
        /// Handles the LoadingRow event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridRowEventArgs"/> instance containing the event data.</param>
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
        /// Initializes the grid.
        /// </summary>
        private void InitializeGrid()
        {
            var json = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json"));
            gridData = new ObservableCollection<MainData>(JsonConvert.DeserializeObject<List<MainData>>(json) ?? new List<MainData>());

            gridViewSource = new CollectionViewSource() { Source = gridData };
            gridViewSource.View.Filter = null;
            visibleItems = gridData.Count;
            dataGrid.ItemsSource = gridViewSource.View;
            filter.Text = string.Empty;
            gridViewSource.View.CollectionChanged += View_CollectionChanged;
        }

        /// <summary>
        /// Handles the TextChanged event of the TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs"/> instance containing the event data.</param>
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var text = ((TextBox)sender).Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                gridViewSource.View.Filter = null;
                visibleItems = gridData.Count;
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
        /// Handles the CollectionChanged event of the View control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
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
        }

        #endregion Methods
    }
}