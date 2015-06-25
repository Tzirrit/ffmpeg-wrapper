using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace ConvertToAudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string APP_PATH = @"External\ffmpeg.exe";

        public MainWindow()
        {
            InitializeComponent();

            tb_Log.IsReadOnly = true;
            tb_Log.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            btn_Convert.IsEnabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appPath"></param>
        /// <param name="args"></param>
        void CallConsoleApp(string appPath, string[] args = null)
        {
            WriteLog(string.Format("Starting '{0}'", appPath));

            string command = " -h"; //common help flag for console apps

            // Create process
            Process process = new Process();
            process.StartInfo.FileName = appPath;
            process.StartInfo.Arguments = command;

            // Set UseShellExecute to false to allow redirection
            process.StartInfo.UseShellExecute = false;

            // Redirect the standard output and error
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            // Start the process
            process.Start();

            // Read output
            if (process.StartInfo.RedirectStandardOutput)
            {
                string output = process.StandardOutput.ReadToEnd();
                WriteLog(output);
            }
            if (process.StartInfo.RedirectStandardOutput)
            {
                string error = process.StandardError.ReadToEnd();
                WriteLog(error);
            }

            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited);

            process.WaitForExit();
            //process.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void process_Exited(object sender, EventArgs e)
        {
            WriteLog("Exited");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_Convert_Click(object sender, RoutedEventArgs e)
        {
            string fullAppPath = System.IO.Path.GetFullPath(APP_PATH);

            if (File.Exists(tb_SourceFile.Text.Trim()))
            {
                CallConsoleApp(fullAppPath);
            }
            else
            {
                WriteLog(string.Format("Could not find '{0}'", tb_SourceFile.Text));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btn_BrowseFiles_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    tb_SourceFile.Text = file;
                    tb_SourceFile.ToolTip = file;
                    btn_Convert.IsEnabled = true;
                    break;

                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    tb_SourceFile.Text = null;
                    tb_SourceFile.ToolTip = null;
                    btn_Convert.IsEnabled = false;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="linebreak"></param>
        void WriteLog(string entry, bool linebreak = true)
        {
            string formatedEntry = linebreak ? string.Format("{0}\n", entry) : entry;

            // Check if invoking is neccessary
            if (!tb_Log.Dispatcher.CheckAccess())
            {
                tb_Log.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate()
                    {
                        tb_Log.AppendText(formatedEntry);
                        tb_Log.ScrollToEnd();
                    }
                ));
            }
            // If not, just write to the log
            else
            {
                tb_Log.AppendText(formatedEntry);
                tb_Log.ScrollToEnd();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tb_SourceFile_LostFocus(object sender, RoutedEventArgs e)
        {
            btn_Convert.IsEnabled = false;

            if (File.Exists(tb_SourceFile.Text.Trim()))
            {
                tb_SourceFile.ToolTip = tb_SourceFile.Text.Trim();
                btn_Convert.IsEnabled = true;
            }
        }
    }
}
