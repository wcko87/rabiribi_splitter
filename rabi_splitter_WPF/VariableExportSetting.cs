using rabi_splitter_WPF.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    public class ExportableVariable<T> : ExportableVariable
    {
        private readonly Func<T> tracker;
        
        public ExportableVariable(string displayName, Func<T> tracker) : base(displayName)
        {
            this.tracker = tracker;
        }

        internal override VariableTracker GetTracker()
        {
            return new VariableTracker<T>(tracker);
        }
    }
    
    public abstract class ExportableVariable
    {
        private static int nextAvailableId = 0;
        private static List<ExportableVariable> _variableExports;
        private static Dictionary<ExportableVariable, string> _variableCaptions = new Dictionary<ExportableVariable, string>();
        
        private readonly int _id;
        private readonly string _displayName;

        protected ExportableVariable(string displayName)
        {
            _id = nextAvailableId++;
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

        internal abstract VariableTracker GetTracker();
        
        public static void DefineVariableExports(ExportableVariable[] exports)
        {
            _variableExports = exports.ToList();
            _variableCaptions = exports.ToDictionary(ev => ev, ev => ev.DisplayName);
        }

        public static Dictionary<ExportableVariable, string> VariableCaptions
        {
            get { return _variableCaptions; }
        }

        #region Equals, GetHashCode
        public override bool Equals(object obj)
        {
            var otherValue = obj as ExportableVariable;
            if (otherValue == null) return false;
            return _id.Equals(otherValue.Id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        #endregion
    }

    public class VariableTracker<T> : VariableTracker
    {
        private readonly Func<T> tracker;
        private T currentValue;

        public VariableTracker(Func<T> tracker)
        {
            this.tracker = tracker;
            forceUpdate = true;
        }

        public override bool CheckForUpdate()
        {
            T newValue = tracker();

            if (forceUpdate || !newValue.Equals(currentValue))
            {
                currentValue = newValue;
                forceUpdate = false;
                return true;
            }
            return false;
        }

        public override object GetValue()
        {
            return currentValue;
        }
    }

    public abstract class VariableTracker
    {
        protected bool forceUpdate;

        public void FormatChanged()
        {
            forceUpdate = true;
        }

        public abstract bool CheckForUpdate();
        public abstract object GetValue();
    }

    public class VariableExportSetting : INotifyPropertyChanged
    {
        private ExportableVariable _selectedVariable;
        private VariableTracker _variableTracker;
        private string _outputFileName;
        private string _outputFormat;
        private string _formatPreview;
        private bool _isExporting;
        private bool _isPreviewingFormat;
        
        public VariableExportSetting()
        {
            // Default values
            _selectedVariable = null;
            _outputFileName = "";
            _outputFormat = "";
            _isExporting = false;
            _isPreviewingFormat = false;
        }

        #region Logic
        private string FormatOutput()
        {
            try
            {
                return string.Format(_outputFormat, _variableTracker.GetValue());
            }
            catch (FormatException e)
            {
                return e.Message;
            }
        }

        internal void OutputUpdate()
        {
            if (_variableTracker == null) return;
            var formattedOutput = FormatOutput();
            FormatPreview = formattedOutput;
            // TODO: Write to file
        }

        internal bool CheckForUpdate()
        {
            if (_variableTracker == null) return false;
            return _variableTracker.CheckForUpdate();
        }

        public void NotifyExportableVariableUpdate()
        {
            OnPropertyChanged(nameof(VariableCaptions));
        }
        #endregion

        #region Dictionaries
        public Dictionary<ExportableVariable, string> VariableCaptions
        {
            get { return ExportableVariable.VariableCaptions; }
        }
        
        internal void DefaultButton_Click()
        {
            if (_selectedVariable == null)
            {
                OutputFormat = "Variable not set.";
            }
            else
            {
                OutputFormat = $"{_selectedVariable.DisplayName}: {{0}}";
            }
        }

        #endregion
        
        #region Parameters

        public ExportableVariable SelectedVariable
        {
            get { return _selectedVariable; }
            set
            {
                if (value.Equals(_selectedVariable)) return;
                _selectedVariable = value;
                _variableTracker = _selectedVariable.GetTracker();
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
                if (_variableTracker != null) _variableTracker.FormatChanged();
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

        // Note: DO NOT OVERRIDE Equals and GetHashCode. We compare by reference equality.

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
