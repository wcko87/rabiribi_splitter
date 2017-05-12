using rabi_splitter_WPF.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace rabi_splitter_WPF
{
    public class VariableExportContext : INotifyPropertyChanged
    {
        private List<VariableExportSetting> _variableExportSettings;
        private List<ExportableVariable> _variables;
        private Dictionary<string, object> variableValues;

        private ItemCollection variableListBoxItems;
        private ItemCollection variableExportListBoxItems;

        public VariableExportContext()
        {
            _variableExportSettings = DefaultVariableExportSettings();
            _variables = new List<ExportableVariable>();
            variableValues = new Dictionary<string, object>();
        }

        private List<VariableExportSetting> DefaultVariableExportSettings()
        {
            return new List<VariableExportSetting>
            {
                new VariableExportSetting() {
                    OutputFileName = "deaths_restarts.txt",
                    OutputFormat = "Deaths: {deaths}\nRestarts: {restarts}"
                },
                new VariableExportSetting() {
                    OutputFileName = "map.txt",
                    OutputFormat = "Map: {map}\nTile: {mapTile}"
                },
                new VariableExportSetting() {
                    OutputFileName = "music.txt",
                    OutputFormat = "Music: {music}"
                },
                new VariableExportSetting() {
                    OutputFileName = "currentboss.txt",
                    OutputFormat = "Current Boss: {currentBoss}\nTime: {currentBossTime:mm\\:ss\\.ff}"
                },
                new VariableExportSetting() {
                    OutputFileName = "lastboss.txt",
                    OutputFormat = "Last Boss: {lastBoss}\nTime: {lastBossTime:mm\\:ss\\.ff}"
                },
                new VariableExportSetting() {
                    OutputFileName = "hammer.txt",
                    OutputFormat = "Hammer: {hammerXp}/{nextHammerExp}\nNext: {nextHammerNameLong}"
                },
                new VariableExportSetting() {
                    OutputFileName = "ribbon.txt",
                    OutputFormat = "Ribbon: {ribbonXp}/{nextRibbonExp}\nNext: {nextRibbonNameLong}"
                },
                new VariableExportSetting() {
                    OutputFileName = "carrot.txt",
                    OutputFormat = "Carrot: {carrotXp}/{nextCarrotExp}\nNext: {nextCarrotNameLong}"
                },
            };
        }

        #region Update Logic

        public void UpdateVariables(bool updateFile)
        {
            foreach (var variable in _variables) {
                variable.UpdateValue();
                variableValues[variable.Handle] = variable.Value;
            }

            foreach (var ves in _variableExportSettings)
            {
                ves.UpdateText(variableValues);
                if (updateFile) ves.MaybeUpdateFile();
            }
        }

        internal void SetItemControls(ItemCollection variableListBoxItems, ItemCollection variableExportListBoxItems)
        {
            this.variableListBoxItems = variableListBoxItems;
            this.variableExportListBoxItems = variableExportListBoxItems;
        }

        public void DefineVariableExports(ExportableVariable[] exports)
        {
            Variables = exports.ToList();
            variableValues.Clear();
        }
        #endregion

        #region Properties
        public List<ExportableVariable> Variables
        {
            get { return _variables; }
            private set
            {
                _variables = value;
                variableListBoxItems.Refresh();
            }
        }

        public List<VariableExportSetting> VariableExportSettings
        {
            get { return _variableExportSettings; }
        }

        internal void Add(VariableExportSetting ves)
        {
            _variableExportSettings.Add(ves);
            variableExportListBoxItems.Refresh();
        }

        internal void Delete(VariableExportSetting ves)
        {
            _variableExportSettings.Remove(ves);
            variableExportListBoxItems.Refresh();
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
