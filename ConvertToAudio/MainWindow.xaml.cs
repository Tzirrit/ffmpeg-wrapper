﻿using System;
using System.Diagnostics;
using System.IO;
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
        const string TEMP_PATH = @"\temp";

        string tempPath;
        bool VerboseLoggingEnabled = true;
        bool CleanUpEnabled = false;

        public MainWindow()
        {
            InitializeComponent();

            tb_Log.IsReadOnly = true;
            tb_Log.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            btn_Convert.IsEnabled = false;

            // Create temp conversion folder if not existing
            tempPath = Directory.GetCurrentDirectory() + TEMP_PATH;
            Directory.CreateDirectory(tempPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appPath"></param>
        /// <param name="args"></param>
        void CallConsoleApp(string appPath, string[] args = null)
        {
            string command = " " + string.Join(" ", args); //" -h"; //common help flag for console apps

            if (VerboseLoggingEnabled)
                WriteLog(command);

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
            if (VerboseLoggingEnabled)
                WriteLog("Conversion Done. Process exited.");
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
                // Copy file to temp directory
                string sourceFileName = Path.GetFileName(tb_SourceFile.Text);
                string sourceDirectory = Path.GetDirectoryName(tb_SourceFile.Text);
                string tempIn = string.Format(@"{0}\{1}", tempPath, sourceFileName);
                File.Copy(tb_SourceFile.Text, tempIn, true);

                string destinationFileName = Path.GetFileNameWithoutExtension(tempIn);
                string tempOut = string.Format(@"{0}\{1}.wav", tempPath, destinationFileName);

                string startTime = "0"; // DEBUG VALUE
                string duration = "15"; // DEBUG VALUE

                // Build the command parameters
                string[] args = new string[7];
                args[0] = string.Format("-i \"{0}\"", tempIn);
                args[1] = string.Format("-ss {0}", startTime);
                args[2] = string.Format("-t {0}", duration);
                args[3] = "-ac 1";
                args[4] = "-ar 11025";
                args[5] = "-y";                                 // Force override
                args[6] = string.Format("\"{0}\"", tempOut);


                // Call ffmpeg to convert file
                CallConsoleApp(fullAppPath, args);

                // Copy converted file back to origin (append "(x)" if already existing)
                string destinationPath = string.Format(@"{0}\{1}.wav", sourceDirectory, destinationFileName);

                int suffix = 1;
                while (File.Exists(destinationPath))
                {
                    destinationPath = string.Format(@"{0}\{1}({2}).wav", sourceDirectory, destinationFileName, suffix);
                    suffix++;
                }

                File.Copy(tempOut, destinationPath);
                WriteLog(string.Format("Saved converted file to '{0}'.", destinationPath));

                // Delete temp files if enabled
                if (CleanUpEnabled)
                {
                    // TempIn
                    if (VerboseLoggingEnabled)
                        WriteLog(string.Format("Deleting temporary input file '{0}'...", tempIn));

                    try
                    {
                        File.Delete(tempIn);
                    }
                    catch (Exception ex)
                    {
                        WriteLog(string.Format("Could not delete temporary input file '{0}': {1}", tempIn, ex.Message));
                    }

                    // TempOut
                    if (VerboseLoggingEnabled)
                        WriteLog(string.Format("Deleting temporary output file '{0}'...", tempOut));

                    try
                    {
                        File.Delete(tempOut);
                    }
                    catch (Exception ex)
                    {
                        WriteLog(string.Format("Could not delete temporary output file '{0}': {1}", tempOut, ex.Message));
                    }
                }

            }
            else
            {
                WriteLog(string.Format("Could not find '{0}'", tb_SourceFile.Text));
            }
        }

        /// <summary>
        /// Browser files on computer and select target source file
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
        /// Appends given string to the log textbox
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
                // ToDo: Add format validation (against list of valid file types).

                tb_SourceFile.ToolTip = tb_SourceFile.Text.Trim();
                btn_Convert.IsEnabled = true;
            }
        }
    }
}
