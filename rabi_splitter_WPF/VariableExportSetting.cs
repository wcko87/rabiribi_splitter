using rabi_splitter_WPF.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    class ExportableVariable
    {


        public List<ExportableVariable> GetAll()
        {
            return new List<ExportableVariable>();
        }
    }

    class VariableExportSetting : INotifyPropertyChanged
    {
        private ExportableVariable _selectedVariable;
        private string _outputFileName;
        private string _outputFormat;
        private bool _isExporting;

        #region Dictionaries 
        
        // Captions for Split Trigger Options
        private static readonly Dictionary<ExportableVariable, string> _variableCaptions = new Dictionary<ExportableVariable, string>()
        {
        };
        
        public Dictionary<ExportableVariable, string> VariableCaptions
        {
            get {return _variableCaptions;}
        }
        
        #endregion
        
        public VariableExportSetting()
        {
            // Default values
            _selectedVariable = null;
            _outputFileName = "";
            _outputFormat = "";
            _isExporting = false;
        }

        #region Parameters

        public ExportableVariable SelectedVariable
        {
            get { return _selectedVariable; }
            set
            {
                if (value.Equals(_selectedVariable)) return;
                _selectedVariable = value;
                OnPropertyChanged(nameof(SelectedVariable));
            }
        }

        public string OutputFileName
        {
            get { return _outputFileName; }
            set
            {
                if (value.Equals(_outputFileName)) return;
                _outputFileName = value;
                OnPropertyChanged(nameof(OutputFileName));
            }
        }

        public string OutputFormat
        {
            get { return _outputFormat; }
            set
            {
                if (value.Equals(_outputFormat)) return;
                _outputFormat = value;
                OnPropertyChanged(nameof(OutputFormat));
            }
        }

        public bool IsExporting
        {
            get { return _isExporting; }
            set
            {
                if (value.Equals(_isExporting)) return;
                _isExporting = value;
                OnPropertyChanged(nameof(IsExporting));
            }
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
