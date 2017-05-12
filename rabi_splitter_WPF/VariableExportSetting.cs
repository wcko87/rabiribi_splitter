using rabi_splitter_WPF.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using StringInject;

namespace rabi_splitter_WPF
{
    public class ExportableVariable<T> : ExportableVariable
    {
        private readonly Func<T> tracker;
        
        public ExportableVariable(string handle, string displayName, Func<T> tracker) : base(handle, displayName)
        {
            this.tracker = tracker;
        }

        public override void UpdateValue()
        {
            Value = tracker();
        }
    }
    
    public abstract class ExportableVariable : INotifyPropertyChanged
    {
        private readonly int _id;
        private readonly string _displayName;
        private readonly string _handle;

        private object _value;

        protected ExportableVariable(string handle, string displayName)
        {
            _handle = handle;
            _displayName = displayName;
        }

        public int Id
        {
            get { return _id; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string Handle
        {
            get { return _handle; }
        }

        public object Value
        {
            get { return _value; }
            protected set
            {
                if (value.Equals(_value)) return;
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
        
        public abstract void UpdateValue();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VariableExportSetting : INotifyPropertyChanged
    {

        private string _outputFileName;
        private string _outputFormat;
        private string _formatPreview;
        private bool _isExporting;
        private bool _isPreviewingFormat;
        private bool _hasChangedSinceLastFileOutput;
        
        public VariableExportSetting()
        {
            // Default values
            _outputFileName = "";
            _outputFormat = "";
            _isExporting = false;
            _isPreviewingFormat = false;
            _hasChangedSinceLastFileOutput = true;
        }

        #region Logic
        private string FormatOutput(Dictionary<string, object> variableValues)
        {
            try
            {
                return _outputFormat.Inject(variableValues);
            }
            catch (FormatException e)
            {
                return e.Message;
            }
        }

        internal void UpdateText(Dictionary<string, object> variableValues)
        {
            var formattedOutput = FormatOutput(variableValues);
            if (formattedOutput != FormatPreview)
            {
                FormatPreview = formattedOutput;
                _hasChangedSinceLastFileOutput = true;
            }
        }

        internal void MaybeUpdateFile()
        {
            if (!_hasChangedSinceLastFileOutput || !IsExporting) return;

            System.IO.StreamWriter file = new System.IO.StreamWriter(OutputFileName);
            file.WriteLine(FormatPreview);
            file.Close();

            _hasChangedSinceLastFileOutput = false;
        }

        #endregion
        
        #region Parameters

        public string OutputFileName
        {
            get { return _outputFileName; }
            set
            {
                if (value.Equals(_outputFileName)) return;
                _outputFileName = value;
                OnPropertyChanged(nameof(OutputFileName));
                _hasChangedSinceLastFileOutput = true;
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

        public string FormatPreview
        {
            get { return _formatPreview; }
            private set
            {
                if (value.Equals(_formatPreview)) return;
                _formatPreview = value;
                OnPropertyChanged(nameof(FormatPreview));
            }
        }

        public bool IsPreviewingFormat
        {
            get { return _isPreviewingFormat; }
            set
            {
                if (value.Equals(_isPreviewingFormat)) return;
                _isPreviewingFormat = value;
                OnPropertyChanged(nameof(IsPreviewingFormat));
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
