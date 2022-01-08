// ***********************************************************************
// Assembly         : DynamicNameGenerator
// Author           : Mario
// Created          : 01-08-2022
//
// Last Modified By : Mario
// Last Modified On : 01-08-2022
// ***********************************************************************
// <copyright file="App.xaml.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DynamicNameGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        public App()
        {
            SetupUnhandledExceptionHandling();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Logs the specified e.
        /// </summary>
        /// <param name="e">The e.</param>
        private void Log(Exception e)
        {
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.log"), e.ToString());
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Setups the unhandled exception handling.
        /// </summary>
        private void SetupUnhandledExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log(args.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (sender, args) => Log(args.Exception);
            Dispatcher.UnhandledException += (sender, args) =>
            {
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    Log(args.Exception);
                }
            };
        }

        #endregion Methods
    }
}