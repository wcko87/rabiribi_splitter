using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace rabi_splitter_WPF
{
    /// <summary>
    /// Interaction logic for ExportPanel.xaml
    /// </summary>
    public partial class ExportPanel : UserControl
    {
        public object VariableExportObject
        {
            get { return (object)GetValue(VariableExportObjectProperty); }
            set { SetValue(VariableExportObjectProperty, value); }

        }

        public static readonly DependencyProperty VariableExportObjectProperty =
            DependencyProperty.Register("VariableExportObject", typeof(object),
              typeof(VariableExportSetting), new PropertyMetadata(null));

        public ExportPanel()
        {
            InitializeComponent();
            this.MainPanel.DataContext = this;
        }
    }
}
