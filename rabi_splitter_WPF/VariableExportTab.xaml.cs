using rabi_splitter_WPF.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for VariableExportTab.xaml
    /// </summary>
    public partial class VariableExportTab : UserControl
    {
        private DebugContext debugContext;
        private VariableExportContext variableExportContext;

        public VariableExportTab()
        {
            InitializeComponent();
        }
        
        public void Initialise(DebugContext debugContext, VariableExportContext variableExportContext)
        {
            this.debugContext = debugContext;
            this.variableExportContext = variableExportContext;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var ves = new VariableExportSetting();
            ves.OutputFileName = "Hello.txt";
            variableExportContext.Add(ves);
            VariableExportListBox.Items.Refresh();
        }
    }
}
