using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace rabi_splitter_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainContext mainContext;
        private RabiRibiDisplay rabiRibiDisplay;
        private DebugContext debugContext;
        private static TcpClient tcpclient;
        private static NetworkStream networkStream;
        private readonly Regex titleReg = new Regex(@"ver.*?(\d+\.?\d+.*)$");
        private readonly Thread memoryThread;
        
        private void ReadMemory()
        {
            var processlist = Process.GetProcessesByName("rabiribi");
            if (processlist.Length > 0)
            {
                Process process = processlist[0];
                if (process.MainWindowTitle != mainContext.oldtitle)
                {
                    var result = titleReg.Match(process.MainWindowTitle);
                    string rabiver;
                    if (result.Success)
                    {
                        rabiver = result.Groups[1].Value;
                        mainContext.veridx = Array.IndexOf(StaticData.VerNames, rabiver);
                        if (mainContext.veridx < 0)
                        {
                            mainContext.GameVer = rabiver + " Running (not supported)";
                            return;
                        }
                    }
                    else
                    {
                        mainContext.veridx = -1;
                        mainContext.GameVer = "Running (Unknown version)";
                        return;
                    }
                    mainContext.GameVer = rabiver + " Running";
                    mainContext.oldtitle = process.MainWindowTitle;
                }
                
                if (mainContext.veridx < 0) return;

                rabiRibiDisplay.ReadMemory(process);
            }
            else
            {
                mainContext.oldtitle = "";
                mainContext.GameVer = "Not Found";
                mainContext.GameMusic = "N/A";
            }
        }
        
        public void SendMessage(string message)
        {
            if (tcpclient != null && tcpclient.Connected)
            {
                try
                {
                    var b = Encoding.UTF8.GetBytes(message);
                    networkStream.Write(b, 0, b.Length);
                }
                catch (Exception)
                {

                    disconnect();
                }
            }
        }

        void disconnect()
        {
            tcpclient = null;
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                BtnConnect.IsEnabled = true;
            }));
        }

        private void DebugLog(string log)
        {
            this.debugContext.Log(log);
        }

        public MainWindow()
        {
            InitializeComponent();
            mainContext=new MainContext();
            debugContext=new DebugContext();
            this.DataContext = mainContext;
            DebugPanel.DataContext = debugContext;
            this.Grid.ItemsSource = debugContext.BossList;
            EntityDataPanel.DataContext = debugContext;
            this.EntityStats.ItemsSource = debugContext.EntityStatsListView;
            BossEventDebug.DataContext = debugContext;
            rabiRibiDisplay = new RabiRibiDisplay(mainContext, debugContext, this);
            memoryThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        ReadMemory();
                    }
                    catch (Exception e)
                    {
                        DebugLog(e.ToString());
                    }
                   
                    Thread.Sleep(1000 / 60);
                }

            });
            memoryThread.IsBackground = true;
            memoryThread.Start();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (tcpclient != null && tcpclient.Connected) return;
            try
            {
                tcpclient = new TcpClient("127.0.0.1", Convert.ToInt32(mainContext.ServerPort));
                networkStream = tcpclient.GetStream();
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    BtnConnect.IsEnabled = false;
                }));
            }
            catch (Exception)
            {
                tcpclient = null;
                networkStream = null;
                MessageBox.Show(this, "Connect Failed");

            }
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/copyliu/rabiribi_splitter");
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = sender as TextBox;
            if (s != null)
            {
                s.ScrollToEnd();
            }
        }
    }
}
