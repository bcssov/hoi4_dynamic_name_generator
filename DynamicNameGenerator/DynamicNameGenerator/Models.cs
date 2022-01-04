// ***********************************************************************
// Assembly         : DynamicNameGenerator
// Author           : Mario
// Created          : 01-04-2022
//
// Last Modified By : Mario
// Last Modified On : 01-04-2022
// ***********************************************************************
// <copyright file="Models.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DynamicNameGenerator
{
    /// <summary>
    /// Class MainData.
    /// Implements the <see cref="System.ComponentModel.INotifyPropertyChanged" />
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class MainData : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The provinces
        /// </summary>
        private ObservableCollection<ProvinceData> provinces;

        /// <summary>
        /// The state identifier
        /// </summary>
        private long stateId;

        /// <summary>
        /// The state name
        /// </summary>
        private string stateName = string.Empty;

        /// <summary>
        /// The type
        /// </summary>
        private string type = string.Empty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainData"/> class.
        /// </summary>
        public MainData()
        {
            provinces = new ObservableCollection<ProvinceData>();
            HandleProvinceData();
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the provinces.
        /// </summary>
        /// <value>The provinces.</value>
        public ObservableCollection<ProvinceData> Provinces
        {
            get
            {
                return provinces;
            }
        }

        /// <summary>
        /// Gets the provinces text.
        /// </summary>
        /// <value>The provinces text.</value>
        [JsonIgnore]
        public string ProvincesText
        {
            get
            {
                if (Provinces != null && Provinces.Any())
                {
                    var sb = new StringBuilder();
                    Provinces.ToList().ForEach(p =>
                    {
                        sb.AppendFormat("{0} - {1}{2}", p.Id, p.Name, Environment.NewLine);
                    });
                    return sb.ToString().Trim(Environment.NewLine.ToCharArray());
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the state identifier.
        /// </summary>
        /// <value>The state identifier.</value>
        public long StateId
        {
            get
            {
                return stateId;
            }
            set
            {
                stateId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StateId)));
            }
        }

        /// <summary>
        /// Gets or sets the name of the state.
        /// </summary>
        /// <value>The name of the state.</value>
        public string StateName
        {
            get
            {
                return stateName;
            }
            set
            {
                stateName = value ?? string.Empty;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StateName)));
            }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value ?? string.Empty;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Handles the province data.
        /// </summary>
        private void HandleProvinceData()
        {
            Provinces.CollectionChanged += Provinces_CollectionChanged;
        }

        /// <summary>
        /// Called when [provinces changed].
        /// </summary>
        private void OnProvincesChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Provinces)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProvincesText)));
        }

        /// <summary>
        /// Handles the PropertyChanged event of the PropertyChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void PropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnProvincesChanged();
        }

        /// <summary>
        /// Handles the CollectionChanged event of the Provinces control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void Provinces_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnProvincesChanged();
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is INotifyPropertyChanged propertyChanged)
                    {
                        propertyChanged.PropertyChanged += PropertyChanged_PropertyChanged;
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is INotifyPropertyChanged propertyChanged)
                    {
                        propertyChanged.PropertyChanged -= PropertyChanged_PropertyChanged;
                    }
                }
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// Class ProvinceData.
    /// Implements the <see cref="System.ComponentModel.INotifyPropertyChanged" />
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class ProvinceData : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The identifier
        /// </summary>
        private long id;

        /// <summary>
        /// The name
        /// </summary>
        private string name = string.Empty;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public long Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value ?? string.Empty;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        #endregion Properties
    }
}