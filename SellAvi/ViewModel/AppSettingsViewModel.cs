using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using SellAvi.Browser;
using SellAvi.Properties;

namespace SellAvi.ViewModel
{
    public class AppSettingsViewModel : ViewModelBase
    {
        public AppSettingsViewModel()
        {
            SaveUserSettingsCommand = new RelayCommand<Window>(w =>
            {
                Settings.Default.Save();
                w.Close();
            });
            RestoreUserSettingsCommand = new RelayCommand<Window>(w =>
            {
                Settings.Default.Reset();
                w.Close();
            });
            NavigateToCommand = new RelayCommand<string>(link => IEHelpers.OpenWebBrowser(link));
            UserAgentChangeCommand = new RelayCommand<ComboBoxItem>(i => { UserAgentSubstitution(i); });
        }

        public static RelayCommand<Window> SaveUserSettingsCommand { get; set; }
        public static RelayCommand<Window> RestoreUserSettingsCommand { get; set; }
        public static RelayCommand<string> NavigateToCommand { get; set; }
        public static RelayCommand<int> ParserPhoneModeCommand { get; set; }
        public static RelayCommand<ComboBoxItem> UserAgentChangeCommand { get; set; }
        public string CaptchaService { get; set; }

        public bool CaptchaIsAutomatic
        {
            get => Settings.Default.CaptchaIsAutomatic;
            set => Settings.Default.CaptchaIsAutomatic = value;
        }

        public string CaptchaToken
        {
            get => Settings.Default.CaptchaToken;
            set => Settings.Default.CaptchaToken = value.Trim();
        }

        public int CaptchaTimeout
        {
            get => Settings.Default.CaptchaTimeOut;
            set => Settings.Default.CaptchaTimeOut = value;
        }

        public int CaptchaTrials
        {
            get => Settings.Default.CaptchaTrials;
            set => Settings.Default.CaptchaTrials = value;
        }

        public int PublicationTimeout
        {
            get => Settings.Default.PublicationTimeOut;
            set => Settings.Default.PublicationTimeOut = value;
        }

        public int PublicationPhotoLimit
        {
            get => Settings.Default.PublicationPhotoLimit;
            set => Settings.Default.PublicationPhotoLimit = value;
        }

        public int PublicationDescLimit
        {
            get => Settings.Default.PublicationDescLimit;
            set => Settings.Default.PublicationDescLimit = value;
        }

        public bool PublicationSkipErrors
        {
            get => Settings.Default.PublicationSkipErrors;
            set => Settings.Default.PublicationSkipErrors = value;
        }

        public bool PublicationCommonUserRegion
        {
            get => Settings.Default.PublicationCommonUserRegion;
            set => Settings.Default.PublicationCommonUserRegion = value;
        }

        public int ParserPolling
        {
            get => Settings.Default.ParserPolling;
            set => Settings.Default.ParserPolling = value;
        }

        public int ParserPagesNum
        {
            get => Settings.Default.ParserPagesNum;
            set => Settings.Default.ParserPagesNum = value;
        }

        public bool ParserTrimHtml
        {
            get => Settings.Default.ParserTrimHtml;
            set => Settings.Default.ParserTrimHtml = value;
        }

        public bool ParserDownloadImages
        {
            get => Settings.Default.ParserDownloadImages;
            set => Settings.Default.ParserDownloadImages = value;
        }

        public bool ParserShowErrors
        {
            get => Settings.Default.ParserShowErrors;
            set => Settings.Default.ParserShowErrors = value;
        }

        public bool ParserAllowDuplicates
        {
            get => Settings.Default.ParserAllowDuplicates;
            set => Settings.Default.ParserAllowDuplicates = value;
        }

        public bool ParserDownloadPhones
        {
            get => Settings.Default.ParserDownloadPhones;
            set => Settings.Default.ParserDownloadPhones = value;
        }

        public bool ParserTrimPrice
        {
            get => Settings.Default.ParserTrimPrice;
            set => Settings.Default.ParserTrimPrice = value;
        }


        public string WebClientAgent
        {
            get => Settings.Default.WebClientAgent;
            set => Settings.Default.WebClientAgent = value;
        }

        public int WebClientTimeOut
        {
            get => Settings.Default.WebClientTimeOut;
            set => Settings.Default.WebClientTimeOut = value;
        }


        public int ActivationPagesNum
        {
            get => Settings.Default.ActivationPagesNum;
            set => Settings.Default.ActivationPagesNum = value;
        }

        public int ActivationTimeOut
        {
            get => Settings.Default.ActivationTimeOut;
            set => Settings.Default.ActivationTimeOut = value;
        }

        public bool ActivationPackage
        {
            get => Settings.Default.ActivationPackage;
            set => Settings.Default.ActivationPackage = value;
        }

        public int MessagePolling
        {
            get => Settings.Default.MessagePolling;
            set => Settings.Default.MessagePolling = value;
        }

        public bool MessageDuplicateCheck
        {
            get => Settings.Default.MessageDuplicateCheck;
            set => Settings.Default.MessageDuplicateCheck = value;
        }

        public int MessageSocketPing
        {
            get => Settings.Default.MessageSocketPing;
            set => Settings.Default.MessageSocketPing = value;
        }

        public string MessageDefaultReply
        {
            get => Settings.Default.MessageDefaultReply;
            set => Settings.Default.MessageDefaultReply = value;
        }

        public int MessageInterval
        {
            get => Settings.Default.MessageInterval;
            set => Settings.Default.MessageInterval = value;
        }

        public string TorExitNodes
        {
            get => Settings.Default.TorExitNodes;
            set => Settings.Default.TorExitNodes = value;
        }

        public bool TorInPriority
        {
            get => Settings.Default.TorInPriority;
            set => Settings.Default.TorInPriority = value;
        }

        public bool TorCompatibility
        {
            get => Settings.Default.TorCompatibility;
            set => Settings.Default.TorCompatibility = value;
        }

        public string ProxyServer
        {
            get => Settings.Default.ProxyServer;
            set => Settings.Default.ProxyServer = value;
        }

        public string ProxyLogin
        {
            get => Settings.Default.ProxyLogin;
            set => Settings.Default.ProxyLogin = value;
        }

        public string ProxyPassword
        {
            get => Settings.Default.ProxyPassword;
            set => Settings.Default.ProxyPassword = value;
        }


        private void MoveIntexToTop(int index, StringCollection collection)
        {
            var selectedValue = collection[index];
            collection.RemoveAt(index);
            collection.Insert(0, selectedValue);
        }

        /// <summary>
        ///     Проверка библиотек Visual C++ Redistributable для работы с Tesseract
        /// </summary>
        public static void CheckIfVisualCInstalled()
        {
            var x64key = Registry.LocalMachine;
            var x32key = Registry.LocalMachine;
            x64key = x64key.OpenSubKey(
                @"SOFTWARE\Classes\Installer\Dependencies\{323dad84-0974-4d90-a1c1-e006c7fdbb7d}", false);
            x32key = x32key.OpenSubKey(
                @"SOFTWARE\Classes\Installer\Dependencies\{462f63a8-6347-4894-a1b3-dbfe3a4c981d}", false);
            if (x32key == null && x64key == null)
            {
                var d =
                    MessageBox.Show(
                        "Для корректной работы (OCR Tesseract) необходимо загрузить библиотеку обработки телефонных номеров Microsoft Visual C++ 2015 Redistributable",
                        "Microsoft Visual C++ 2015 Redistributable", MessageBoxButton.YesNo);
                if (d == MessageBoxResult.Yes)
                    IEHelpers.OpenWebBrowser("https://www.microsoft.com/ru-ru/download/details.aspx?id=48145");
            }
        }

        private void UserAgentSubstitution(ComboBoxItem i)
        {
            switch (i.Name)
            {
                case "cbi1":
                    WebClientAgent =
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
                    break;
                case "cbi2":
                    WebClientAgent = "Mozilla/5.0 (Windows NT 5.1; rv:7.0.1) Gecko/20100101 Firefox/7.0.1";
                    break;
                case "cbi3":
                    WebClientAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:54.0) Gecko/20100101 Firefox/54.0";
                    break;
                case "cbi4":
                    WebClientAgent =
                        "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36";
                    break;
                case "cbi5":
                    WebClientAgent =
                        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.157 Safari/537.36";
                    break;
                case "cbi6":
                    WebClientAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1";
                    break;
                case "cbi7":
                    WebClientAgent = "Mozilla/4.0 (compatible; MSIE 9.0; Windows NT 6.1)";
                    break;
                case "cbi8":
                    WebClientAgent =
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393";
                    break;
                case "cbi9":
                    WebClientAgent =
                        "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
                    break;
                case "cbi10":
                    WebClientAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
                    break;
                case "cbi11":
                    WebClientAgent =
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063";
                    break;
                case "cbi12":
                    WebClientAgent =
                        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_6) AppleWebKit/603.3.8 (KHTML, like Gecko)";
                    break;
            }

            RaisePropertyChanged("WebClientAgent");
        }
    }
}