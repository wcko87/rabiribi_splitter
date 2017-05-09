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

        public VariableExportContext()
        {
            //_variableExportSettings = new List<VariableExportSetting>();
            _variableExportSettings =
                new List<VariableExportSetting>
                {
                    new VariableExportSetting() {OutputFileName = "Test.txt" }
                };
        }

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
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
