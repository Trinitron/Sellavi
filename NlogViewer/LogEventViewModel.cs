using System;
using System.Windows;
using System.Windows.Media;
using NLog;

namespace NlogViewer
{
    public class LogEventViewModel
    {
        private LogEventInfo logEventInfo;

        public LogEventViewModel(LogEventInfo logEventInfo)
        {
            // TODO: Complete member initialization
            this.logEventInfo = logEventInfo;

            ToolTip = logEventInfo.FormattedMessage;
            Level = logEventInfo.Level.ToString();
            FormattedMessage = logEventInfo.FormattedMessage;
            Exception = logEventInfo.Exception;
            LoggerName = logEventInfo.LoggerName;

            SetupColors(logEventInfo);
        }

        public string ExceptionDetails => Exception != null ? Exception.ToString() : null;

        public Visibility ContainsException
        {
            get
            {
                if (ExceptionDetails == null)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }


        public string LoggerName { get; }
        public string Level { get; }
        public string FormattedMessage { get; }
        public Exception Exception { get; }
        public string ToolTip { get; }
        public SolidColorBrush Background { get; private set; }
        public SolidColorBrush Foreground { get; private set; }
        public SolidColorBrush BackgroundMouseOver { get; private set; }
        public SolidColorBrush ForegroundMouseOver { get; private set; }

        private void SetupColors(LogEventInfo logEventInfo)
        {
            if (logEventInfo.Level == LogLevel.Warn)
            {
                Background = Brushes.Yellow;
                BackgroundMouseOver = Brushes.GreenYellow;
            }
            else if (logEventInfo.Level == LogLevel.Error)
            {
                Background = Brushes.Tomato;
                BackgroundMouseOver = Brushes.IndianRed;
            }
            else
            {
                Background = Brushes.White;
                BackgroundMouseOver = Brushes.LightGray;
            }

            Foreground = Brushes.Black;
            ForegroundMouseOver = Brushes.Black;
        }
    }
}