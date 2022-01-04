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
        private List<ProvinceData> provinces = new List<ProvinceData>();

        /// <summary>
        /// The state identifier
        /// </summary>
        private long stateId;

        /// <summary>
        /// The type
        /// </summary>
        private string type = string.Empty;

        #endregion Fields

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
        public List<ProvinceData> Provinces
        {
            get
            {
                return provinces;
            }
            set
            {
                provinces = value ?? new List<ProvinceData>();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Provinces)));
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
                    Provinces.ForEach(p =>
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