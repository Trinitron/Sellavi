using System.Windows;

namespace SellAvi.Views
{
    /// <summary>
    ///     Interaction logic for WindowDebug.xaml
    /// </summary>
    public partial class WindowDebug : Window
    {
        public WindowDebug()
        {
            InitializeComponent();
            window.Loaded += Window_Loaded;
        }

        private void bntClose_OnClick(object sender, RoutedEventArgs e)
        {
            window.Close();
        }

        private void btnSave_OnClick(object sender, RoutedEventArgs e)
        {
            logCtrl.ExportLog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = 250;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
        }
    }
}