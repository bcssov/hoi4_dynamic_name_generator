// ***********************************************************************
// Assembly         : DynamicNameGenerator
// Author           : Mario
// Created          : 01-05-2022
//
// Last Modified By : Mario
// Last Modified On : 01-05-2022
// ***********************************************************************
// <copyright file="CodeExporter.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using SmartFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DynamicNameGenerator
{
    /// <summary>
    /// Class CodeExporter.
    /// </summary>
    internal class CodeExporter
    {
        #region Fields

        /// <summary>
        /// The base export path
        /// </summary>
        private string baseExportPath;

        /// <summary>
        /// The base template
        /// </summary>
        private string baseTemplate;

        /// <summary>
        /// The scripted effects
        /// </summary>
        private string scriptedEffects;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeExporter" /> class.
        /// </summary>
        /// <param name="baseExportPath">The base export path.</param>
        public CodeExporter(string baseExportPath)
        {
            this.baseExportPath = baseExportPath;
            scriptedEffects = Path.Combine(baseExportPath, "common\\scripted_effects");
            baseTemplate = File.ReadAllText(Path.Combine(baseExportPath, "code.txt"));
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Exports this instance.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Export(IEnumerable<MainData> data)
        {
            if (data == null || data.Count() == 0)
            {
                return;
            }
            if (Directory.Exists(scriptedEffects))
            {
                Directory.Delete(scriptedEffects, true);
            }
            Directory.CreateDirectory(scriptedEffects);
            var clearFlags = new StringBuilder(Environment.NewLine) ;
            data.GroupBy(p => p.Type).Select(p => p.FirstOrDefault()).ToList().ForEach(p =>
            {
                clearFlags.AppendFormat("{2}clr_state_flag = state_name_{0}{1}", p.Type, Environment.NewLine, new string(' ', 8));
            });
            foreach (var types in data.GroupBy(p => p.Type))
            {
                var export = new StringBuilder();
                export.AppendLine($"apply_{types.FirstOrDefault().Type}_name = {{");
                foreach (var type in types)
                {
                    var provinceNames = new StringBuilder(Environment.NewLine);
                    type.Provinces.ToList().ForEach(p =>
                    {
                        provinceNames.AppendFormat("{2}set_province_name = {{ id = {0} name = \"{1}\" }}{3}", p.Id, p.Name, new string(' ', 8), Environment.NewLine);
                    });
                    var code = Smart.Format(baseTemplate, new
                    {
                        state_id = type.StateId,
                        state_flag = $"state_name_{type.Type}",
                        clear_flags = clearFlags.ToString().TrimEnd(Environment.NewLine.ToCharArray()),
                        state_name = type.StateName,
                        province_names = provinceNames.ToString().TrimEnd(Environment.NewLine.ToCharArray())
                    });
                    export.AppendLine(code);
                }
                export.AppendLine("}");
                File.WriteAllText(Path.Combine(scriptedEffects, $"{types.FirstOrDefault().Type}_name.txt"), export.ToString());
            }
        }

        #endregion Methods
    }
}