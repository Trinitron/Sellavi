using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using SellAvi.Browser;
using SellAvi.Classes;
using SellAvi.Model.DataBaseModels;
using SellAvi.Properties;
using SellAvi.Services;
using SellAvi.Views;

namespace SellAvi.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IDataAccessService _serviceProxy;
        private Authenticator _authenticator;
        private RequestHandler _requestHandler;

        public RelayCommand<CancelEventArgs> WindowClosing { get; set; }
        public RelayCommand AuthenticateCommand { get; set; }
        public RelayCommand CallUserListCommand { get; set; }
        public RelayCommand CallProfileNavigateCommand { get; set; }
        public RelayCommand TabChangedCommand { get; set; }
        public RelayCommand CallProfileRegisterCommand { get; set; }
        public RelayCommand CallAvitoTechnicalCommand { get; set; }
        public RelayCommand CallGetProxyCommand { get; set; }
        public RelayCommand CallHidePanelCommand { get; set; }
        public RelayCommand CallAddProductCommand { get; set; }
        public RelayCommand CallAddNewProductCommand { get; set; }
        public RelayCommand CallImportProductCommand { get; set; }
        public RelayCommand CallViewProductsCommand { get; set; }
        public RelayCommand CallSendMessagesCommand { get; set; }
        public RelayCommand CallViewPhotosCommand { get; set; }
        public RelayCommand CallLicenseInfoCommand { get; set; }
        public RelayCommand CallStartPageCommand { get; set; }
        public RelayCommand CallDebugInfoCommand { get; set; }
        public RelayCommand CallAppSettingsCommand { get; set; }
        public RelayCommand CallCustomParamsCommand { get; set; }
        public RelayCommand CallBrowserDevToolsCommand { get; set; }
        public RelayCommand CallReloadBrowserCommand { get; set; }
        public RelayCommand CallBrowserSetZoomCommand { get; set; }
        public RelayCommand CallActivateTorCommand { get; set; }
        public RelayCommand CallCheckIpCommand { get; set; }
        public RelayCommand CallNewTorIdentityCommand { get; set; }

        //TODO: вынести в отдельную viewmodel
        public RelayCommand<AvitoUser> LogInAsUserCommand { get; set; }
        public RelayCommand<AvitoUser> RefreshUserInformationCommand { get; set; }
        public RelayCommand<AvitoUser> DropCredentialsCommand { get; set; }
        public RelayCommand<AvitoUser> DeleteUserCommand { get; set; }
        public RelayCommand<AvitoUser> SetDefaultPhoneCommand { get; set; }



        public WebClientEx BrowserClientEx { get; set; }
        public CefSharpWrapper ChBrowser { get; set; }
        public ObservableCollection<AvitoUser> AvitoUsers { get; set; }

        private AvitoUser _currentDetachedUser;
        public AvitoUser CurrentDetachedUser
        {
            get { return _currentDetachedUser; }
            set
            {
                _currentDetachedUser = (AvitoUser)value;
                RaisePropertyChanged("CurrentDetachedUser");
            }
        }

        private bool _isAuthorized;
        public bool IsAuthorized
        {
            get { return _isAuthorized; }
            set
            {
                _isAuthorized = value;
                RaisePropertyChanged("IsAuthorized");
            }
        }

        private string _currentIp;
        public string CurrentIp
        {
            get { return _currentIp; }
            set
            {
                _currentIp = value;
                RaisePropertyChanged("CurrentIP");
            }
        }

        private Visibility _menuVisibility = Visibility.Visible;
        public Visibility MenuVisibility
        {
            get { return _menuVisibility; }
            set
            {
                _menuVisibility = value;
                RaisePropertyChanged("MenuVisibility");
            }
        }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataAccessService servPxy)
        {
            try
            {
                _serviceProxy = servPxy;
                Database.SetInitializer(new AvitoModelInitializer());
                //ID приложения: 2355847


                //Создание браузерной группы
                BrowserClientEx = new WebClientEx();
                BrowserClientEx.Encoding = new UTF8Encoding();
                BrowserClientEx.DefaultHeaders[HttpRequestHeader.UserAgent] = Settings.Default.WebClientAgent;
                BrowserClientEx.RequestTimeOut = Settings.Default.WebClientTimeOut * 1000;

                //Настройки CefSharp
                CefSharp.Wpf.CefSettings cfsettings = new CefSharp.Wpf.CefSettings
                {
                    CachePath = Environment.CurrentDirectory + @"\browserCache",
                    UserAgent = Settings.Default.WebClientAgent,
                };

                Cef.Initialize(cfsettings);
                ChBrowser = new CefSharpWrapper();
                _requestHandler = new RequestHandler();
                ChBrowser.RequestHandler = _requestHandler;

                BrowserClientEx.CHW = ChBrowser;



                AvitoUsers = servPxy.GetUsers;
                CurrentDetachedUser = (AvitoUsers.LastOrDefault() != null)
                    ? _serviceProxy.GetDetachedEntity(AvitoUsers.LastOrDefault())
                    : new AvitoUser();
                
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show(exception.Message, "Ошибка инициализации приложения", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // WINDOW COMMANDS
            WindowClosing = new RelayCommand<CancelEventArgs>((s) =>
            {
                Cef.Shutdown();
            });
            AuthenticateCommand = new RelayCommand(DoAvitoLogin);
            CallUserListCommand = new RelayCommand(OpenUserListWindow);
            CallProfileNavigateCommand = new RelayCommand(() => { ChBrowser.Address = "https://www.avito.ru/profile"; });
            CallProfileRegisterCommand = new RelayCommand(AddNewProfile);
            CallAvitoTechnicalCommand = new RelayCommand(() => { ChBrowser.Address = "https://www.avito.ru/info/show_technical"; });
            CallHidePanelCommand = new RelayCommand(HideLeftPanel);


            CallViewPhotosCommand = new RelayCommand(() => { Process.Start(Environment.CurrentDirectory + @"\images"); });
            CallLicenseInfoCommand = new RelayCommand(OpenLicenseInfo);
            CallDebugInfoCommand = new RelayCommand(OpenDebugWindow);
            CallReloadBrowserCommand = new RelayCommand(() => CefSharp.WebBrowserExtensions.Reload(ChBrowser, true));


            //TODO: вынести в отдельную viewmodel
            LogInAsUserCommand = new RelayCommand<AvitoUser>(LogInAsUser);
            RefreshUserInformationCommand = new RelayCommand<AvitoUser>(RefreshUserInformation);
            DropCredentialsCommand = new RelayCommand<AvitoUser>(DropCredentials);
            DeleteUserCommand = new RelayCommand<AvitoUser>(DeleteUser);
            SetDefaultPhoneCommand = new RelayCommand<AvitoUser>(SetDefaultPhone);


        }


        private void OpenUserListWindow()
        {
            new WindowUserList().Show();
        }
        private void OpenDebugWindow()
        {
            new WindowDebug().Show();
        }

        private void OpenLicenseInfo()
        {
            var siteResponse = BrowserClientEx.DownloadString(Authenticator.GetLicenseQuery(CurrentDetachedUser, 1));
            MessageBox.Show(siteResponse, "Информация о лицензии", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DoAvitoLoginAsync()
        {
            Task.Factory.StartNew(_ => DoAvitoLogin(), new CancellationTokenSource()).ContinueWith(_ => { });
        }

        private void DoAvitoLogin()
        {
            Logger.Trace("Авторизация пользователя {0}", CurrentDetachedUser.UserName);
            _authenticator = new Authenticator(CurrentDetachedUser) { Client = BrowserClientEx };
            BrowserClientEx.CookieContainer = new CookieContainer();
            var cookieAuth = (AttachedCurrentUser != null && AttachedCurrentUser.UserCookie != null);

            if (_authenticator.HasCredentials() && _authenticator.Authenticate(cookieAuth) && _authenticator.CheckAuthorization())
            {
                Logger.Info("Этап авторизации успешно пройден");
                IsAuthorized = true;

                //Сохраняем cookie с правильной авторизацией
                CurrentDetachedUser.UserCookie = BrowserClientEx.CookieContainer.GetCookieHeader(new Uri("https://www.avito.ru"));

                MessageBox.Show("Поздравляем, вы справились с заданием!", "Награда 100$");

                ChBrowser.SetUserAuthorizationCookies(CurrentDetachedUser.UserCookie);
                Messenger.Default.Send<WebClientEx>(BrowserClientEx);
                Messenger.Default.Send<AvitoUser>(AttachedCurrentUser);
            }
            else
            {
                // Ошибка авторизации может произойти из-за устаревших cookies
                // Даем пользователю повторно ввести пароль от учетной записи
                if (AttachedCurrentUser!=null)
                {
                    CurrentDetachedUser.UserCookie = AttachedCurrentUser.UserCookie = null;
                    CurrentDetachedUser.UserPassword = AttachedCurrentUser.UserPassword = null;
                    RaisePropertyChanged("CurrentDetachedUser");
                    _serviceProxy.SaveContext();
                }

                ChBrowser.Address = "https://www.avito.ru/profile/exit";
                Logger.Warn("Ошибка авторизации пользователя {0} (пароль: {1})", CurrentDetachedUser.UserName, CurrentDetachedUser.UserPassword);
                MessageBox.Show("Авторизоваться не удалось!", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

        }

        public AvitoUser AttachedCurrentUser
        {
            get
            {
                return AvitoUsers.FirstOrDefault((u => u.UserName == CurrentDetachedUser.UserName));
            }
        }


        private void AddNewProfile()
        {
            CurrentDetachedUser = new AvitoUser();
            IsAuthorized = false;
        }

        private void HideLeftPanel()
        {
            MenuVisibility = (MenuVisibility == Visibility.Visible)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }






        #region UserListCommands
        private void DeleteUser(AvitoUser user)
        {
            var d =
                MessageBox.Show(
                    "Удаление пользователя приведет к очистке всех его объявлений из программы, продолжить?",
                    "Удаление пользователя", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (d == MessageBoxResult.Yes)
            {
                _serviceProxy.DeleteUser(user);
                AvitoUsers.Remove(user);

            }
        }

        private void LogInAsUser(AvitoUser user)
        {
            CurrentDetachedUser = _serviceProxy.GetDetachedEntity(user);
            DoAvitoLogin();
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage(this, "CloseWindowUserList"));
        }

        private void SetDefaultPhone(AvitoUser user)
        {
            if (user != null)
            {
                _serviceProxy.SaveContext();
                MessageBox.Show(String.Format("Для публикации объявлений на сайте будет использован телефонный номер: {0}",
                    user.SplitedCompanyPhones.FirstOrDefault()), "Изменения номера для публикации", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

        }

        private void RefreshUserInformation(AvitoUser user)
        {
            if (IsAuthorized && user.UserName == CurrentDetachedUser.UserName)
            {
                _authenticator.UpdateUserInformation(user);
                _serviceProxy.SaveContext();
                MessageBox.Show("Профиль пользователя обновлен, откройте окно заново!", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Для обновления информации о пользователе необходимо быть авторизованным в системе",
                    "Ошибка обновления", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }
        private void DropCredentials(AvitoUser user)
        {
            //Разлогиниваемся в браузере
            BrowserClientEx.DownloadStringWithNotification(new Uri("https://www.avito.ru/profile/exit"));

            //Удаление пароля и кук у Attached Entity для указаного user 
            user.UserCookie = null;
            user.UserPassword = null;
            _serviceProxy.SaveContext();

            //Получение обновленой инфы по всем пользователям включая user (окно управления пользователями)
            AvitoUsers = _serviceProxy.GetUsers;
            RaisePropertyChanged("AvitoUsers");

            //Текущему пользователю проставляем пустой пароль (для формы авторизации)
            CurrentDetachedUser = _serviceProxy.GetDetachedEntity(AvitoUsers.FirstOrDefault(x => x.UserName == user.UserName));
        }
        #endregion


    }
}