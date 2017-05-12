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
            _variableExportSettings = new List<VariableExportSetting>();
            _variables = new List<ExportableVariable>();
            variableValues = new Dictionary<string, object>();
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
                ves.OutputUpdate(variableValues, updateFile);
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
