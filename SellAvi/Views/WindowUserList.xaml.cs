using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace SellAvi.Views
{
    /// <summary>
    ///     Interaction logic for WindowUserList.xaml
    /// </summary>
    public partial class WindowUserList : Window
    {
        public WindowUserList()
        {
            InitializeComponent();

            Messenger.Default.Register<NotificationMessage>(this, nm =>
            {
                if (nm.Notification == "CloseWindowUserList")
                    if (nm.Sender == DataContext)
                        Close();
            });
        }
    }
}