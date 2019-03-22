using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using NLog;
using NLog.Common;

namespace NlogViewer
{
    /// <summary>
    ///     Interaction logic for NlogViewer.xaml
    /// </summary>
    public partial class NlogViewer : UserControl
    {
        public NlogViewer()
        {
            IsTargetConfigured = false;
            LogEntries = new ObservableCollection<LogEventViewModel>();
            Directory.CreateDirectory("logs");

            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
                foreach (var target in LogManager.Configuration.AllTargets.Where(t => t is NlogViewerTarget)
                    .Cast<NlogViewerTarget>())
                {
                    IsTargetConfigured = true;
                    target.LogReceived += LogReceived;
                }
        }

        public ObservableCollection<LogEventViewModel> LogEntries { get; private set; }
        public bool IsTargetConfigured { get; }

        protected void LogReceived(AsyncLogEventInfo log)
        {
            var vm = new LogEventViewModel(log.LogEvent);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (LogEntries.Count >= 50)
                    LogEntries.RemoveAt(0);

                LogEntries.Add(vm);
            }));
        }

        public void ExportLog()
        {
            var log = new StringBuilder();
            var file = string.Format(@"{0}\logs\{1}.txt", Environment.CurrentDirectory, DateTime.Now.ToFileTime());
            foreach (var lg in LogEntries)
                log.AppendLine(lg.LoggerName + "\t" + lg.Level + "\t" +
                               lg.FormattedMessage +
                               (lg.ExceptionDetails != null ? "\r\n" + lg.ExceptionDetails : string.Empty));
            File.WriteAllText(file, log.ToString());
            Process.Start(file);
        }
    }
}