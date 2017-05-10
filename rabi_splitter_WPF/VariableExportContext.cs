using rabi_splitter_WPF.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace rabi_splitter_WPF
{
    public class VariableExportContext : INotifyPropertyChanged
    {
        private List<VariableExportSetting> _variableExportSettings;
        private HashSet<VariableExportSetting> pendingUpdates;

        public VariableExportContext()
        {
            _variableExportSettings = new List<VariableExportSetting>();
            pendingUpdates = new HashSet<VariableExportSetting>();
        }

        #region Update Logic

        public void OutputUpdates()
        {
            foreach (var ves in pendingUpdates)
            {
                ves.OutputUpdate();
            }
            pendingUpdates.Clear();
        }

        private void RegisterUpdate(VariableExportSetting ves)
        {
            pendingUpdates.Add(ves);
        }

        public void CheckForUpdates()
        {
            foreach (var ves in _variableExportSettings)
            {
                bool hasUpdate = ves.CheckForUpdate();
                if (hasUpdate) RegisterUpdate(ves);
            }
        }

        public void NotifyExportableVariableUpdate()
        {
            foreach (var ves in _variableExportSettings)
            {
                ves.NotifyExportableVariableUpdate();
            }
        }
        #endregion

        #region Variables
        public List<VariableExportSetting> VariableExportSettings
        {
            get { return _variableExportSettings; }
        }

        internal void Add(VariableExportSetting ves)
        {
            _variableExportSettings.Add(ves);
        }

        internal void Delete(VariableExportSetting ves)
        {
            _variableExportSettings.Remove(ves);
            pendingUpdates.Remove(ves);
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
